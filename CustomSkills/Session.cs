using Banking;
using Corruption.PluginSupport;
using CustomSkills.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using TShockAPI;

namespace CustomSkills
{
	/// <summary>
	/// Represents a logged in players current skill state. 
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class Session
	{
		internal static ConcurrentDictionary<string, Session> ActiveSessions { get; private set; } = new ConcurrentDictionary<string, Session>();

		/// <summary>
		/// Gets the CustomSkillsPlugin.Instance.SessionRepository
		/// </summary>
		internal static ISessionDatabase SessionRepository => CustomSkillsPlugin.Instance.SessionRepository;

		//Player can be wiped by the time we save(), so its best to store the name string.
		public string PlayerName { get; set; }

		public TSPlayer Player { get; set; }

		/// <summary>
		/// Dictionary of all learned skills, and their status.
		/// </summary>
		[JsonProperty(Order = 0)]
		internal Dictionary<string, PlayerSkillInfo> PlayerSkillInfos { get; set; } = new Dictionary<string, PlayerSkillInfo>();

		//DO NOT PERSIST THIS... it should be regenerated on join/reload...
		/// <summary>
		/// Maps trigger words to skills.
		/// </summary>
		internal Dictionary<string, CustomSkillDefinition> TriggerWordsToSkillDefinitions { get; set; } = new Dictionary<string, CustomSkillDefinition>();

		public Session() { }

		public Session(TSPlayer player)
		{
			PlayerName = player.Name;
			Player = player;
		}

		public static Session GetOrCreateSession(TSPlayer player)
		{
			if (!ActiveSessions.TryGetValue(player.Name, out var playerSession))
			{
				//session is not active, try to get it from the db
				playerSession = SessionRepository.Load(player.Name);

				if (playerSession == null)
				{
					//otherwise, create new
					playerSession = new Session(player);
				}
				else
				{
					//update all trigger words
					foreach (var skillName in playerSession.PlayerSkillInfos.Keys)
						playerSession.UpdateTriggerWords(skillName);

					//have to re-add these
					playerSession.Player = player;
					playerSession.PlayerName = player.Name;
				}

				ActiveSessions.TryAdd(player.Name, playerSession);
			}

			return playerSession;
		}

		public static void SaveAll()
		{
			foreach (var session in ActiveSessions.Values)
				session?.Save();
		}

		public void Save()
		{
			try
			{
				SessionRepository.Save(PlayerName, this);
			}
			catch (Exception ex)
			{
				CustomSkillsPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Error);
			}
		}

		/// <summary>
		/// Attempts to get both the requested skill definition, and its current level definition for the current session.
		/// </summary>
		/// <param name="skillName"></param>
		/// <param name="skillDefinition"></param>
		/// <param name="levelDefinition"></param>
		/// <returns>True if both skill and level could be retrieved</returns>
		internal bool TryGetSkillAndLevel(string skillName, out CustomSkillDefinition skillDefinition, out CustomSkillLevelDefinition levelDefinition)
		{
			skillDefinition = CustomSkillsPlugin.Instance.CustomSkillDefinitionLoader.TryGetDefinition(skillName);
			levelDefinition = null;

			if (skillDefinition != null)
			{
				if (PlayerSkillInfos.TryGetValue(skillName, out var skillInfo))
				{
					levelDefinition = skillDefinition.Levels[skillInfo.CurrentLevel];
					return true;
				}
			}

			return false;
		}

		public bool HasLearned(string skillName) => PlayerSkillInfos.ContainsKey(skillName);

		public bool LearnSkill(string skillName)
		{
			if (!HasLearned(skillName))
			{
				PlayerSkillInfos.Add(skillName, new PlayerSkillInfo());
				UpdateTriggerWords(skillName);
				return true;
			}

			return false;
		}

		public bool IsSkillReady(string skillName)
		{
			var definition = CustomSkillsPlugin.Instance.CustomSkillDefinitionLoader.TryGetDefinition(skillName);

			if (definition != null)
			{
				if (PlayerSkillInfos.TryGetValue(skillName, out var skillInfo))
				{
					var levelDef = definition.Levels[skillInfo.CurrentLevel];

					return (DateTime.Now - skillInfo.CooldownStartTime) > levelDef.CastingCooldown;
				}
			}

			return false;
		}

