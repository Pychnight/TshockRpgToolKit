using Banking;
using Corruption.PluginSupport;
using CustomNpcs.Npcs;
using Microsoft.Xna.Framework;
using NpcShops.Shops;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace NpcShops
{
	[ApiVersion(2, 1)]
	public sealed partial class NpcShopsPlugin : TerrariaPlugin
	{
		public override string Author => "MarioE, Timothy Barela";
		public override string Description => "Adds an NPC shop system.";
		public override string Name => "NpcShops";
		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

		private const string SessionKey = "NpcShops_Session";
		private static readonly string ConfigPath = Path.Combine("npcshops", "config.json");
		internal static NpcShopsPlugin Instance { get; private set; }
		internal List<NpcShop> NpcShops = new List<NpcShop>();
		internal NpcPauser NpcPauser = new NpcPauser();

		public NpcShopsPlugin(Main game) : base(game)
		{
			Instance = this;
		}

		public override void Initialize()
		{
			//tryLoadConfig();

			GeneralHooks.ReloadEvent += OnReload;
			ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize, int.MinValue);
			ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
			ServerApi.Hooks.NetGetData.Register(this, OnNetGetData);

			Commands.ChatCommands.Add(new Command("npcshops.npcbuy", NpcBuy, "npcbuy"));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				//File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config.Instance, Formatting.Indented));

				GeneralHooks.ReloadEvent -= OnReload;
				ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
				ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);

				ServerApi.Hooks.NetGetData.Deregister(this, OnNetGetData);
			}
			base.Dispose(disposing);
		}

		private static Session GetOrCreateSession(TSPlayer player)
		{
			var session = player.GetData<Session>(SessionKey);
			if (session == null)
			{
				session = new Session(player);
				player.SetData(SessionKey, session);
			}
			return session;
		}

		private void TryLoad()
		{
			var shops = new List<NpcShop>();

			if (BankingPlugin.Instance == null)
			{
				this.LogPrint("BankingPlugin is not available. Banking is required for NpcShopsPlugin to operate.", TraceLevel.Error);
				return;
			}

			var files = Directory.EnumerateFiles("npcshops", "*.shop", SearchOption.AllDirectories);

			foreach (var file in files)
			{
				var definition = NpcShopDefinition.TryLoadFromFile(file);

				if (definition != null)
				{
					var errors = 0;
					var warnings = 0;

					var validationResult = definition.Validate();
					validationResult.Source = $"NpcShop {file}.";
					validationResult.GetTotals(ref errors, ref warnings);

					if (errors > 0 || warnings > 0)
					{
						this.LogPrint(validationResult);
					}

					if (errors == 0)
					{
						try
						{
							var shop = new NpcShop(definition);
							shops.Add(shop);
						}
						catch (Exception ex)
						{
							this.LogPrint($"An error occured while trying to create NpcShop {file}.", TraceLevel.Error);
							this.LogPrint(ex.Message, TraceLevel.Error);
						}
					}
					else
					{
						this.LogPrint($"Disabling NpcShop {file} due to errors.", TraceLevel.Error);
					}
				}
			}

			NpcShops = shops;
		}

		private static string GetPostPurchaseRenderString(NpcShop shop, ShopProduct product, decimal totalCost, int quantity)
		{
			var sb = new StringBuilder();

			if (totalCost > 0)
			{
				var totalCostString = product.Currency.GetCurrencyConverter().ToString(totalCost);
				sb.Append($"[c/{Color.OrangeRed.Hex3()}:{totalCostString}]");
			}

			if (product.RequiredItems.Count > 0)
			{
				var fragment = NpcShop.GetMaterialsCostRenderString(product, quantity); ;
				sb.Append(totalCost > 0 ? $" and {fragment}" : fragment);
			}

			return sb.ToString();
		}

		private void OnGamePostInitialize(EventArgs args)
		{
			Config.Instance = JsonConfig.LoadOrCreate<Config>(this, ConfigPath);
			TryLoad();
		}

		private void OnReload(ReloadEventArgs args)
		{
			NpcPauser.UnpauseAll();

			//clear sessions
			TShock.Players.Where(tp => tp?.Active == true)
							.ForEach(tp => tp.SetData<Session>(SessionKey, null));

			Config.Instance = JsonConfig.LoadOrCreate<Config>(this, ConfigPath);
			TryLoad();

			args.Player.SendSuccessMessage("[NpcShops] Reloaded config!");
		}

		private void OnNetGetData(GetDataEventArgs args)
		{
			var msgId = args.MsgID;

			if (msgId == PacketTypes.NpcTalk)
			{
				//Debug.Print("NpcShopTalk!!!"); // This happens when we right click the merchant

				//following is based off of https://github.com/MarioE/NoCheat/blob/master/NoCheat/ItemSpawning/Module.cs
				var player = TShock.Players[args.Msg.whoAmI];

				// Ignore packets sent when the client is syncing.
				if (player.State < 10)
					return;

				using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
				{
					reader.ReadByte();
					var npcIndex = reader.ReadInt16();
					var npc = npcIndex < 0 ? null : Main.npc[npcIndex];
					var npcType = npcIndex < 0 ? "0" : npc.type.ToString();

					//Determine if this is a custom npc, and if so try to get its id/internal name.
					if (npc != null)
					{
						var customNpc = NpcManager.Instance?.GetCustomNpc(npc);
						if (customNpc != null)
						{
							npcType = customNpc.Definition.Name ?? npcType;
						}
					}

					if (NpcShop.NpcToShopMap.ContainsKey(npcType))
					{
						//player.SendData(PacketTypes.NpcTalk, "", player.Index, npcIndex);
						player.SendData(PacketTypes.NpcTalk, "", player.Index, -1);
						//player.SendData(PacketTypes.NpcUpdate, "", npcIndex);

						if (npc != null && npc.active)
						{
							NpcPauser.Pause(npc, Config.Instance.ShopNpcPauseDuration);
						}

						var session = GetOrCreateSession(player);
						session.CurrentShopkeeperNpcIndex = npcIndex;
						//session.CurrentShop = npcShop;
						session.shopKeeperClickedHack = true;

						args.Handled = true;
						//Debug.Print($"Shop mapped to npc index {npcIndex}.");
					}
				}
			}
		}

		private void OnGameUpdate(EventArgs args)
		{
			foreach (var player in TShock.Players.Where(p => p?.Active == true))
			{
				var session = GetOrCreateSession(player);
				session.Update();
			}

			foreach (var shop in NpcShops)
			{
				shop.TryRestock();
			}

			NpcPauser.OnGameUpdate();
		}
	}
}
