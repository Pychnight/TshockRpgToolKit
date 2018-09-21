using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Xna.Framework;
using Corruption;
using BooTS;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler;
using Corruption.PluginSupport;
using Banking;

namespace CustomNpcs.Npcs
{
    /// <summary>
    ///     Represents an NPC manager. This class is a singleton.
    /// </summary>
    public sealed class NpcManager : CustomTypeManager<NpcDefinition>, IDisposable
    {
        internal const string IgnoreCollisionKey = "CustomNpcs_IgnoreCollision";

        private static readonly bool[] AllowedDrops = ItemID.Sets.Factory.CreateBoolSet(
            // Allow dropping coins.
            ItemID.CopperCoin, ItemID.SilverCoin, ItemID.GoldCoin, ItemID.PlatinumCoin,
            // Allow dropping hearts and stars.
            ItemID.Heart, ItemID.CandyApple, ItemID.CandyCane, ItemID.Star, ItemID.SoulCake, ItemID.SugarPlum,
            // Allow dropping biome-related souls.
            ItemID.SoulofLight, ItemID.SoulofNight,
            // Allow dropping mechanical boss summoning items.
            ItemID.MechanicalWorm, ItemID.MechanicalEye, ItemID.MechanicalSkull,
            // Allow dropping key molds.
            ItemID.JungleKeyMold, ItemID.CorruptionKeyMold, ItemID.CrimsonKeyMold, ItemID.HallowedKeyMold,
            ItemID.FrozenKeyMold,
            // Allow dropping nebula armor items.
            ItemID.NebulaPickup1, ItemID.NebulaPickup2, ItemID.NebulaPickup3);

		private readonly bool[] _checkNpcForReplacement = new bool[Main.maxNPCs + 1];
        private readonly ConditionalWeakTable<NPC, CustomNpc> _customNpcs = new ConditionalWeakTable<NPC, CustomNpc>();
        private readonly CustomNpcsPlugin _plugin;
        private readonly Random _random = new Random();
				
        internal NpcManager(CustomNpcsPlugin plugin)
        {
            _plugin = plugin;

			BasePath = "npcs";
			ConfigPath = Path.Combine(BasePath, "npcs.json");
			AssemblyNamePrefix = "Npc_";
						
			LoadDefinitions();

			GeneralHooks.ReloadEvent += OnReload;
            ServerApi.Hooks.GameUpdate.Register(_plugin, OnGameUpdate);
            ServerApi.Hooks.NpcAIUpdate.Register(_plugin, OnNpcAiUpdate);
            ServerApi.Hooks.NpcKilled.Register(_plugin, OnNpcKilled);
            ServerApi.Hooks.NpcLootDrop.Register(_plugin, OnNpcLootDrop);
            ServerApi.Hooks.NpcSetDefaultsInt.Register(_plugin, OnNpcSetDefaults);
            ServerApi.Hooks.NpcSpawn.Register(_plugin, OnNpcSpawn);
            ServerApi.Hooks.NpcStrike.Register(_plugin, OnNpcStrike);
			//ServerApi.Hooks.NpcTransform.Register(_plugin, OnNpcTransform);
			OTAPI.Hooks.Npc.PostTransform = OnNpcTransform;
		}

        /// <summary>
        ///     Gets the NPC manager instance.
        /// </summary>
        public static NpcManager Instance { get; internal set; }

