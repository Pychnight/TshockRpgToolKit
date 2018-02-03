using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using Wolfje.Plugins.SEconomy;

namespace NpcShops.Shops
{
	/// Attempted to clean things up, but have to move on. Leaving this for a future refactor/cleanup of this project.
	public class PurchaseOrder
	{
		public ShopProduct Product { get; set; }
		public int Quantity { get; set; }
		public Money UnitCost { get; set; }
		public double SalesTaxRate { get; set; }
		public Money PurchaseCost { get { return Quantity * UnitCost; } }
		public Money SalesTax { get { return (Money)Math.Round(PurchaseCost * SalesTaxRate); } }

		public PurchaseOrder() {}

		public PurchaseOrder(ShopProduct product)
		{
			Product = product;
		}
	}
}
