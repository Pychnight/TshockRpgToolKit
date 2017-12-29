using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using System.Diagnostics;

namespace CustomNpcs.Npcs
{
    /// <summary>
    ///     Represents an NPC manager. This class is a singleton.
    /// </summary>
    public sealed class NpcManager : IDisposable
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

        private static readonly string NpcsPath = Path.Combine("npcs", "npcs.json");

        private readonly bool[] _checkNpcForReplacement = new bool[Main.maxNPCs + 1];
        private readonly object _checkNpcLock = new object();
        private readonly ConditionalWeakTable<NPC, CustomNpc> _customNpcs = new ConditionalWeakTable<NPC, CustomNpc>();
        private readonly object _lock = new object();
        private readonly CustomNpcsPlugin _plugin;
        private readonly Random _random = new Random();
		
        private List<NpcDefinition> _definitions = new List<NpcDefinition>();

        internal NpcManager(CustomNpcsPlugin plugin)
        {
            _plugin = plugin;

            Utils.TryExecuteLua(LoadDefinitions, "NpcManager");
			
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
        [CanBeNull]
        public static NpcManager Instance { get; internal set; }

        /// <summary>
        ///     Disposes the NPC manager.
        /// </summary>
        public void Dispose()
        {
            File.WriteAllText(NpcsPath, JsonConvert.SerializeObject(_definitions, Formatting.Indented));

            foreach (var definition in _definitions)
            {
                definition.Dispose();
            }
            _definitions.Clear();

            GeneralHooks.ReloadEvent -= OnReload;
            ServerApi.Hooks.GameUpdate.Deregister(_plugin, OnGameUpdate);
            ServerApi.Hooks.NpcAIUpdate.Deregister(_plugin, OnNpcAiUpdate);
            ServerApi.Hooks.NpcKilled.Deregister(_plugin, OnNpcKilled);
            ServerApi.Hooks.NpcLootDrop.Deregister(_plugin, OnNpcLootDrop);
            ServerApi.Hooks.NpcSetDefaultsInt.Deregister(_plugin, OnNpcSetDefaults);
            ServerApi.Hooks.NpcSpawn.Deregister(_plugin, OnNpcSpawn);
            ServerApi.Hooks.NpcStrike.Deregister(_plugin, OnNpcStrike);
			//ServerApi.Hooks.NpcTransform.Deregister(_plugin, OnNpcTransform);
		}

        /// <summary>
        ///     Finds the definition with the specified name.
        /// </summary>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <returns>The definition, or <c>null</c> if it does not exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        [CanBeNull]
        public NpcDefinition FindDefinition([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            lock (_lock)
            {
                return _definitions.FirstOrDefault(d => name.Equals(d.Name, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        ///     Gets the custom NPC associated with the specified NPC.
        /// </summary>
        /// <param name="npc">The NPC, which must not be <c>null</c>.</param>
        /// <returns>The custom NPC, or <c>null</c> if it does not exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="npc" /> is <c>null</c>.</exception>
        public CustomNpc GetCustomNpc([NotNull] NPC npc)
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
        public CustomNpc SpawnCustomNpc([NotNull] NpcDefinition definition, int x, int y)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            // The following code needs to be synchronized since SpawnCustomNpc may run on a different thread than the
            // main thread. Otherwise, there is a possible race condition where the newly-spawned custom NPC may be
            // erroneously checked for replacement.
            lock (_checkNpcLock)
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

            lock (_lock)
            {
                Utils.TryExecuteLua(() => definition.OnSpawn?.Call(customNpc), definition.Name);
            }

            // Ensure that all players see the changes.
            var npcId = npc.whoAmI;
            _checkNpcForReplacement[npcId] = false;
            TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", npcId);
            TSPlayer.All.SendData(PacketTypes.UpdateNPCName, "", npcId);
            return customNpc;
        }

        private void LoadDefinitions()
        {
            if (File.Exists(NpcsPath))
            {
                _definitions = JsonConvert.DeserializeObject<List<NpcDefinition>>(File.ReadAllText(NpcsPath));
                var failedDefinitions = new List<NpcDefinition>();
                foreach (var definition in _definitions)
                {
                    try
                    {
                        definition.ThrowIfInvalid();
                    }
                    catch (FormatException ex)
                    {
                        TShock.Log.ConsoleError(
                            $"[CustomNpcs] An error occurred while parsing NPC '{definition.Name}': {ex.Message}");
                        failedDefinitions.Add(definition);
                        continue;
                    }
                    definition.LoadLuaDefinition();
                }
                _definitions = _definitions.Except(failedDefinitions).ToList();
            }
			else
			{
				ServerApi.LogWriter.PluginWriteLine(_plugin, $"Npc's configuration does not exist. Expected config file to be at: {NpcsPath}", TraceLevel.Error);

				_definitions = new List<NpcDefinition>();
			}
        }

        private void OnGameUpdate(EventArgs args)
        {
            Utils.TrySpawnForEachPlayer(TrySpawnCustomNpc);
			
            lock (_checkNpcLock)
            {
                foreach (var npc in Main.npc.Where(n => n?.active == true))
                {
                    var customNpc = GetCustomNpc(npc);
                    if (customNpc?.Definition.ShouldAggressivelyUpdate == true)
                    {
                        npc.netUpdate = true;
                    }

					if(customNpc?.HasTransformed == true)
					{
						var id = customNpc.Npc.whoAmI;
						customNpc.HasTransformed = false;

						var definition = customNpc.Definition;
						var onTransformed = definition.OnTransformed;
						if( onTransformed != null )
						{
							Utils.TryExecuteLua(() => onTransformed.Call(customNpc), definition.Name);
						}

						TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", id);
						TSPlayer.All.SendData(PacketTypes.UpdateNPCName, "", id);
					}

					var npcId = npc.whoAmI;
                    if (_checkNpcForReplacement[npcId])
                    {
                        TryReplaceNpc(npc);
                        _checkNpcForReplacement[npcId] = false;
                    }
                }
            }

            foreach (var player in TShock.Players.Where(p => p?.Active == true))
            {
                var tplayer = player.TPlayer;
                var playerHitbox = tplayer.Hitbox;
                if (!tplayer.immune)
                {
                    player.SetData(IgnoreCollisionKey, false);
                }

                foreach (var npc in Main.npc.Where(n => n?.active == true))
                {
					//test against npc
                    var customNpc = GetCustomNpc(npc);
                    if(customNpc != null)
                    {
						if (npc.Hitbox.Intersects(playerHitbox) && !player.GetData<bool>(IgnoreCollisionKey))
						{
							lock (_lock)
							{
								var definition = customNpc.Definition;
								Utils.TryExecuteLua(() => definition.OnCollision?.Call(customNpc, player), definition.Name);
							}
							player.SetData(IgnoreCollisionKey, true);
							break;//should this be a continue instead??
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

			lock (_lock)
            {
                var definition = customNpc.Definition;
                var onAiUpdate = definition.OnAiUpdate;
                if (onAiUpdate != null)
                {
                    Utils.TryExecuteLua(() => args.Handled = (bool)onAiUpdate.Call(customNpc)[0], definition.Name);
                }
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

            lock (_lock)
            {
                Utils.TryExecuteLua(() => definition.OnKilled?.Call(customNpc), definition.Name);
            }
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

            lock (_lock)
            {
				var onStrike = definition.OnStrike;
                if (onStrike != null)
                {
                    Utils.TryExecuteLua(() => args.Handled =
                         (bool)onStrike.Call(customNpc, player, args.Damage, args.KnockBack, args.Critical)[0],
                         definition.Name);
                }
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
            lock (_lock)
            {
                foreach (var definition in _definitions)
                {
                    definition.Dispose();
                }
                _definitions.Clear();

                Utils.TryExecuteLua(LoadDefinitions, "NpcManager");
            }
            args.Player.SendSuccessMessage("[CustomNpcs] Reloaded NPCs!");
        }

        private void TryReplaceNpc(NPC npc)
        {
            var chances = new Dictionary<NpcDefinition, double>();
            lock (_lock)
            {
                foreach (var definition in _definitions.Where(d => d.ShouldReplace))
                {
                    var chance = 0.0;
                    var onCheckReplace = definition.OnCheckReplace;
                    if (onCheckReplace != null)
                    {
                        Utils.TryExecuteLua(() => chance = (double)onCheckReplace.Call(npc)[0], definition.Name);
                    }
                    chances[definition] = chance;
                }
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

			var weights = new Dictionary<NpcDefinition, int>(_definitions.Count);
			lock( _lock )
			{
				foreach( var definition in _definitions )
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
						var onCheckSpawn = definition.OnCheckSpawn;
						if( onCheckSpawn != null )
						{
							// First cast to a double then an integer because NLua will return a double.
							Utils.TryExecuteLua(() => {
								//we must check in case of script not returning a number
								var results = onCheckSpawn.Call(player, tileX, tileY);
								if( results != null && results.Length == 1 && results[0] is double )
									weight = (int)(double)results[0];
								else
									throw new Exception($"NPC '{definition.Name}' function OnCheckSpawn() should return a number.");
							},
							definition.Name);
						}
						weights[definition] = weight;
					}
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
