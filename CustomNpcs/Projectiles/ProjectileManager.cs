﻿using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using BooTS;
using Corruption;
using Corruption.PluginSupport;
using CustomNpcs.Npcs;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using OTAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace CustomNpcs.Projectiles
{
	public class ProjectileManager : CustomTypeManager<ProjectileDefinition>, IDisposable
	{
		public static ProjectileManager Instance { get; set; }
		CustomNpcsPlugin plugin;
		//public List<ProjectileDefinition> Definitions { get; private set; }
		ConditionalWeakTable<Projectile, CustomProjectile> customProjectiles;
		
		public ProjectileManager(CustomNpcsPlugin plugin)
		{
			this.plugin = plugin;

			BasePath = "npcs";
			ConfigPath = Path.Combine(BasePath, "projectiles.json");
			AssemblyNamePrefix = "Projectile_";
			
			customProjectiles = new ConditionalWeakTable<Projectile, CustomProjectile>();

			LoadDefinitions();

			GeneralHooks.ReloadEvent += OnReload;
			//ServerApi.Hooks.GameUpdate.Register(plugin, onGameUpdate);
			//ServerApi.Hooks.ProjectileSetDefaults.Register(plugin, onProjectileSetDefaults);
			//ServerApi.Hooks.ProjectileAIUpdate.Register(plugin, onProjectileAiUpdate);

			OTAPI.Hooks.Projectile.PreUpdate = OnProjectilePreUpdate;
			OTAPI.Hooks.Projectile.PreAI = OnProjectilePreAi;
			OTAPI.Hooks.Projectile.PreKill = OnProjectilePreKill;
		}

		public void Dispose()
		{
			ClearDefinitions();

			GeneralHooks.ReloadEvent -= OnReload;
			//ServerApi.Hooks.GameUpdate.Deregister(plugin, onGameUpdate);
			//ServerApi.Hooks.ProjectileSetDefaults.Deregister(plugin, onProjectileSetDefaults);
			//ServerApi.Hooks.ProjectileAIUpdate.Deregister(plugin, onProjectileAiUpdate);

			OTAPI.Hooks.Projectile.PreUpdate = null;
			OTAPI.Hooks.Projectile.PreAI = null;
			OTAPI.Hooks.Projectile.PreKill = null;
		}

		protected override IEnumerable<EnsuredMethodSignature> GetEnsuredMethodSignatures()
		{
			var sigs = new List<EnsuredMethodSignature>()
			{
				new EnsuredMethodSignature("OnSpawn")
					.AddParameter("projectile",typeof(CustomProjectile)),

				new EnsuredMethodSignature("OnKilled")
					.AddParameter("projectile",typeof(CustomProjectile)),

				new EnsuredMethodSignature("OnGameUpdate",typeof(bool))
					.AddParameter("projectile",typeof(CustomProjectile)),

				new EnsuredMethodSignature("OnAiUpdate",typeof(bool))
					.AddParameter("projectile",typeof(CustomProjectile)),

				new EnsuredMethodSignature("OnCollision")
					.AddParameter("projectile",typeof(CustomProjectile))
					.AddParameter("player",typeof(TSPlayer)),

				new EnsuredMethodSignature("OnTileCollision")
					.AddParameter("projectile",typeof(CustomProjectile))
					.AddParameter("tileHits",typeof(List<Point>))
			};

			return sigs;
		}
		
		protected override void LoadDefinitions()
		{
			CustomNpcsPlugin.Instance.LogPrint($"Loading CustomProjectiles...", TraceLevel.Info);
			base.LoadDefinitions();
		}

		public static void SendProjectileUpdate(int index)
		{
			TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", index);
			//Debug.Print($"Sent projectile new or update for index #{index}!");
		}

		public static void SendProjectileKill(int index, int owner = 255)
		{
			TSPlayer.All.SendData(PacketTypes.ProjectileDestroy, "", index, owner);
			//Debug.Print($"Sent projectile destroy for index #{index}!");
		}

		public CustomProjectile SpawnCustomProjectile(ProjectileDefinition definition, float x, float y, float xSpeed, float ySpeed, int owner = 255 )
		{
			var baseOverride = definition.BaseOverride;
			var projectileId = Projectile.NewProjectile(x, y, xSpeed, ySpeed, definition.BaseType, (int)baseOverride.Damage, (float)baseOverride.KnockBack, owner);
			var customProjectile = projectileId != Main.maxProjectiles ? AttachCustomProjectile(Main.projectile[projectileId], definition) : null;

			if( customProjectile != null )
			{
				customProjectile.OldPosition = new Vector2(x, y);
				//customProjectile.SendNetUpdate = true;
				SendProjectileUpdate(projectileId);
				//Debug.Print($"Sent initial projectile for index #{projectileId}!");
			}

			return customProjectile;
		}
		
		public CustomProjectile GetCustomProjectile(Projectile projectile)
		{
			CustomProjectile customProjectile = null;

			Debug.Assert(projectile != null, "projectile cannot be null.");
			if(projectile!=null)
			{
				customProjectiles.TryGetValue(projectile, out customProjectile);
			}

			return customProjectile;
		}

		private CustomProjectile AttachCustomProjectile(Projectile projectile, ProjectileDefinition definition)
		{
			var customProjectile = new CustomProjectile(projectile, definition);
			customProjectiles.Remove(projectile);
			customProjectiles.Add(projectile, customProjectile);

			definition.ApplyTo(projectile);
			
			try
			{
				CustomIDFunctions.CurrentID = definition.Name;
				definition.OnSpawn?.Invoke(customProjectile);
			}
			catch(Exception ex)
			{
				Utils.LogScriptRuntimeError(ex);
				definition.OnSpawn = null;
			}
			
			return customProjectile;
		}
		
		//private void onProjectileSetDefaults(SetDefaultsEventArgs<Projectile,int> args)
		//{
		//	var projectile = args.Object;
			
		//	var customProjectile = GetCustomProjectile(projectile);
		//	if(customProjectile!=null)
		//	{
		//		Debug.Print("onProjectileSetDefaults!");
		//		var definition = customProjectile.Definition;

		//		definition.ApplyTo(projectile);

		//		lock( locker )
		//		{
		//			Utils.TryExecuteLua(() => definition.OnSpawn?.Call(customProjectile), definition.Name);
		//		}

		//		TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", projectile.whoAmI);

		//		args.Handled = true;
		//	}
		//}
		
		private HookResult OnProjectilePreUpdate(Projectile projectile, ref int index)
		{
			var result = HookResult.Continue;
			var customProjectile = GetCustomProjectile(projectile);

			if( customProjectile == null )
				return HookResult.Continue;
						
			var definition = customProjectile.Definition;
			var lastTimeLeft = projectile.timeLeft;	//we capture this to help determine whether we need to decrement timeLeft at the end of this method.
													//terraria has many points where it sets timeLeft internally, but our custom proj modifies whether/when those points run.
													//by the end of this method we hopefully have enough information to tell if terraria modified it, or if we need to do it ourselves.

			//game updates
			var onGameUpdate = definition.OnGameUpdate;
			if( customProjectile.Active && onGameUpdate != null )
			{
				try
				{
					CustomIDFunctions.CurrentID = definition.Name;
					var handled = onGameUpdate(customProjectile);
					result = handled == true ? HookResult.Cancel : HookResult.Continue;

					if( result == HookResult.Cancel )
					{
						//if we dont pass execution onto Terraria's Projectile.Update(), AI() will never get run, so we better run it ourselves.
						projectile.AI();
					}
					
					customProjectile.SendNetUpdate = true;
				}
				catch( Exception ex )
				{
					Utils.LogScriptRuntimeError(ex);
					definition.OnGameUpdate = null;
				}
			}

			//collision tests
			
			//players
			if( customProjectile.Active && definition.OnCollision != null )
			{
				foreach( var player in TShock.Players )
				{
					if( player?.Active == true )
					{
						var onCollision = definition.OnCollision;
						if( onCollision != null )
						{
							var tplayer = player.TPlayer;
							var playerHitbox = tplayer.Hitbox;

							if( !tplayer.immune && projectile.Hitbox.Intersects(playerHitbox) )
							{
								try
								{
									CustomIDFunctions.CurrentID = definition.Name;
									onCollision(customProjectile, player);
									customProjectile.SendNetUpdate = true;
								}
								catch( Exception ex )
								{
									Utils.LogScriptRuntimeError(ex);
									definition.OnCollision = null;
								}
							}
						}
					}
				}
			}

			//tiles
			if( customProjectile.Active && projectile.tileCollide )
			{
				// this is a bit convoluted, because of the 2 conditions-- player wants to run custom code on tile collisions and/or player isn't allowing terraria
				// to run Update(), thus the projectile wont be killed in a timely manner. See condition below for result == HookResult.Cancel
				if( definition.OnTileCollision != null || result == HookResult.Cancel )
				{
					var tileCollisions = TileFunctions.GetOverlappedTiles(projectile.Hitbox);

					if( tileCollisions.Count > 0 )
					{
						var killProjectile = false; 
												
						//if terrarias code won't be running Update(and thus AI() ), we should kill the projectile ourselves if we hit any applicable tile.
						if( result != HookResult.Continue )
						{
							//...we have to scan the list before the player does, to ensure they dont modify anything(we shouldn't have switched from ReadOnlyCollection. )
							foreach( var hit in tileCollisions )
							{
								if( TileFunctions.IsSolidOrSlopedTile(hit.X, hit.Y) ||
									( !( definition.BaseOverride.IgnoreWater == true ) && TileFunctions.IsLiquidTile(hit.X, hit.Y) ) )
								{
									killProjectile = true;
									break;
								}
							}
						}
						
						try
						{
							CustomIDFunctions.CurrentID = definition.Name;
							definition.OnTileCollision?.Invoke(customProjectile, tileCollisions);
							//customProjectile.SendNetUpdate = true;
						}
						catch( Exception ex )
						{
							Utils.LogScriptRuntimeError(ex);
							definition.OnTileCollision = null;
						}

						//script hasnt killed projectile, but we did hit a foreground tile, so lets kill it ourselves
						if( customProjectile.Active && killProjectile == true )
							customProjectile.Kill();
					}
				}
			}

			//We need to decrement timeLeft ourselves if no other code has, and no code run after this point will do so
			if(customProjectile.Active && result == HookResult.Cancel && customProjectile.TimeLeft==lastTimeLeft )
			{
				customProjectile.TimeLeft--;

				if( customProjectile.TimeLeft < 1 )
					customProjectile.Kill();
			}

			if(customProjectile.Active && customProjectile.SendNetUpdate)
			{
				SendProjectileUpdate(customProjectile.Index);
			}
			
			return result;
		}
		
		private HookResult OnProjectilePreAi(Projectile projectile)
		{
			var result = HookResult.Continue;//we usually let terraria handle ai
			var customProjectile = GetCustomProjectile(projectile);

			if( customProjectile != null )
			{
				var onAiUpdate = customProjectile.Definition.OnAiUpdate;
				if( onAiUpdate != null )
				{
					try
					{
						CustomIDFunctions.CurrentID = customProjectile.Definition.Name;
						var handled = onAiUpdate(customProjectile);
						result = handled == true ? HookResult.Cancel : HookResult.Continue;
					}
					catch(Exception ex)
					{
						Utils.LogScriptRuntimeError(ex);
						customProjectile.Definition.OnAiUpdate = null;
					}
				}
			}

			return result;
		}

		private HookResult OnProjectilePreKill(Projectile projectile)
		{
			var customProjectile = GetCustomProjectile(projectile);
			if( customProjectile != null )
			{
				var definition = customProjectile.Definition;
				var onKilled = definition.OnKilled;
				if(onKilled!=null)
				{
					try
					{
						CustomIDFunctions.CurrentID = definition.Name;
						onKilled(customProjectile);
					}
					catch(Exception ex)
					{
						Utils.LogScriptRuntimeError(ex);
						definition.OnKilled = null;
					}
					
					customProjectiles.Remove(projectile);
					projectile.active = false;
					SendProjectileKill(customProjectile.Index, customProjectile.Owner);

					return HookResult.Cancel;
				}
				else
				{
					customProjectiles.Remove(projectile);
					return HookResult.Continue;
				}
			}
			else
			{
				return HookResult.Continue;
			}
		}
		
		private void OnReload(ReloadEventArgs args)
		{
			ClearDefinitions();

			LoadDefinitions();
			
			args.Player.SendSuccessMessage("[CustomNpcs] Reloaded Projectiles!");
		}
	}
}
