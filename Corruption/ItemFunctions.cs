using System.Linq;
using TShockAPI;
using TShockAPI.Localization;

namespace Corruption
{
	public static class ItemFunctions
	{
		public static string GetItemNameFromId(int type) => EnglishLanguage.GetItemNameById(type);

		public static int? GetItemIdFromName(string name)
		{
			var item = TShock.Utils.GetItemByName(name).FirstOrDefault();
			return item?.type;
		}
	}
}
