using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Wolfje.Plugins.SEconomy;
using TShockAPI;
using System.Linq;
using TShockAPI.DB;


namespace Housing.HousingEntites
{
    /// <summary>
    ///     Represents a house.
    /// </summary>
    /// 

    public class House : HousingEntity
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="House" /> class with the specified owner name, name, and coordinates.
        /// </summary>
        /// <param name="ownerName">The owner name.</param>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <param name="x">The first X coordinate.</param>
        /// <param name="y">The first Y coordinate.</param>
        /// <param name="x2">The second X coordinate, which must be at least the first.</param>
        /// <param name="y2">The second Y coordinate, which must be at least the second.</param>
        /// 

        public int House_ID { get; set; }
        public House(int region_id, int player_id, string entity_name, string owner_name) : base(region_id, player_id, entity_name, owner_name)
        {
            HouseName = EntityName;
            EnitityRectangle = EntityRegion.Area;
        }


        public House(TSPlayer owner, string entity_name, int x, int y, int x2, int y2) : base(owner, entity_name, x, y, x2, y2)
        {
            HouseName = EntityName;
            EnitityRectangle = EntityRegion.Area;
        }

        public string HouseName { get; set; }
        public bool ForSale { get; set; }

        public Money Debt { get; set; }
        public DateTime LastTaxed { get; set; } = DateTime.UtcNow;
        public Money Price { get; set; }
    }
}
