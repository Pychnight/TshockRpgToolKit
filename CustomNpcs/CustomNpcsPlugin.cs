using System;
using System.IO;
using System.Reflection;
using CustomNpcs.Invasions;
using CustomNpcs.Npcs;
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomNpcsPlugin" /> class using the specified Main instance.
        /// </summary>
        /// <param name="game">The Main instance.</param>
        public CustomNpcsPlugin(Main game) : base(game)
        {
            Directory.CreateDirectory("npcs");

            InvasionManager.Instance = new InvasionManager(this);
            NpcManager.Instance = new NpcManager(this);
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
        public override string Name => "CustomNpcs";

        /// <summary>
        ///     Gets the version.
        /// </summary>
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        private static void CustomInvade(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 1)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}cinvade <name|stop>");
                return;
            }

            var currentInvasion = InvasionManager.Instance?.CurrentInvasion;
            var inputName = parameters[0];
            if (inputName.Equals("stop", StringComparison.OrdinalIgnoreCase))
            {
                if (currentInvasion == null)
                {
                    player.SendErrorMessage("There is currently no custom invasion.");
                    return;
                }

                InvasionManager.Instance.StartInvasion(null);
                TSPlayer.All.SendInfoMessage($"{player.Name} stopped the current custom invasion.");
                return;
            }

            if (currentInvasion != null)
            {
                player.SendErrorMessage("There is currently already a custom invasion.");
                return;
            }

            var definition = InvasionManager.Instance?.FindDefinition(inputName);
            if (definition == null)
            {
                player.SendErrorMessage($"Invalid invasion '{inputName}'.");
                return;
            }

            InvasionManager.Instance.StartInvasion(definition);
        }

        private static void CustomMaxSpawns(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 1)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}cmaxspawns <max-spawns>");
                return;
            }

            var inputMaxSpawns = parameters[0];
            if (!int.TryParse(inputMaxSpawns, out var maxSpawns) || maxSpawns < 0 || maxSpawns > 200)
            {
                player.SendErrorMessage($"Invalid maximum spawns '{inputMaxSpawns}'.");
                return;
            }

            Config.Instance.MaxSpawns = maxSpawns;
            if (args.Silent)
            {
                player.SendSuccessMessage($"Set custom maximum spawns to {maxSpawns}.");
            }
            else
            {
                TSPlayer.All.SendInfoMessage($"{player.Name} set the custom maximum spawns to {maxSpawns}.");
            }
        }

        private static void CustomSpawnMob(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 1 && parameters.Count != 2)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}cspawnmob <name> [amount]");
                return;
            }

            var inputName = parameters[0];
            var definition = NpcManager.Instance?.FindDefinition(inputName);
            if (definition == null)
            {
                player.SendErrorMessage($"Invalid custom NPC name '{inputName}'.");
                return;
            }

            var inputAmount = parameters.Count == 2 ? parameters[1] : "1";
            if (!int.TryParse(inputAmount, out var amount) || amount <= 0 || amount > 200)
            {
                player.SendErrorMessage($"Invalid amount '{inputAmount}'.");
                return;
            }

            var x = player.TileX;
            var y = player.TileY;
            for (var i = 0; i < amount; ++i)
            {
                TShock.Utils.GetRandomClearTileWithInRange(x, y, 50, 50, out var spawnX, out var spawnY);
                NpcManager.Instance.SpawnCustomNpc(definition, 16 * spawnX, 16 * spawnY);
            }
            player.SendSuccessMessage($"Spawned {amount} {definition.Name}(s).");
        }

        private static void CustomSpawnRate(CommandArgs args)
        {
            var parameters = args.Parameters;
            var player = args.Player;
            if (parameters.Count != 1)
            {
                player.SendErrorMessage($"Syntax: {Commands.Specifier}cspawnrate <spawn-rate>");
                return;
            }

            var inputSpawnRate = parameters[0];
            if (!int.TryParse(inputSpawnRate, out var spawnRate) || spawnRate < 1)
            {
                player.SendErrorMessage($"Invalid spawn rate '{inputSpawnRate}'.");
                return;
            }

            Config.Instance.SpawnRate = spawnRate;
            if (args.Silent)
            {
                player.SendSuccessMessage($"Set custom spawn rate to {spawnRate}.");
            }
            else
            {
                TSPlayer.All.SendInfoMessage($"{player.Name} set the custom spawn rate to {spawnRate}.");
            }
        }

        private static void OnReload(ReloadEventArgs args)
        {
            if (File.Exists(ConfigPath))
            {
                Config.Instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
            args.Player.SendSuccessMessage("[CustomNpcs] Reloaded config!");
        }

        /// <summary>
        ///     Initializes the plugin.
        /// </summary>
        public override void Initialize()
        {
            if (File.Exists(ConfigPath))
            {
                Config.Instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }

            GeneralHooks.ReloadEvent += OnReload;

            Commands.ChatCommands.Add(new Command("customnpcs.cinvade", CustomInvade, "cinvade"));
            Commands.ChatCommands.Add(new Command("customnpcs.cmaxspawns", CustomMaxSpawns, "cmaxspawns"));
            Commands.ChatCommands.Add(new Command("customnpcs.cspawnmob", CustomSpawnMob, "cspawnmob", "csm"));
            Commands.ChatCommands.Add(new Command("customnpcs.cspawnrate", CustomSpawnRate, "cspawnrate"));
        }

        /// <summary>
        ///     Disposes the plugin.
        /// </summary>
        /// <param name="disposing"><c>true</c> to dispose managed resources; otherwise, <c>false</c>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config.Instance, Formatting.Indented));

                InvasionManager.Instance?.Dispose();
                InvasionManager.Instance = null;
                NpcManager.Instance?.Dispose();
                NpcManager.Instance = null;

                GeneralHooks.ReloadEvent -= OnReload;
            }

            base.Dispose(disposing);
        }
    }
}