        /// <summary>
        ///     Disposes the NPC manager.
        /// </summary>
        public void Dispose()
        {
            //File.WriteAllText(NpcsConfigPath, JsonConvert.SerializeObject(_definitions, Formatting.Indented));

            foreach (var definition in Definitions)
            {
                definition.Dispose();
            }
            Definitions.Clear();

            GeneralHooks.ReloadEvent -= OnReload;
            ServerApi.Hooks.GameUpdate.Deregister(_plugin, OnGameUpdate);
            ServerApi.Hooks.NpcAIUpdate.Deregister(_plugin, OnNpcAiUpdate);
            ServerApi.Hooks.NpcKilled.Deregister(_plugin, OnNpcKilled);
            ServerApi.Hooks.NpcLootDrop.Deregister(_plugin, OnNpcLootDrop);
            ServerApi.Hooks.NpcSetDefaultsInt.Deregister(_plugin, OnNpcSetDefaults);
            ServerApi.Hooks.NpcSpawn.Deregister(_plugin, OnNpcSpawn);
            ServerApi.Hooks.NpcStrike.Deregister(_plugin, OnNpcStrike);
			//ServerApi.Hooks.NpcTransform.Deregister(_plugin, OnNpcTransform);
			OTAPI.Hooks.Npc.PostTransform = null;
		}
		
        /// <summary>
        ///     Gets the custom NPC associated with the specified NPC.
        /// </summary>
        /// <param name="npc">The NPC, which must not be <c>null</c>.</param>
        /// <returns>The custom NPC, or <c>null</c> if it does not exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="npc" /> is <c>null</c>.</exception>
        public CustomNpc GetCustomNpc(NPC npc)
        {
            if (npc == null)
            {
                throw new ArgumentNullException(nameof(npc));
            }

            return _customNpcs.TryGetValue(npc, out var customNpc) ? customNpc : null;
        }

        /// <summary>
        ///     Spawns a custom NPC at the specified coordinates.
        /// </summary>
        /// <param name="definition">The definition, which must not be <c>null</c>.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition" /> is <c>null</c>.</exception>
        /// <returns>The custom NPC, or <c>null</c> if spawning failed.</returns>
        public CustomNpc SpawnCustomNpc( NpcDefinition definition, int x, int y)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            // The following code needs to be synchronized since SpawnCustomNpc may run on a different thread than the
            // main thread. Otherwise, there is a possible race condition where the newly-spawned custom NPC may be
            // erroneously checked for replacement.
            //lock (_checkNpcLock)
            {
                var npcId = NPC.NewNPC(x, y, definition.BaseType);
                return npcId != Main.maxNPCs ? AttachCustomNpc(Main.npc[npcId], definition) : null;
            }
        }

        internal CustomNpc AttachCustomNpc(NPC npc, NpcDefinition definition)
        {
            definition.ApplyTo(npc);

            var customNpc = new CustomNpc(npc, definition);
            _customNpcs.Remove(npc);
            _customNpcs.Add(npc, customNpc);

			//HACK! Banking plugin tracks Npc Life, but will never see the custom set hp at spawn event(happens too early).
			//So we have to set it ourselves here...
			if(BankingPlugin.Instance!=null)
			{
				//Debug.Print("** Custom set life.");
				BankingPlugin.Instance.NpcSpawnHP[npc.whoAmI] = customNpc.Hp;//dont use max hp, see the definition.Apply() for notes on life and clients.
				//Debug.Print($"** newHp: {BankingPlugin.Instance.NpcSpawnHP[npc.whoAmI]}");
			}

			try
			{
				CustomIDFunctions.CurrentID = definition.Name;
				definition.OnSpawn?.Invoke(customNpc);
			}
			catch(Exception ex)
			{
				Utils.LogScriptRuntimeError(ex);
				definition.OnSpawn = null;
			}
			
            // Ensure that all players see the changes.
            var npcId = npc.whoAmI;
            _checkNpcForReplacement[npcId] = false;
            TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", npcId);
            TSPlayer.All.SendData(PacketTypes.UpdateNPCName, "", npcId);
            return customNpc;
        }

