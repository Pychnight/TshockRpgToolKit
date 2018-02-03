using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolfje.Plugins.SEconomy;

namespace NpcShops.Shops
{
	/// <summary>
	///		Common base class for shop sellable items.
	/// </summary>
	public abstract class ShopProduct
	{
		/// <summary>
		///     Gets or sets the stack size. A value of -1 indicates unlimited.
		/// </summary>
		public int StackSize { get; set; }

		/// <summary>
		///     Gets the unit price.
		/// </summary>
		public virtual Money UnitPrice { get; set; }

		/// <summary>
		///		Gets the items required for purchase.
		/// </summary>
		public List<RequiredItem> RequiredItems { get; set; } = new List<RequiredItem>();//just a placeholder for now

		/// <summary>
		///    Restocks the shop product.
		/// </summary>
		public abstract void Restock();
	}
}
