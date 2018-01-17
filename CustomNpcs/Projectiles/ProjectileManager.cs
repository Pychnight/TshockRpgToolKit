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
using System.Reflection;
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

		public static readonly string ProjectilesBasePath = "npcs";
		public static readonly string ProjectilesConfigPath = Path.Combine(ProjectilesBasePath, "projectiles.json");

		CustomNpcsPlugin plugin;

		public List<ProjectileDefinition> Definitions { get; private set; }
		ConditionalWeakTable<Projectile, CustomProjectile> customProjectiles;
		Assembly projectileScriptsAssembly;
				
		public ProjectileManager(CustomNpcsPlugin plugin)
		{
			this.plugin = plugin;

			customProjectiles = new ConditionalWeakTable<Projectile, CustomProjectile>();
						
			LoadDefinitions();

			GeneralHooks.ReloadEvent += OnReload;
			//ServerApi.Hooks.GameUpdate.Register(plugin, onGameUpdate);
			//ServerApi.Hooks.ProjectileSetDefaults.Register(plugin, onProjectileSetDefaults);
			//ServerApi.Hooks.ProjectileAIUpdate.Register(plugin, onProjectileAiUpdate);

			OTAPI.Hooks.Projectile.PreUpdate = onProjectilePreUpdate;
			OTAPI.Hooks.Projectile.PreAI = onProjectilePreAi;
			OTAPI.Hooks.Projectile.PreKill = onProjectilePreKill;
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
		}

		private void LoadDefinitions()
		{
			if(File.Exists(ProjectilesConfigPath))
			{
				var booScripts = new List<string>();

				var definitions = JsonConvert.DeserializeObject<List<ProjectileDefinition>>(File.ReadAllText(ProjectilesConfigPath));
				var failedDefinitions = new List<ProjectileDefinition>();
				foreach(var def in definitions)
				{
					try
					{
						def.ThrowIfInvalid();
					}
					catch(FormatException ex)
					{
						CustomNpcsPlugin.Instance.LogPrint($"An error occurred while parsing CustomProjectile '{def.Name}': {ex.Message}", TraceLevel.Error);
						failedDefinitions.Add(def);
					}

					var rootedScriptPath = Path.Combine(ProjectilesBasePath, def.ScriptPath);

					if( !string.IsNullOrWhiteSpace(def.ScriptPath) )
					{
						Debug.Print($"Added projectile script '{def.ScriptPath}'.");
						booScripts.Add(rootedScriptPath);
					}
				}

				Definitions = definitions.Except(failedDefinitions).ToList();

				if( booScripts.Count > 0 )
				{
					Debug.Print($"Compiling boo projectile scripts.");
					projectileScriptsAssembly = BooScriptCompiler.Compile("ScriptedProjectiles.dll", booScripts);

					if( projectileScriptsAssembly != null )
					{
						Debug.Print($"Compilation succeeded.");

						foreach( var d in Definitions )
							d.LinkToScript(projectileScriptsAssembly);
					}
					else
						Debug.Print($"Compilation failed.");
				}
			}
			else
			{
				ServerApi.LogWriter.PluginWriteLine(plugin, $"Projectiles configuration does not exist. Expected config file to be at: {ProjectilesConfigPath}", TraceLevel.Error);
				Definitions = new List<ProjectileDefinition>();
			}
		}

		public ProjectileDefinition FindDefinition(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			return Definitions.FirstOrDefault(d => name.Equals(d.Name, StringComparison.OrdinalIgnoreCase));
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
				//customProjectile.SendNetUpdate = true;
				ProjectileManager.SendProjectileUpdate(projectileId);
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
			
			definition.OnSpawn?.Invoke(customProjectile);

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
				var onGameUpdate = definition.OnGameUpdate;
				if( onGameUpdate != null )
				{
					var handled = onGameUpdate(customProjectile);
					result = handled == true ? HookResult.Cancel : HookResult.Continue;
				}

				if( result == HookResult.Cancel )
				{
					//if we dont pass execution onto Terraria's Projectile.Update(), AI() will never get run, so we better run it ourselves.
					projectile.AI();
				}

				//try to update projectile	
				//if( Main.projectile[projectile.whoAmI] != null && projectile.active )
				if(customProjectile?.Active==true)
				{
					//projectile.netUpdate = true;
					ProjectileManager.SendProjectileUpdate(customProjectile.Index);
				}

				//collision tests

				//tiles
				if(projectile.tileCollide)
				{
					var tileCollisions = TileFunctions.GetOverlappedTiles(projectile.Hitbox);

					if( tileCollisions.Count > 0 )
					{
						definition.OnTileCollision?.Invoke(customProjectile, tileCollisions);
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
								onCollision(customProjectile, player);
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
				var onAiUpdate = customProjectile.Definition.OnAiUpdate;
				if( onAiUpdate != null )
				{
					var handled = onAiUpdate(customProjectile);
					result = handled == true ? HookResult.Cancel : HookResult.Continue;
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
					onKilled(customProjectile);

					customProjectiles.Remove(projectile);
					projectile.active = false;
					ProjectileManager.SendProjectileKill(customProjectile.Index, customProjectile.Owner);
					
				}
				
				return HookResult.Cancel;
			}
			else
			{
				return HookResult.Continue;
			}
		}
		
		private void OnReload(ReloadEventArgs args)
		{
			foreach( var definition in Definitions )
			{
				definition.Dispose();
			}
			Definitions.Clear();

			LoadDefinitions();
			
			args.Player.SendSuccessMessage("[CustomNpcs] Reloaded Projectiles!");
		}
	}
}