		public bool CanAfford(TSPlayer player, CustomSkillCost cost)
		{
			if (cost == null)
				return true;
			else
			{
				if (cost.RequiresHp)
				{
					if (Player.TPlayer.statLife < cost.Hp)
						return false;
				}

				if (cost.RequiresMp)
				{
					if (Player.TPlayer.statMana < cost.Mp)
						return false;
				}

				var bank = BankingPlugin.Instance.Bank;
				if (bank == null)
					return true;//probably an error condition, but let the admin deal with this.

				if (cost.RequiresExp)
				{
					var account = BankingPlugin.Instance.GetBankAccount(player, "Exp");
					if (account != null)
					{
						if (account.Balance < Math.Abs(cost.Exp))
							return false;//player cant cover balance
					}
					else
						return false;
				}

				if (cost.RequiresCurrency)
				{
					if (bank.CurrencyManager.TryFindCurrencyFromString(cost.Currency, out var currency))
					{
						var account = bank.GetBankAccount(Player.Name, currency.InternalName);

						if (account != null)
						{
							//check for funds...
							Debug.Print($"Found bank account for {currency.InternalName}");

							//parse
							currency.GetCurrencyConverter().TryParse(cost.Currency, out var rawValue);

							if (account.Balance < rawValue)
							{
								Debug.Print("Player bank balance is less than skill cost.");
								return false;
							}

							//account.TryWithdraw(rawValue)
						}
						else
							Debug.Print($"Did not find bank account for {currency.InternalName}");
					}
				}

				return true;
			}
		}

		public bool TryDeductCost(TSPlayer player, CustomSkillCost cost)
		{
			if (!CanAfford(player, cost))
				return false;

			if (cost == null)
				return true;
			else
			{
				var tPlayer = Player.TPlayer;

				if (cost.RequiresHp)
				{
					tPlayer.statLife = Math.Max(0, tPlayer.statLife - cost.Hp);

					//send player life packet
					TSPlayer.All.SendData(PacketTypes.PlayerHp, "", Player.Index);//, tPlayer.statLife, tPlayer.statLifeMax );
				}

				if (cost.RequiresMp)
				{
					tPlayer.statMana = Math.Max(0, tPlayer.statMana - cost.Mp);

					//send player mana packet
					TSPlayer.All.SendData(PacketTypes.PlayerMana, "", Player.Index);//, tPlayer.statMana, tPlayer.statManaMax);
				}

				var bank = BankingPlugin.Instance.Bank;
				if (bank == null)
					return true;//this probably is an error condition, but for now we just get out of dodge.

				if (cost.RequiresExp)
				{
					var account = BankingPlugin.Instance.GetBankAccount(player, "Exp");
					if (account != null)
					{
						var result = account.TryWithdraw(Math.Abs(cost.Exp), WithdrawalMode.RequireFullBalance);
						if (result == false)//player cant cover balance
							return false;
					}
					else
						return false;
				}

				if (cost.RequiresCurrency)
				{
					if (bank.CurrencyManager.TryFindCurrencyFromString(cost.Currency, out var currency))
					{
						var account = bank.GetBankAccount(Player.Name, currency.InternalName);
						if (account != null)
						{
							//check for funds...
							Debug.Print($"Found bank account for {currency.InternalName}");

							//parse
							currency.GetCurrencyConverter().TryParse(cost.Currency, out var rawValue);

							if (!account.TryWithdraw(rawValue, WithdrawalMode.RequireFullBalance))
							{
								Debug.Print("Player bank balance is less than skill cost.");
								return false;
							}
						}
						else
							Debug.Print($"Did not find bank account for {currency.InternalName}");
					}
				}

				return true;
			}
		}

		public bool CanAffordCastingSkill(string skillName)
		{
			if (TryGetSkillAndLevel(skillName, out var skillDef, out var levelDef))
				return CanAfford(Player, levelDef.CastingCost);

			return false;
		}

		public bool CanAffordChargingSkill(string skillName)
		{
			if (TryGetSkillAndLevel(skillName, out var skillDef, out var levelDef))
				return CanAfford(Player, levelDef.ChargingCost);

			return false;
		}

		private void UpdateTriggerWords(string skillName)
		{
			if (CustomSkillsPlugin.Instance.CustomSkillDefinitionLoader.TriggeredDefinitions.TryGetValue(skillName, out var skill))
			{
				foreach (var word in skill.TriggerWords)
					TriggerWordsToSkillDefinitions[word] = skill;
			}
		}
	}
}