		protected override IEnumerable<EnsuredMethodSignature> GetEnsuredMethodSignatures()
		{
			var sigs = new List<EnsuredMethodSignature>()
			{
				new EnsuredMethodSignature("OnCheckReplace",typeof(double))
					.AddParameter("npc",typeof(NPC)),

				new EnsuredMethodSignature("OnCheckSpawn", typeof(int))
					.AddParameter("player",typeof(TSPlayer))
					.AddParameter("x",typeof(int))
					.AddParameter("y",typeof(int)),

				new EnsuredMethodSignature("OnSpawn")
					.AddParameter("npc",typeof(CustomNpc)),

				new EnsuredMethodSignature("OnCollision")
					.AddParameter("npc",typeof(CustomNpc))
					.AddParameter("player",typeof(TSPlayer)),
				
				new EnsuredMethodSignature("OnTileCollision")
					.AddParameter("npc",typeof(CustomNpc))
					.AddParameter("tileHits",typeof(List<Point>)),

				new EnsuredMethodSignature("OnKilled")
					.AddParameter("npc",typeof(CustomNpc)),

				new EnsuredMethodSignature("OnTransformed")
					.AddParameter("npc",typeof(CustomNpc)),

				new EnsuredMethodSignature("OnStrike",typeof(bool))
					.AddParameter("npc",typeof(CustomNpc))
					.AddParameter("player",typeof(TSPlayer))
					.AddParameter("damage",typeof(int))
					.AddParameter("knockback",typeof(float))
					.AddParameter("critical",typeof(bool)),

				new EnsuredMethodSignature("OnAiUpdate",typeof(bool))
					.AddParameter("npc",typeof(CustomNpc))
			};
			
			return sigs;
		}
		
		protected override void LoadDefinitions()
		{
			CustomNpcsPlugin.Instance.LogPrint($"Compiling NPC scripts.", TraceLevel.Info);
			base.LoadDefinitions();		
		}

		private void OnGameUpdate(EventArgs args)
        {
            Utils.TrySpawnForEachPlayer(TrySpawnCustomNpc);
			
			foreach (var npc in Main.npc.Where(n => n?.active == true))
            {
				var customNpc = GetCustomNpc(npc);
                if (customNpc?.Definition.ShouldAggressivelyUpdate == true)
                {
                    npc.netUpdate = true;
                }

				if(customNpc?.HasChildren==true)
				{
					customNpc?.UpdateChildren();
				}

				if(customNpc?.HasTransformed == true)
				{
					var id = customNpc.Npc.whoAmI;
					customNpc.HasTransformed = false;

					var definition = customNpc.Definition;

					if( definition.OnTransformed != null )
					{
						try
						{
							CustomIDFunctions.CurrentID = definition.Name;
							definition.OnTransformed(customNpc);
						}
						catch(Exception ex)
						{
							Utils.LogScriptRuntimeError(ex);
							definition.OnTransformed = null;
						}
						
						TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", id);
						TSPlayer.All.SendData(PacketTypes.UpdateNPCName, "", id);
					}
				}

				var npcId = npc.whoAmI;
                if (_checkNpcForReplacement[npcId])
                {
                    TryReplaceNpc(npc);
                    _checkNpcForReplacement[npcId] = false;
                }
            }

			//player - npc collisions
			foreach( var player in TShock.Players.Where(p => p?.Active == true) )
			{
				var tplayer = player.TPlayer;
				var playerHitbox = tplayer.Hitbox;
				if( !tplayer.immune )
				{
					player.SetData(IgnoreCollisionKey, false);
				}

				foreach( var npc in Main.npc.Where(n => n?.active == true) )
				{
					var customNpc = GetCustomNpc(npc);
					if( customNpc != null )
					{
						var definition = customNpc.Definition;

						if( definition.OnCollision != null )
						{
							if( npc.Hitbox.Intersects(playerHitbox) && !player.GetData<bool>(IgnoreCollisionKey) )
							{
								try
								{
									CustomIDFunctions.CurrentID = definition.Name;
									definition.OnCollision(customNpc, player);
								}
								catch(Exception ex)
								{
									Utils.LogScriptRuntimeError(ex);
									definition.OnCollision = null;
								}
																
								//player.SetData(IgnoreCollisionKey, true);
								//break;//should this be a continue instead??
							}
						}
					}
				}

				if( !tplayer.immune )
				{
					player.SetData(IgnoreCollisionKey, true);
				}
			}

			//npc - tile collisions
			foreach( var npc in Main.npc.Where(n => n?.active == true) )
			{
				var customNpc = GetCustomNpc(npc);
				if( customNpc != null )
				{
					var definition = customNpc.Definition;

					if( definition.OnTileCollision != null )
					{
						var tileCollisions = TileFunctions.GetOverlappedTiles(npc.Hitbox);
						if( tileCollisions.Count > 0 )
						{
							try
							{
								CustomIDFunctions.CurrentID = definition.Name;
								definition.OnTileCollision(customNpc, tileCollisions);
							}
							catch(Exception ex)
							{
								Utils.LogScriptRuntimeError(ex);
								definition.OnTileCollision = null;
							}
						}
					}
				}
			}
	    }

