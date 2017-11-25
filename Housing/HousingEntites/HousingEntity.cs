using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Wolfje.Plugins.SEconomy;
using Terraria;
using System.Linq;
using TShockAPI;
using TShockAPI.DB;
using System.Data.Common;
using Housing.Database;

namespace Housing.HousingEntites
{
    public abstract class HousingEntity
    {
        public HousingEntity() { }
        //Initializers
        public HousingEntity(int region_id, int player_id, string entity_name, string owner_name)
        {
            WorldID = Main.worldID;
            RegionID = region_id;
            PlayerID = player_id;
            EntityName = entity_name;
            OwnerName = owner_name;
        }

        //use this for creating new ones
        public HousingEntity(TSPlayer owner, string entity_name, int x, int y, int x2, int y2)
        {
            WorldID = Main.worldID;
            PlayerID = owner.User.ID;
            EntityName = entity_name;
            OwnerName = owner.User.Name;
            AddNewRegion(x, y, x2, y2, entity_name);
        }


        protected void AddNewRegion(int x, int y, int x2, int y2, string entity_name)
        {
            string region_name = $"__House<>{OwnerName}<>{entity_name}";
            bool region_added = TShock.Regions.AddRegion(
                   x, y, x2 - x + 1, y2 - y + 1, region_name, OwnerName,
                   Main.worldID.ToString(), 100);
            Debugger.Log(1, "Region", $"region added did not return true returned {region_added} instead" );
            Debugger.Log(1, "Region", $"But does {TShock.Regions.GetRegionByName(region_name)} Exist?");
            if (region_added)
            {
                try
                {
                    RegionID = TShock.Regions.GetRegionByName(region_name).ID;
                    TShock.Regions.SetRegionState(RegionID, true);
                }
                catch(Exception ex)
                {
                    throw new Exception($"The Region You were trying to create does not exist by the name:{region_name}! the execption thrown was {ex.InnerException}");
                }
            }
            else
            {
                throw new Exception($"Error Tring to create region named: {region_name}");
            }
        }

        //Accessors
        public virtual ISet<string> AllowedUsernames { get; } = new HashSet<string>();

        public virtual int Area { get; }

        public Rectangle EnitityRectangle{ get; set; }
            
        public static int GetArea(HousingEntity entity) => entity.Area;


        public override string ToString() => GetType().Name;
        //attributes

        public Region EntityRegion { get { return TShock.Regions.GetRegionByID(RegionID); } }

        public int WorldID { get; set; }

        public int RegionID { get; set; }

        public int? PlayerID { get; set; }

        public string OwnerName { get; set; }

        public string EntityName { get; set; }
       
    }
}
