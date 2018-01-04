using CustomNpcs.Npcs;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using NLua;
using OTAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace CustomNpcs.Projectiles
{
	public class ProjectileManager : IDisposable
	{
		public static ProjectileManager Instance { get; set; }

		public string ProjectilesPath { get; set; } = Path.Combine("npcs", "projectiles.json");

		CustomNpcsPlugin plugin;

		public List<ProjectileDefinition> Definitions { get; private set; }
		ConditionalWeakTable<Projectile, CustomProjectile> customProjectiles;
		
		object locker = new object();
		object checkProjectileLock = new object();
		
		public ProjectileManager(CustomNpcsPlugin plugin)
		{
			this.plugin = plugin;

			customProjectiles = new ConditionalWeakTable<Projectile, CustomProjectile>();
			
			Utils.TryExecuteLua(LoadDefinitions, "ProjectileManager");

			GeneralHooks.ReloadEvent += OnReload;
			//ServerApi.Hooks.GameUpdate.Register(plugin, onGameUpdate);
			//ServerApi.Hooks.ProjectileSetDefaults.Register(plugin, onProjectileSetDefaults);
			//ServerApi.Hooks.ProjectileAIUpdate.Register(plugin, onProjectileAiUpdate);

			OTAPI.Hooks.Projectile.PreUpdate = onProjectilePreUpdate;
			OTAPI.Hooks.Projectile.PreAI = onProjectilePreAi;
			OTAPI.Hooks.Projectile.PreKill = onProjectilePreKill;
			//OTAPI.Hooks.Projectile.PostKilled = onProjectilePostKilled;
		}

		public void Dispose()
		{
			foreach (var def in Definitions)
			{
				def.Dispose();
			}
			Definitions.Clear();

			GeneralHooks.ReloadEvent -= OnReload;
			//ServerApi.Hooks.GameUpdate.Deregister(plugin, onGameUpdate);
			//ServerApi.Hooks.ProjectileSetDefaults.Deregister(plugin, onProjectileSetDefaults);
			//ServerApi.Hooks.ProjectileAIUpdate.Deregister(plugin, onProjectileAiUpdate);

			OTAPI.Hooks.Projectile.PreUpdate = null;
			OTAPI.Hooks.Projectile.PreAI = null;
			OTAPI.Hooks.Projectile.PreKill = null;
			//OTAPI.Hooks.Projectile.PostKilled = null;
		}

		private void LoadDefinitions()
		{
			if(File.Exists(ProjectilesPath))
			{
				var definitions = JsonConvert.DeserializeObject<List<ProjectileDefinition>>(File.ReadAllText(ProjectilesPath));
				var failedDefinitions = new List<ProjectileDefinition>();
				foreach(var def in definitions)
				{
					try
					{
						def.ThrowIfInvalid();
						def.LoadLuaDefinition();
					}
					catch(FormatException ex)
					{
						TShock.Log.ConsoleError( $"[CustomNpcs] An error occurred while parsing CustomProjectile '{def.Name}': {ex.Message}");
						failedDefinitions.Add(def);
					}
				}

				Definitions = definitions.Except(failedDefinitions).ToList();
			}
			else
			{
				ServerApi.LogWriter.PluginWriteLine(plugin, $"Projectiles configuration does not exist. Expected config file to be at: {ProjectilesPath}", TraceLevel.Error);
				Definitions = new List<ProjectileDefinition>();
			}
		}

		public ProjectileDefinition FindDefinition(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			lock(locker)
			{
				return Definitions.FirstOrDefault(d => name.Equals(d.Name, StringComparison.OrdinalIgnoreCase));
			}
		}

		public CustomProjectile SpawnCustomProjectile(ProjectileDefinition definition, float x, float y, float xSpeed, float ySpeed, int owner = 255 )
		{
			// The following code needs to be synchronized since SpawnCustomNpc may run on a different thread than the
			// main thread. Otherwise, there is a possible race condition where the newly-spawned custom NPC may be
			// erroneously checked for replacement.
			lock(checkProjectileLock)
			{
				var baseOverride = definition.BaseOverride;
				var projectileId = Projectile.NewProjectile(x, y, xSpeed, ySpeed, definition.BaseType, (int)baseOverride.Damage, (float)baseOverride.KnockBack, owner);
				var customProjectile =  projectileId != Main.maxProjectiles ? AttachCustomProjectile(Main.projectile[projectileId], definition) : null;
								
				if( customProjectile != null )
				{
					//customProjectile.SendNetUpdate = true;
					TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", projectileId);
				}
				
				return customProjectile;
			}
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

			lock( locker )
			{
				var onSpawn = definition.OnSpawn;
				onSpawn?.Call(definition.Name, customProjectile);
			}
						
			//TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", projectile.whoAmI);

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

		private HookResult onProjectilePreUpdate(Projectile projectile, ref int index)
		{
			var result = HookResult.Continue;
			var customProjectile = GetCustomProjectile(projectile);

			if(customProjectile!=null)
			{
				var definition = customProjectile.Definition;

				//game updates
				lock( locker )
				{
					var onGameUpdate = definition.OnGameUpdate;
					if(onGameUpdate!=null)
					{
						var handled = onGameUpdate.Call(definition.Name, customProjectile).GetResult<bool>();
						result = handled == true ? HookResult.Cancel : HookResult.Continue;
					}
				}

				if( result == HookResult.Cancel )
				{
					//if we dont pass execution onto Terraria's Projectile.Update(), AI() will never get run, so we better run it ourselves.
					projectile.AI();
				}

				//try to update projectile
				if( Main.projectile[projectile.whoAmI] != null && projectile.active )
				{
					//projectile.netUpdate = true;
					TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", projectile.whoAmI);
				}

				//collision tests

				//tiles
				if(projectile.tileCollide)
				{
					var tileCollisions = TileFunctions.GetOverlappedTiles(projectile.Hitbox);

					if( tileCollisions.Count > 0 )
					{
						var onTileCollision = definition.OnTileCollision;
						if(onTileCollision!=null)
						{
							lock( locker )
							{
								onTileCollision.Call(definition.Name, customProjectile, tileCollisions);
							}
						}
					}
				}
								
				//players
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
								lock( locker )
								{
									onCollision.Call(definition.Name, customProjectile, player);
								}
							}
						}
					}
				}
			}
			
			return result;
		}

		private HookResult onProjectilePreAi(Projectile projectile)
		{
			var result = HookResult.Continue;//we usually let terraria handle ai
			var customProjectile = GetCustomProjectile(projectile);

			if( customProjectile != null )
			{
				lock( locker )
				{
					var definition = customProjectile.Definition;
					var onAiUpdate = definition.OnAiUpdate;
					if( onAiUpdate != null )
					{
						var handled = onAiUpdate.Call(definition.Name, customProjectile).GetResult<bool>();

						result = handled == true ? HookResult.Cancel : HookResult.Continue;
					}
				}
			}

			return result;
		}

		private HookResult onProjectilePreKill(Projectile projectile)
		{
			var customProjectile = GetCustomProjectile(projectile);
			if( customProjectile != null )
			{
				var definition = customProjectile.Definition;
				var onKilled = definition.OnKilled;
				if(onKilled!=null)
				{
					lock( locker )
					{
						onKilled.Call(definition.Name, customProjectile);

						customProjectiles.Remove(projectile);
						projectile.active = false;
						TSPlayer.All.SendData(PacketTypes.ProjectileDestroy, "", projectile.whoAmI);
					}
				}
				
				return HookResult.Cancel;
			}
			else
			{
				return HookResult.Continue;
			}
		}

		//private void onProjectilePostKilled(Projectile projectile)
		//{
		//	var customProjectile = GetCustomProjectile(projectile);
		//	if( customProjectile != null )
		//	{
		//		lock( locker )
		//		{
		//			var definition = customProjectile.Definition;
		//			Utils.TryExecuteLua(() => definition.OnKilled?.Call(customProjectile), definition.Name);

		//			customProjectiles.Remove(projectile);
		//			projectile.active = false;
		//			TSPlayer.All.SendData(PacketTypes.ProjectileDestroy, "", projectile.whoAmI);
		//		}
		//	}
		//}

		private void OnReload(ReloadEventArgs args)
		{
			lock(locker)
			{
				foreach (var definition in Definitions)
				{
					definition.Dispose();
				}
				Definitions.Clear();

				Utils.TryExecuteLua(LoadDefinitions, "ProjectileManager");
			}
			args.Player.SendSuccessMessage("[CustomNpcs] Reloaded Projectiles!");
		}
	}
}
