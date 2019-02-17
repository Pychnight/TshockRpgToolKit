using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;
using TShockAPI.Localization;

namespace Corruption
{
	public static class ItemFunctions
	{
		public static string GetItemNameFromId(int type)
		{
			return EnglishLanguage.GetItemNameById(type);
		}

		public static int? GetItemIdFromName(string name)
		{
			var item = TShock.Utils.GetItemByName(name).FirstOrDefault();
			return item?.type;
		}
	}
}