        private void OnNpcAiUpdate(NpcAiUpdateEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            var customNpc = GetCustomNpc(args.Npc);
            if (customNpc == null)
            {
                return;
            }

			var definition = customNpc.Definition;

			try
			{
				CustomIDFunctions.CurrentID = definition.Name;
				var result = definition.OnAiUpdate?.Invoke(customNpc);
				args.Handled = result==true;
			}
			catch(Exception ex)
			{
				Utils.LogScriptRuntimeError(ex);
				definition.OnAiUpdate = null;
			}
			
			TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", args.Npc.whoAmI);
        }

        private void OnNpcKilled(NpcKilledEventArgs args)
        {
            var npc = args.npc;
            var customNpc = GetCustomNpc(npc);
            if (customNpc == null)
            {
                return;
            }

            var definition = customNpc.Definition;
            foreach (var lootEntry in definition.LootEntries)
            {
                if (_random.NextDouble() >= lootEntry.Chance)
                {
                    continue;
                }

                var stackSize = _random.Next(lootEntry.MinStackSize, lootEntry.MaxStackSize);
                var items = TShock.Utils.GetItemByIdOrName(lootEntry.Name);
                if (items.Count == 1)
                {
                    Item.NewItem(npc.position, npc.Size, items[0].type, stackSize, false, lootEntry.Prefix);
                }
            }

			try
			{
				CustomIDFunctions.CurrentID = definition.Name;
				definition.OnKilled?.Invoke(customNpc);
			}
			catch(Exception ex)
			{
				Utils.LogScriptRuntimeError(ex);
				definition.OnKilled = null;
			}

			customNpc.IsNpcValid = false;
		}

        private void OnNpcLootDrop(NpcLootDropEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            var customNpc = GetCustomNpc(Main.npc[args.NpcArrayIndex]);
            if (customNpc != null)
            {
                args.Handled = customNpc.Definition.ShouldOverrideLoot && !AllowedDrops[args.ItemId];
            }
        }

        private void OnNpcSetDefaults(SetDefaultsEventArgs<NPC, int> args)
        {
            if (args.Handled)
            {
                return;
            }

            // If an NPC has its defaults set while in the world (e.g., NPC.Transform, slimes, and the Eater of
            // Worlds), we need to check for replacement since the NPC since the NPC may turn into a replaceable NPC.
            var npc = args.Object;
            if (npc.active && npc.position.X > 100)
            {
                _checkNpcForReplacement[npc.whoAmI] = true;
            }
        }

        private void OnNpcSpawn(NpcSpawnEventArgs args)
        {
			if (args.Handled)
            {
                return;
            }

			//Debug.Print($"OnNpcSpawn!! NpcId: {args.NpcId}");

			var npcId = args.NpcId;
            // Set npc.whoAmI. This is normally set only when the NPC is first updated; in this case, this needs to be
            // done now as we make several assumptions regarding it.
            Main.npc[npcId].whoAmI = npcId;
            _checkNpcForReplacement[npcId] = true;
        }

