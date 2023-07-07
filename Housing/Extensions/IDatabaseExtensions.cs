using Housing.Database;
using Housing.Models;
using System.Collections.Generic;
using System.Linq;

namespace Housing.Extensions
{
	public static class IDatabaseExtensions
	{
		/// <summary>
		///     Gets the house containing the specified coordinates, or <c>null</c> if there is none.
		/// </summary>
		/// <param name="x">The X coordinate.</param>
		/// <param name="y">The Y coordinate.</param>
		/// <returns>The house, or <c>null</c> if there is none.</returns>
		public static House GetHouse(this IDatabase database, int x, int y) => database.GetHouses().FirstOrDefault(h => h.Rectangle.Contains(x, y));

		/// <summary>
		///		Attempts to find a House by the given owner and house name.
		/// </summary>
		/// <param name="ownerName"></param>
		/// <param name="houseName"></param>
		/// <returns></returns>
		public static House GetHouse(this IDatabase database, string ownerName, string houseName)
		{
			var house = database.GetHouses().Where(h => h.OwnerName == ownerName).
											FirstOrDefault(h => h.Name == houseName);

			return house;
		}

		/// <summary>
		///		Finds all houses with the given owner.
		/// </summary>
		/// <param name="ownerName"></param>
		/// <returns></returns>
		public static IList<House> GetHouses(this IDatabase database, string ownerName) => database.GetHouses().Where(h => h.OwnerName == ownerName).ToList();

		/// <summary>
		///     Gets the shop containing the specified coordinates, or <c>null</c> if there is none.
		/// </summary>
		/// <param name="x">The X coordinate.</param>
		/// <param name="y">The Y coordinate.</param>
		/// <returns>The shop, or <c>null</c> if there is none.</returns>
		public static Shop GetShop(this IDatabase database, int x, int y) => database.GetShops().FirstOrDefault(h => h.Rectangle.Contains(x, y));

		/// <summary>
		///		Trys to get a TaxCollector for the given player name.
		/// </summary>
		/// <param name="playerName">Player name.</param>
		/// <returns>TaxCollector if found, null otherwise.</returns>
		public static TaxCollector GetTaxCollector(this IDatabase database, string playerName) => database.GetTaxCollectors().FirstOrDefault(t => t.PlayerName == playerName);
	}
}
