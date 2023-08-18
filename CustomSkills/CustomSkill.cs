using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
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
		internal DateTime CastStartTime;
		internal DateTime ChargeStartTime;
		internal DateTime ChargeIntervalStartTime;
		//DateTime CooldownStartTime;
		internal Vector2 StartLocation;
		internal bool NotifyUserOnCooldown => Definition.NotifyUserOnCooldown;
		internal SkillState SkillState { get; private set; }

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
			SkillState = new SkillState(this);

			if (levelIndex < 0 || levelIndex >= skillDefinition.Levels.Count)
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
			if (!Player.ConnectionAlive)
			{
				Phase = SkillPhase.Failed;
				return;
			}

			switch (Phase)
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

				case SkillPhase.Cancelled:
					RunCancelled();
					break;
			}
		}

		void RunOnCast()
		{
			try
			{
				//Debug.Print($"Casting {Definition.Name}.");
				var levelDef = LevelDefinition;
				var session = Session.GetOrCreateSession(Player);

				var elapsed = DateTime.Now - CastStartTime;
				var completed = (float)(elapsed.TotalMilliseconds / levelDef.CastingDuration.TotalMilliseconds) * 100.0f;
				completed = Math.Min(completed, 100.0f);

				//if(!session.TryDeductCost(Player,levelDef.CastingCost))
				//{
				//	Debug.Print("Player was unable to afford casting cost, after casting started. Ignoring.");
				//}

				SkillState.Progress = completed;
				SkillState.ElapsedTime = elapsed;

				if (levelDef.OnCast?.Invoke(Player, SkillState) == false)
				{
					Phase = SkillPhase.Cancelled;
					return;
				}

				if (completed >= 100.0f)
				{
					ChargeStartTime = DateTime.Now;
					ChargeIntervalStartTime = ChargeStartTime;
					Phase = SkillPhase.Charging;
				}
			}
			catch (Exception ex)
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

				if (!levelDef.CanCasterMove && HasCasterMoved())
				{
					Phase = SkillPhase.Cancelled;
					return;
				}

				if (levelDef.ChargingCostInterval > TimeSpan.Zero)
				{
					var now = DateTime.Now;

					//time to deduct?
					var elapsedChargeInterval = now - ChargeIntervalStartTime;

					if (elapsedChargeInterval >= levelDef.ChargingCostInterval)
					{
						//reset the deduction timer... and account for overages...
						ChargeIntervalStartTime = now - (elapsedChargeInterval - levelDef.ChargingCostInterval);

						var session = Session.GetOrCreateSession(Player);

						//Debug.Print($"Deducting charging cost.");

						if (!session.TryDeductCost(session.Player, levelDef.ChargingCost))
						{
							Phase = levelDef.IsLongRunning ? SkillPhase.Firing : SkillPhase.Cancelled;
							return;
						}
					}
				}

				//Debug.Print($"Charging {Definition.Name}. ({completed}%)");

				SkillState.Progress = completed;
				SkillState.ElapsedTime = elapsed;

				if (levelDef.IsLongRunning)
				{
					if (levelDef.OnCharge?.Invoke(Player, SkillState) == false)
					{
						Phase = SkillPhase.Firing;
						return;
					}
				}
				else
				{
					if (levelDef.OnCharge?.Invoke(Player, SkillState) == false)
					{
						Phase = SkillPhase.Cancelled;
						return;
					}

					if (completed >= 100.0f)
						Phase = SkillPhase.Firing;
				}
			}
			catch (Exception ex)
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

				if (!levelDef.CanCasterMove && HasCasterMoved())
				{
					Phase = SkillPhase.Cancelled;
					return;
				}

				SkillState.Progress = 100f;
				SkillState.ElapsedTime = new TimeSpan(0);

				levelDef.OnFire?.Invoke(Player, SkillState);
				//only fires once, but should spark something that can continue for some time afterwards.
				//check if we moved up a level..

				var session = Session.GetOrCreateSession(Player);

				if (session.PlayerSkillInfos.TryGetValue(Definition.Name, out var playerSkillInfo))
				{
					playerSkillInfo.CurrentUses++;

					if (Definition.CanLevelUp(playerSkillInfo.CurrentLevel))
					{
						if (playerSkillInfo.CurrentUses >= levelDef.UsesToLevelUp)
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
			catch (Exception ex)
			{
				Phase = SkillPhase.Failed;
				throw ex;
			}
		}

		private void RunCancelled()
		{
			//Debug.Print($"Skill {Definition.Name} was Cancelled.");

			try
			{
				var session = Session.GetOrCreateSession(Player);
				var levelDef = LevelDefinition;

				levelDef.OnCancelled?.Invoke(Player, SkillState);

				SkillState.Emitters.Destroy();

				session.PlayerSkillInfos[Definition.Name].CooldownStartTime = DateTime.Now;
				Phase = SkillPhase.Failed;
			}
			catch (Exception ex)
			{
				Phase = SkillPhase.Failed;
				throw ex;
			}
		}
	}
}