        private void OnNpcStrike(NpcStrikeEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            var npc = args.Npc;
            var customNpc = GetCustomNpc(npc);
            if (customNpc == null)
            {
                return;
            }

            var definition = customNpc.Definition;
            var player = TShock.Players[args.Player.whoAmI];
            if (!definition.ShouldTallyKills)
            {
                // Don't tally kills.
                npc.playerInteraction[player.Index] = false;
            }
            if (definition.ShouldUpdateOnHit)
            {
                customNpc.SendNetUpdate = true;
            }

			try
			{
				CustomIDFunctions.CurrentID = definition.Name;
				var result = definition.OnStrike?.Invoke(customNpc, player, args.Damage, args.KnockBack, args.Critical);
				args.Handled = result == true;
			}
			catch(Exception ex)
			{
				Utils.LogScriptRuntimeError(ex);
				definition.OnStrike = null;
			}
		}
		
		private void OnNpcTransform(NPC npc)
		{
			var customNpc = GetCustomNpc(npc);
			if( customNpc == null )
			{
				return;
			}

			customNpc.HasTransformed = true;
		}

		private void OnReload(ReloadEventArgs args)
        {
			foreach( var definition in Definitions )
			{
				definition.Dispose();
			}
			Definitions.Clear();

			LoadDefinitions();
			args.Player.SendSuccessMessage("[CustomNpcs] Reloaded NPCs!");
        }

        private void TryReplaceNpc(NPC npc)
        {
            var chances = new Dictionary<NpcDefinition, double>();
           
			foreach( var definition in Definitions.Where(d => d.ShouldReplace) )
			{
				var chance = 0.0;

				if( definition.OnCheckReplace != null )
				{
					try
					{
						CustomIDFunctions.CurrentID = definition.Name;
						chance = definition.OnCheckReplace(npc);
					}
					catch(Exception ex)
					{
						Utils.LogScriptRuntimeError(ex);
						definition.OnCheckReplace = null;
					}
				}
								
				chances[definition] = chance;
			}

			var randomDefinition = Utils.TryPickRandomKey(chances);
            if (randomDefinition != null)
            {
                AttachCustomNpc(npc, randomDefinition);
            }
        }

		private void TrySpawnCustomNpc(TSPlayer player, int tileX, int tileY)
		{
			Utils.GetSpawnData(player, out var maxSpawns, out var spawnRate);
			if( player.TPlayer.activeNPCs >= maxSpawns )
			{
				return;
			}

			var spawnViaGlobalRate = _random.Next((int)spawnRate) == 0;

			var weights = new Dictionary<NpcDefinition, int>(Definitions.Count);
			
			foreach( var definition in Definitions )
			{
				if( !definition.ShouldSpawn )
					continue;

				var candidateForSpawning = spawnViaGlobalRate;

				if(definition.SpawnRateOverride!=null)
				{
					candidateForSpawning = _random.Next((int)definition.SpawnRateOverride) == 0;
				}

				if(candidateForSpawning)
				{
					var weight = 0;

					if(definition.OnCheckSpawn!=null)
					{
						try
						{
							CustomIDFunctions.CurrentID = definition.Name;
							weight = definition.OnCheckSpawn(player, tileX, tileY);
						}
						catch(Exception ex)
						{
							Utils.LogScriptRuntimeError(ex);
							definition.OnCheckSpawn = null;
						}
					}
											
					weights[definition] = weight;
				}
			}
			
			if(weights.Count>0)
			{
				var randomDefinition = Utils.PickRandomWeightedKey(weights);
				if( randomDefinition != null )
				{
					SpawnCustomNpc(randomDefinition, 16 * tileX + 8, 16 * tileY);
				}
			}
		}
	}
}
