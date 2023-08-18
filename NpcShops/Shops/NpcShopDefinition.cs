using Corruption.PluginSupport;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NpcShops.Shops
{
	/// <summary>
	///     Represents an NPC shop definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class NpcShopDefinition : IValidator
	{
		/// <summary>
		///     Gets the opening time.
		/// </summary>
		[JsonProperty(Order = 1)]
		public string OpeningTime { get; private set; }

		/// <summary>
		///     Gets the closing time.
		/// </summary>
		[JsonProperty(Order = 2)]
		public string ClosingTime { get; private set; }

		/// <summary>
		///     Gets the message.
		/// </summary>
		[JsonProperty(Order = 3)]
		public string Message { get; private set; }

		/// <summary>
		///     Gets the closed message.
		/// </summary>
		[JsonProperty(Order = 4)]
		public string ClosedMessage { get; private set; }

		/// <summary>
		///     Gets the restock time.
		/// </summary>
		[JsonProperty(Order = 5)]
		public TimeSpan RestockTime { get; private set; } = TimeSpan.FromMinutes(1);

		/// <summary>
		///     Gets the sales tax rate.
		/// </summary>
		[JsonProperty(Order = 6)]
		public double SalesTaxRate { get; private set; } = 0.07;

		/// <summary>
		///     Gets the region name.
		/// </summary>
		[JsonProperty(Order = 7)]
		public string RegionName { get; private set; }

		/// <summary>
		///		Gets the town npc types that this shop overrides.
		/// </summary>
		[JsonProperty(Order = 8)]
		public List<object> OverrideNpcTypes { get; private set; } = new List<object>();//we use object to maintain compatibility with older versions which used int npc id's.
																						//but now we also want to support string id's, in order to use CustomNpcs.
																						//A later step will parse the values, and convert them all to string keys used internally now.

		/// <summary>
		///     Gets the list of shop items.
		/// </summary>
		[JsonProperty(Order = 9)]
		public IList<ShopItemDefinition> ShopItems { get; private set; } = new List<ShopItemDefinition>();

		/// <summary>
		///     Gets the list of shop items.
		/// </summary>
		[JsonProperty(Order = 10)]
		public IList<ShopCommandDefinition> ShopCommands { get; private set; } = new List<ShopCommandDefinition>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static NpcShopDefinition TryLoadFromFile(string filePath)
		{
			NpcShopDefinition result = null;

			try
			{
				NpcShopsPlugin.Instance.LogPrint($"Loading NpcShop {filePath} ...", TraceLevel.Info);
				var txt = File.ReadAllText(filePath);
				result = JsonConvert.DeserializeObject<NpcShopDefinition>(txt);

				return result;
			}
			catch (JsonReaderException jrex)
			{
				NpcShopsPlugin.Instance.LogPrint($"A json error occured while trying to load NpcShop {filePath}.", TraceLevel.Error);
				NpcShopsPlugin.Instance.LogPrint(jrex.Message, TraceLevel.Error);
			}
			catch (Exception ex)
			{
				NpcShopsPlugin.Instance.LogPrint($"An error occured while trying to load NpcShop {filePath}.", TraceLevel.Error);
				NpcShopsPlugin.Instance.LogPrint(ex.Message, TraceLevel.Error);
			}

			NpcShopsPlugin.Instance.LogPrint("Shop disabled.", TraceLevel.Error);

			return result;
		}

		public ValidationResult Validate()
		{
			var result = new ValidationResult(this);

			if (OverrideNpcTypes == null || OverrideNpcTypes.Count < 1)
				result.Warnings.Add(new ValidationWarning($"OverrideNpcTypes is null or empty. This shop will never be used."));

			if ((ShopItems == null || ShopItems.Count == 0) &&
				(ShopCommands == null || ShopCommands.Count == 0))
				result.Warnings.Add(new ValidationWarning($"There are no ShopItems or ShopCommands defined. This shop can never sell anything."));

			if (ShopItems.Count > 0)
			{
				//copy each item error and warning 
				for (var i = 0; i < ShopItems.Count; i++)
				{
					var item = ShopItems[i];

					if (item != null)
					{
						var itemName = !string.IsNullOrWhiteSpace(item.ItemName) ? $" '{item.ItemName}'" : "";
						var itemResult = item.Validate();

						itemResult.Source = $"ShopItem[{i}]{itemName}";
						result.Children.Add(itemResult);
					}
					else
					{
						result.Errors.Add(new ValidationError($"ShopItem at index #{i} is null."));
					}
				}
			}

			if (ShopCommands.Count > 0)
			{
				//copy each item error and warning 
				for (var i = 0; i < ShopCommands.Count; i++)
				{
					var cmd = ShopCommands[i];

					if (cmd != null)
					{
						var cmdName = !string.IsNullOrWhiteSpace(cmd.Name) ? $" '{cmd.Name}'" : "";
						var cmdResult = cmd.Validate();

						cmdResult.Source = $"ShopCommand[{i}]{cmdName}";
						result.Children.Add(cmdResult);
					}
					else
					{
						result.Errors.Add(new ValidationError($"ShopCommand at index #{i} is null."));
					}
				}
			}

			return result;
		}
	}
}
