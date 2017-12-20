using CustomNpcs.Npcs;
using Newtonsoft.Json;
using NLua;
using System;
using System.Collections.Generic;
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

		public List<ProjectileDefinition> Definitions { get; set; }
		ConditionalWeakTable<Projectile, CustomProjectile> customProjectiles;

		object locker = new object();
		object checkProjectileLock = new object();
		
		public ProjectileManager(CustomNpcsPlugin plugin)
		{
			this.plugin = plugin;

			customProjectiles = new ConditionalWeakTable<Projectile, CustomProjectile>();

			Utils.TryExecuteLua(LoadDefinitions, "ProjectileManager");

			GeneralHooks.ReloadEvent += OnReload;
			ServerApi.Hooks.GameUpdate.Register(plugin, onGameUpdate);
			//ServerApi.Hooks.ProjectileSetDefaults.Register(plugin, onProjectileSetDefaults);
			ServerApi.Hooks.ProjectileAIUpdate.Register(plugin, onProjectileAiUpdate);

			//OTAPI.Hooks.

			OTAPI.Hooks.Projectile.PostKilled = onProjectilePostKilled;
		}

		public void Dispose()
		{
			foreach (var def in Definitions)
			{
				def.Dispose();
			}
			Definitions.Clear();

			GeneralHooks.ReloadEvent -= OnReload;
			ServerApi.Hooks.GameUpdate.Deregister(plugin, onGameUpdate);
			//ServerApi.Hooks.ProjectileSetDefaults.Deregister(plugin, onProjectileSetDefaults);
			ServerApi.Hooks.ProjectileAIUpdate.Deregister(plugin, onProjectileAiUpdate);

			OTAPI.Hooks.Projectile.PostKilled = null;
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
				Debug.Print("SpawnCustomProjectile!");

				var baseOverride = definition.baseOverride;
				var projectileId = Projectile.NewProjectile(x, y, xSpeed, ySpeed, definition.BaseType, (int)baseOverride.Damage, (float)baseOverride.KnockBack, owner);
				var customProjectile =  projectileId != Main.maxProjectiles ? AttachCustomProjectile(Main.projectile[projectileId], definition) : null;
								
				if( customProjectile != null )
				{
					customProjectile.SendNetUpdate = true;

					TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", projectileId);
				}
				
				return customProjectile;
			}
		}
		
		public CustomProjectile GetCustomProjectile(Projectile projectile)
		{
			Debug.Assert(projectile != null, "projectile cannot be null.");

			CustomProjectile customProjectile = null;

			customProjectiles.TryGetValue(projectile, out customProjectile);

			return customProjectile;
		}

		private CustomProjectile AttachCustomProjectile(Projectile projectile, ProjectileDefinition definition)
		{
			Debug.Print("AttachCustomProjectile");
			
			var customProjectile = new CustomProjectile(projectile, definition);
			customProjectiles.Remove(projectile);
			customProjectiles.Add(projectile, customProjectile);

			definition.ApplyTo(projectile);

			lock( locker )
			{
				Utils.TryExecuteLua(() => definition.OnSpawn?.Call(customProjectile), definition.Name);
			}

			//TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", projectile.whoAmI);

			//// Ensure that all players see the changes.
			//var npcId = npc.whoAmI;
			//_checkNpcForReplacement[npcId] = false;
			//TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", npcId);
			//TSPlayer.All.SendData(PacketTypes.UpdateNPCName, "", npcId);
			//return customNpc;

			//TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", projectile.whoAmI);

			return customProjectile;
		}

		private void onGameUpdate(EventArgs args)
		{
			foreach(var projectile in Main.projectile)
			{
				var customProjectile = GetCustomProjectile(projectile);

				if(customProjectile!=null)
				{
					//game updates
					lock( locker )
					{
						var definition = customProjectile.Definition;
						Utils.TryExecuteLua(() => definition.OnGameUpdate?.Call(customProjectile), definition.Name);
					}

					//collision tests
					foreach(var player in TShock.Players)
					{
						if(player?.Active==true)
						{
							var tplayer = player.TPlayer;
							var playerHitbox = tplayer.Hitbox;
								
							if( !tplayer.immune && projectile.Hitbox.Intersects(playerHitbox) )
							{
								lock( locker )
								{
									var definition = customProjectile.Definition;
									Utils.TryExecuteLua(() => definition.OnCollision?.Call(customProjectile, player), definition.Name);
								}
							}
						}
					}
				}
			}
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

		private void onProjectileAiUpdate(ProjectileAiUpdateEventArgs args)
		{
			if (args.Handled)
				return;

			var customProjectile = GetCustomProjectile(args.Projectile);
			if (customProjectile != null)
			{
				//Debug.Print("CustomProjectile.OnProjectileAiUpdate!!");
				lock(locker)
				{
					var definition = customProjectile.Definition;
					var onAiUpdate = definition.OnAiUpdate;
					if (onAiUpdate != null)
					{
						Utils.TryExecuteLua(() => args.Handled = (bool)onAiUpdate.Call(customProjectile)[0], definition.Name);
					}
				}
			}
		}

		private void onProjectilePostKilled(Projectile projectile)
		{
			var customProjectile = GetCustomProjectile(projectile);
			if( customProjectile != null )
			{
				//Debug.Print("CustomProjectile.OnProjectilePostKilled!!");
				lock( locker )
				{
					var definition = customProjectile.Definition;
					Utils.TryExecuteLua(() => definition.OnKilled?.Call(customProjectile), definition.Name);

					customProjectiles.Remove(projectile);
				}
			}
		}

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
