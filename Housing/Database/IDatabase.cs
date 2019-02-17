using Housing.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Housing.Database
{
	/// <summary>
	/// Repository pattern interface for supporting multiple data-store types.
	/// </summary>
	public interface IDatabase
	{
		void Load();

		IList<House> GetHouses();
		IList<Shop> GetShops();
		IList<TaxCollector> GetTaxCollectors();
		
		House AddHouse(TSPlayer player, string name, int x, int y, int x2, int y2);
		Shop AddShop(TSPlayer player, string name, int x, int y, int x2, int y2, int chestX, int chestY);
		TaxCollector AddTaxCollector(string name);
		
		void Update(House house);
		void Update(Shop shop);
		
		void Remove(House house);
		void Remove(Shop shop);
		void Remove(TaxCollector taxCollector);
	}
}
