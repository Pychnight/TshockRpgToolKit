using Housing.Models;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace Housing.Database
{
	public abstract class DatabaseBase : IDatabase
	{
		protected string ConnectionString { get; set; }
		protected List<House> Houses { get; set; } = new List<House>();
		protected List<Shop> Shops { get; set; } = new List<Shop>();
		protected List<TaxCollector> TaxCollectors { get; set; } = new List<TaxCollector>();
		protected HashSet<string> TaxCollectorNames { get; set; } = new HashSet<string>();

		public abstract House AddHouse(TSPlayer player, string name, int x, int y, int x2, int y2);
		public abstract Shop AddShop(TSPlayer player, string name, int x, int y, int x2, int y2, int chestX, int chestY);
		public abstract TaxCollector AddTaxCollector(string name);

		/// <summary>
		///     Gets the houses.
		/// </summary>
		/// <returns>The houses.</returns>
		public IList<House> GetHouses() => Houses.ToList();

		/// <summary>
		///     Gets the shops.
		/// </summary>
		/// <returns>The shops.</returns>
		public IList<Shop> GetShops() => Shops.ToList();

		/// <summary>
		///  Gets the TaxCollectors.
		/// </summary>
		/// <returns>TaxCollectors.</returns>
		public IList<TaxCollector> GetTaxCollectors() => TaxCollectors.ToList();

		public abstract void Load();
		public abstract void Remove(House house);
		public abstract void Remove(Shop shop);
		public abstract void Remove(TaxCollector taxCollector);
		public abstract void Update(House house);
		public abstract void Update(Shop shop);
	}
}
