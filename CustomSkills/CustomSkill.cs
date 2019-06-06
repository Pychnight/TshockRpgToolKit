using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomSkills
{
	/// <summary>
	/// Represents a CustomSkill, that is currently "in action".
	/// </summary>
	internal class CustomSkill
	{
		internal string PlayerName { get; set; }
		internal TSPlayer Player { get; set; }
		internal CustomSkillDefinition Definition { get; set; }
		internal int LevelIndex { get; set; }
		internal CustomSkillLevelDefinition LevelDefinition => Definition.Levels[LevelIndex];
		internal SkillPhase Phase { get; set; }
		DateTime ChargeStartTime;
		//DateTime CooldownStartTime;
		internal Vector2 StartLocation;
        internal bool NotifyUserOnCooldown => Definition.NotifyUserOnCooldown;
        		
		internal CustomSkill()
		{
		}

		internal CustomSkill(TSPlayer player, CustomSkillDefinition skillDefinition, int levelIndex)
		{
			PlayerName = player.Name;
			Player = player;
			Definition = skillDefinition;
			LevelIndex = levelIndex;
			StartLocation = new Vector2(player.X, player.Y);

			if(levelIndex < 0 || levelIndex >= skillDefinition.Levels.Count)
				throw new ArgumentOutOfRangeException($"{nameof(levelIndex)}");
		}

		internal bool HasCasterMoved()
		{
			var currentLocation = new Vector2(Player.X, Player.Y);

			return currentLocation != StartLocation;
		}

        internal bool HasCooldownCompleted()
        {
			var session = Session.GetOrCreateSession(Player);
			var skillInfo = session.PlayerSkillInfos[Definition.Name];
			var result = (DateTime.Now - skillInfo.CooldownStartTime) >= LevelDefinition.CastingCooldown;
			
            return result;
        }
		
		internal void Update()
		{
			if(!Player.ConnectionAlive)
			{
				Phase = SkillPhase.Failed;
				return;
			}

			switch(Phase)
			{
				case SkillPhase.Casting:
					RunOnCast();
					break;

				case SkillPhase.Charging:
					RunOnCharging();
					break;

				case SkillPhase.Firing:
					RunOnFiring();
					break;

				//case SkillPhase.Cooldown:
				//	RunCooldown();
				//	break;

				case SkillPhase.Cancelled:
					RunCancelled();
					break;
			}
		}
		
		void RunOnCast()
		{
			try
			{
				var levelDef = LevelDefinition;

				//Debug.Print($"Casting {Definition.Name}.");

				levelDef.OnCast?.Invoke(Player);
				//only fires one time

				ChargeStartTime = DateTime.Now;
				Phase = SkillPhase.Charging;
			}
			catch(Exception ex)
			{
				Phase = SkillPhase.Failed;
				throw ex;
			}
		}

		void RunOnCharging()
		{
			try
			{
				var levelDef = LevelDefinition;

				var elapsed = DateTime.Now - ChargeStartTime;
				var completed = (float)(elapsed.TotalMilliseconds / levelDef.ChargingDuration.TotalMilliseconds) * 100.0f;
				completed = Math.Min(completed, 100.0f);
				
				//Debug.Print($"Charging {Definition.Name}. ({completed}%)");

				if(!levelDef.CanCasterMove && HasCasterMoved())
				{
					Phase = SkillPhase.Cancelled;
					return;
				}

				levelDef.OnCharge?.Invoke(Player,completed);

				//can fire continuously...
				//if(DateTime.Now - ChargeStartTime >= levelDef.ChargingDuration)
				//	Phase = SkillPhase.Firing;
				if(completed >= 100.0f)
					Phase = SkillPhase.Firing;
			}
			catch(Exception ex)
			{
				Phase = SkillPhase.Failed;
				throw ex;
			}
		}

		void RunOnFiring()
		{
			try
			{
				var levelDef = LevelDefinition;

				//Debug.Print($"Firing {Definition.Name}.");

				if(!levelDef.CanCasterMove && HasCasterMoved())
				{
					Phase = SkillPhase.Cancelled;
					return;
				}
				
				levelDef.OnFire?.Invoke(Player);
				//only fires once, but should spark something that can continue for some time afterwards.
				//check if we moved up a level..

				var session = Session.GetOrCreateSession(Player);
				
				if(session.PlayerSkillInfos.TryGetValue(Definition.Name, out var playerSkillInfo))
				{
					playerSkillInfo.CurrentUses++;

					if(Definition.CanLevelUp(playerSkillInfo.CurrentLevel))
					{
						if(playerSkillInfo.CurrentUses >= levelDef.UsesToLevelUp)
						{
							//level up
							playerSkillInfo.CurrentLevel++;
							playerSkillInfo.CurrentUses = 0;

							var nextLevelDef = Definition.Levels[playerSkillInfo.CurrentLevel];
							nextLevelDef.OnLevelUp?.Invoke(Player);
						}
					}

					playerSkillInfo.CooldownStartTime = DateTime.Now;
					Phase = SkillPhase.Completed;
				}
				else
				{
					Phase = SkillPhase.Failed;
					throw new KeyNotFoundException($"Tried to get PlayerSkillInfo for key '{Definition.Name}', but none was found.");
				}
			}
			catch(Exception ex)
			{
				Phase = SkillPhase.Failed;
				throw ex;
			}
		}

		//void RunCooldown()
		//{
		//	try
		//	{
		//		var levelDef = LevelDefinition;

		//		//Debug.Print($"Cooling down {Definition.Name}.");

		//		if(DateTime.Now - CooldownStartTime >= levelDef.CastingCooldown)
		//			Phase = SkillPhase.Completed;
		//	}
		//	catch(Exception ex)
		//	{
		//		Phase = SkillPhase.Failed;
		//		throw ex;
		//	}
		//}

		private void RunCancelled()
		{
			//Debug.Print($"Skill {Definition.Name} was Cancelled.");

			try
			{
				var session = Session.GetOrCreateSession(Player);
				var levelDef = LevelDefinition;
				
				levelDef.OnCancelled?.Invoke(Player);

				session.PlayerSkillInfos[Definition.Name].CooldownStartTime = DateTime.Now;
				Phase = SkillPhase.Failed;
			}
			catch(Exception ex)
			{
				Phase = SkillPhase.Failed;
				throw ex;
			}
		}
	}
}
