using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CustomNpcs.Definitions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace CustomNpcs
{
    /// <summary>
    ///     Represents the custom NPCs plugin.
    /// </summary>
    [ApiVersion(2, 1)]
    [UsedImplicitly]
    public sealed class CustomNpcsPlugin : TerrariaPlugin
    {
        private static readonly string ConfigPath = Path.Combine("npcs", "config.json");
        private static readonly string NpcsPath = Path.Combine("npcs", "npcs.json");

        private readonly ConditionalWeakTable<NPC, CustomNpc> _customNpcs = new ConditionalWeakTable<NPC, CustomNpc>();

        private readonly Random _random = new Random();

        private Config _config = new Config();

        private List<CustomNpcDefinition> _customNpcDefinitions =
            new List<CustomNpcDefinition> {new CustomNpcDefinition()};

        private int _ignoreSetDefaults;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomNpcsPlugin" /> class using the specified Main instance.
        /// </summary>
        /// <param name="game">The Main instance.</param>
        public CustomNpcsPlugin(Main game) : base(game)
        {
        }

        /// <summary>
        ///     Gets the author.
        /// </summary>
        public override string Author => "MarioE";

        /// <summary>
        ///     Gets the description.
        /// </summary>
        public override string Description => "Provides a custom NPC system.";

        /// <summary>
        ///     Gets the name.
        /// </summary>
        public override string Name => "CustomNpcsPlugin";

        /// <summary>
        ///     Gets the version.
        /// </summary>
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        /// <summary>
        ///     Initializes the plugin.
        /// </summary>
        public override void Initialize()
        {
            Directory.CreateDirectory("npcs");
            if (File.Exists(ConfigPath))
            {
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
            else
            {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(_config, Formatting.Indented));
            }
            if (File.Exists(NpcsPath))
            {
                _customNpcDefinitions =
                    JsonConvert.DeserializeObject<List<CustomNpcDefinition>>(File.ReadAllText(NpcsPath));
            }
            else
            {
                File.WriteAllText(NpcsPath, JsonConvert.SerializeObject(_customNpcDefinitions, Formatting.Indented));
            }
            foreach (var definition in _customNpcDefinitions)
            {
                definition.LoadLuaDefinition();
            }

            // Set all NPCs to use 4 life bytes.
            foreach (var key in Main.npcLifeBytes.Keys)
            {
                Main.npcLifeBytes[key] = 4;
            }

            GeneralHooks.ReloadEvent += OnReload;
            ServerApi.Hooks.NpcAIUpdate.Register(this, OnNpcAiUpdate);
            ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
            ServerApi.Hooks.NpcLootDrop.Register(this, OnNpcLootDrop);
            ServerApi.Hooks.NpcSetDefaultsInt.Register(this, OnNpcSetDefaults);
            ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);
            ServerApi.Hooks.NpcStrike.Register(this, OnNpcStrike);

            //Commands.ChatCommands.Add(new Command("customnpcs.test", Test, "test"));
        }

        /// <summary>
        ///     Disposes the plugin.
        /// </summary>
        /// <param name="disposing"><c>true</c> to dispose managed resources; otherwise, <c>false</c>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var definition in _customNpcDefinitions)
                {
                    definition.Dispose();
                }
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(_config, Formatting.Indented));
                File.WriteAllText(NpcsPath, JsonConvert.SerializeObject(_customNpcDefinitions, Formatting.Indented));

                GeneralHooks.ReloadEvent -= OnReload;
                ServerApi.Hooks.NpcAIUpdate.Deregister(this, OnNpcAiUpdate);
                ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);
                ServerApi.Hooks.NpcLootDrop.Deregister(this, OnNpcLootDrop);
                ServerApi.Hooks.NpcSetDefaultsInt.Deregister(this, OnNpcSetDefaults);
                ServerApi.Hooks.NpcSpawn.Deregister(this, OnNpcSpawn);
                ServerApi.Hooks.NpcStrike.Deregister(this, OnNpcStrike);
            }

            base.Dispose(disposing);
        }

        private void OnNpcAiUpdate(NpcAiUpdateEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }
            
            var npc = args.Npc;
            if (!_customNpcs.TryGetValue(npc, out var customNpc))
            {
                return;
            }

            var onAiUpdate = customNpc.Definition.OnAiUpdate;
            onAiUpdate?.Call(customNpc);
        }

        private void OnNpcKilled(NpcKilledEventArgs args)
        {
            var npc = args.npc;
            if (!_customNpcs.TryGetValue(npc, out var customNpc))
            {
                return;
            }

            var loot = customNpc.Definition.LootOverride;
            if (loot != null)
            {
                foreach (var definition in loot)
                {
                    if (_random.NextDouble() < definition.Chance)
                    {
                        var stackSize = _random.Next(definition.MinStackSize, definition.MaxStackSize);
                        Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, definition.Type,
                            stackSize, false, definition.Prefix);
                    }
                }

                double coins = npc.value;
                if (npc.midas)
                {
                    coins *= 1 + _random.Next(10, 50) * 0.01;
                }
                coins *= 1 + _random.Next(-20, 21) * 0.01;
                coins = new[] {5, 10, 15, 20}
                    .Where(w => _random.Next(w) == 0)
                    .Aggregate(coins, (c, w) => c * (1 + _random.Next(w, 2 * w + 1) * 0.01));
                coins += npc.extraValue;

                while (coins > 0)
                {
                    var coinSizes = new[] {1000000, 10000, 100, 1};
                    for (var i = 0; i < coinSizes.Length && coins > 0; ++i)
                    {
                        var coinSize = coinSizes[i];
                        var stack = (int)(coins / coinSize);
                        if (stack > 50 && _random.Next(5) == 0)
                        {
                            stack /= Main.rand.Next(3) + 1;
                        }
                        if (coinSize == 1)
                        {
                            if (Main.rand.Next(5) == 0)
                            {
                                stack /= Main.rand.Next(4) + 1;
                            }
                            stack = Math.Max(1, stack);
                        }
                        else if (Main.rand.Next(5) == 0)
                        {
                            stack /= Main.rand.Next(3) + 1;
                        }
                        coins -= coinSize * stack;
                        Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, 74 - i, stack);
                    }
                }
            }

            var onKilled = customNpc.Definition.OnKilled;
            onKilled?.Call(customNpc);
        }

        private void OnNpcLootDrop(NpcLootDropEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            var npc = Main.npc[args.NpcArrayIndex];
            if (!_customNpcs.TryGetValue(npc, out var customNpc))
            {
                return;
            }

            args.Handled = customNpc.Definition.LootOverride != null;
        }

        private void OnNpcSetDefaults(SetDefaultsEventArgs<NPC, int> args)
        {
            if (args.Handled || _ignoreSetDefaults-- > 0)
            {
                return;
            }

            // If the ID is negative, we need to ignore the next two SetDefaults. This is because SetDefaultsFromNetId
            // calls SetDefaults twice.
            var baseType = args.Info;
            if (baseType < 0)
            {
                _ignoreSetDefaults = 2;
            }

            var npc = args.Object;
            foreach (var definition in _customNpcDefinitions.Where(d => d.BaseType == baseType))
            {
                if (_random.NextDouble() < (definition.ReplacementChance ?? 0.0))
                {
                    // Use SetDefaultsDirect to prevent infinite recursion.
                    npc.SetDefaultsDirect(baseType);
                    definition.ApplyTo(npc);
                    if (_customNpcs.TryGetValue(npc, out _))
                    {
                        _customNpcs.Remove(npc);
                    }
                    _customNpcs.Add(npc, new CustomNpc(npc, definition));

                    args.Handled = true;
                    return;
                }
            }

            _customNpcs.Remove(npc);
        }

        private void OnNpcSpawn(NpcSpawnEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            var npc = Main.npc[args.NpcId];
            args.Handled = _customNpcs.TryGetValue(npc, out _);
        }

        private void OnNpcStrike(NpcStrikeEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            var npc = args.Npc;
            if (!_customNpcs.TryGetValue(npc, out var customNpc))
            {
                return;
            }

            var player = TShock.Players[args.Player.whoAmI];
            var onStrike = customNpc.Definition.OnStrike;
            if ((bool)(onStrike?.Call(customNpc, player, args.Damage, args.KnockBack, args.Critical)?[0] ?? false))
            {
                args.Handled = true;
            }
        }

        private void OnReload(ReloadEventArgs e)
        {
            foreach (var definition in _customNpcDefinitions)
            {
                definition.Dispose();
            }
            if (File.Exists(ConfigPath))
            {
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
            if (File.Exists(NpcsPath))
            {
                _customNpcDefinitions =
                    JsonConvert.DeserializeObject<List<CustomNpcDefinition>>(File.ReadAllText(NpcsPath));
            }
            foreach (var definition in _customNpcDefinitions)
            {
                definition.LoadLuaDefinition();
            }
        }
    }
}
