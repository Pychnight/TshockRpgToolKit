using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Mono.Data.Sqlite;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using Wolfje.Plugins.SEconomy;
using Housing.HousingEntites;

namespace Housing.Database
{
    /// <summary>
    ///     Represents a database manager for the plots and houses.
    /// </summary>
    public sealed class DatabaseManager
    {
        private readonly IDbConnection _connection;
        private readonly List<House> _houses = new List<House>();
        private readonly object _lock = new object();
        private readonly List<Shop> _shops = new List<Shop>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="DatabaseManager" /> class with the specified connection.
        /// </summary>
        /// <param name="connection">The connection, which must not be <c>null</c>.</param>
        public DatabaseManager(IDbConnection connection)
        {
            Debug.Assert(connection != null, "Connection must not be null.");

            _connection = connection;
            _connection.Query("CREATE TABLE IF NOT EXISTS Houses (" +
                              "  OwnerName TEXT," +
                              "  HouseName      TEXT," +
                              "  PlayerID  INTEGER," +
                              "  WorldID   INTEGER NOT NULL," +
                              "  RegionID  INTEGER NOT NULL," +
                              "  Debt      INTEGER DEFAULT 0," +
                              "  LastTaxed TEXT," +
                              "  ForSale   INTEGER DEFAULT 0," +
                              "  PRICE     INTEGER DEFAULT 0," +
                              "  HOUSE_ID  INTEGER PRIMARY KEY NOT NULL)");
            _connection.Query("CREATE TABLE IF NOT EXISTS Shops (" +
                              "  OwnerName  TEXT," +
                              "  ShopName   TEXT," +
                              "  WorldID    INTEGER NOT NULL," +
                              "  RegionID   INTEGER NOT NULL," +
                              "  PlayerID   INTEGER," +
                              "  ChestX     INTEGER," +
                              "  ChestY     INTEGER," +
                              "  OpenTime   TEXT," +
                              "  ClosingTime TEXT," +
                              "  IsOpen     INTEGER," +
                              "  Message    TEXT," +
                              "  SHOP_ID    INTEGER PRIMARY KEY NOT NULL)");

            _connection.Query("CREATE TABLE IF NOT EXISTS HouseHasUser (" +
                              "  OwnerName TEXT," +
                              "  HouseName TEXT," +
                              "  House_ID  INTEGER NOT NULL," +
                              "  WorldId   INTEGER," +
                              "  Username  TEXT," +
                              "  FOREIGN KEY(House_ID)" +
                              "    REFERENCES Houses(HOUSE_ID) ON DELETE CASCADE," +
                              "  UNIQUE(OwnerName, HouseName, WorldId, Username) ON CONFLICT IGNORE)");
            _connection.Query("CREATE TABLE IF NOT EXISTS ShopHasItem (" +
                              "  OwnerName          TEXT," +
                              "  ShopName           TEXT," +
                              "  WorldId            INTEGER," +
                              "  ItemIndex          INTEGER," +
                              "  ItemId             INTEGER," +
                              "  StackSize          INTEGER," +
                              "  PrefixId           INTEGER," +
                              "  Shop_ID            INTEGER NOT NULL," +
                              "  FOREIGN KEY(Shop_ID)" +
                              "    REFERENCES Shops(SHOP_ID) ON DELETE CASCADE," +
                              "  UNIQUE(OwnerName, ShopName, WorldId, ItemIndex) ON CONFLICT REPLACE)");
            _connection.Query("CREATE TABLE IF NOT EXISTS ShopHasPrice (" +
                              "  OwnerName          TEXT," +
                              "  ShopName           TEXT," +
                              "  WorldId            INTEGER," +
                              "  ItemId             INTEGER," +
                              "  UnitPrice          INTEGER," +
                              "  Shop_ID            INTEGER NOT NULL," +
                              "  FOREIGN KEY(Shop_ID)" +
                              "    REFERENCES Shops(SHOP_ID) ON DELETE CASCADE," +
                              "  UNIQUE(OwnerName, ShopName, WorldId, ItemId) ON CONFLICT REPLACE)");
        }

        /// <summary>
        ///     Adds a house with the specified properties.
        /// </summary>
        /// <param name="owner">The owner, which must not be <c>null</c>.</param>
        /// <param name="name">The name, which must not be <c>null</c> and logged in.</param>
        /// <param name="x">The first X coordinate.</param>
        /// <param name="y">The first Y coordinate.</param>
        /// <param name="x2">The second X coordinate, which must be at least the first.</param>
        /// <param name="y2">The second Y coordinate, which must be at least the second.</param>
        /// <returns>The resulting house.</returns>
        public House AddHouse(TSPlayer owner, string name, int x, int y, int x2, int y2)
        {
            Debug.Assert(name != null, "Name must not be null.");
            Debug.Assert(owner != null, "Owner must not be null.");
            //Debug.Assert(owner.User != null, "Owner must be logged in.");
            //Debug.Assert(x2 >= x, "Second X coordinate must be at least the first.");
            //Debug.Assert(y2 >= y, "Second Y coordinate must be at least the first.");

            lock (_lock)
            {
                var house = new House(owner, name, x, y, x2, y2);
                _connection.Query(
                    "INSERT INTO Houses (OwnerName, HouseName, WorldID, RegionID, PlayerID, LastTaxed)" +
                    "VALUES (@0, @1, @2, @3, @4, @5)",
                    house.OwnerName, house.HouseName, house.WorldID, house.RegionID, house.PlayerID, DateTime.UtcNow.ToString("s"));
                using (var reader = _connection.QueryReader("SELECT * FROM Houses WHERE WorldID = @0 AND OwnerName = @1 AND RegionID = @2 AND HouseName = @4", Main.worldID, house.OwnerName, house.RegionID, house.HouseName))
                {
                    while (reader.Read())
                    {
                        var houseid = reader.Get<int>("HOUSE_ID");
                        house.House_ID = houseid;
                    }
                }

                _houses.Add(house);
                return house;
            }
        }

        /// <summary>
        ///     Adds a shop with the specified properties.
        /// </summary>
        /// <param name="owner">The owner, which must not be <c>null</c>.</param>
        /// <param name="name">The name, which must not be <c>null</c> and logged in.</param>
        /// <param name="x">The first X coordinate.</param>
        /// <param name="y">The first Y coordinate.</param>
        /// <param name="x2">The second X coordinate, which must be at least the first.</param>
        /// <param name="y2">The second Y coordinate, which must be at least the second.</param>
        /// <param name="chestX">The chest X coordinate.</param>
        /// <param name="chestY">The chest Y coordinate.</param>
        /// <returns>The resulting shop.</returns>
        public Shop AddShop(TSPlayer owner, string name, int x, int y, int x2, int y2, int chestX, int chestY)
        {
            Debug.Assert(name != null, "Name must not be null.");
            Debug.Assert(owner != null, "Owner must not be null.");
            Debug.Assert(owner.User != null, "Owner must be logged in.");
            Debug.Assert(x2 >= x, "Second X coordinate must be at least the first.");
            Debug.Assert(y2 >= y, "Second Y coordinate must be at least the first.");
            
            lock (_lock)
            {
                var shop = new Shop(owner, name, x, y, x2, y2, chestX, chestY);
                _connection.Query(
                    "INSERT INTO Shops (OwnerName, ShopName, WorldID, RegionID, ChestX, ChestY, IsOpen)" +
                    "VALUES (@0, @1, @2, @3, @4, @5, @6)",
                    shop.OwnerName, shop.ShopName,shop.WorldID, shop.RegionID, shop.ChestX, shop.ChestY, 0);
                using (var reader = _connection.QueryReader("SELECT * FROM Shops WHERE WorldID = @0 AND OwnerName = @1 AND RegionID = @2 AND ShopName = @4", Main.worldID, shop.OwnerName, shop.RegionID, shop.ShopName))
                {
                    while (reader.Read())
                    {
                        var shopid = reader.Get<int>("SHOP_ID");
                        shop.Shop_ID = shopid;
                    }
                }
                _shops.Add(shop);
                return shop;
            }
        }

        /// <summary>
        ///     Gets the house containing the specified coordinates, or <c>null</c> if there is none.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>The house, or <c>null</c> if there is none.</returns>
        public House GetHouse(int x, int y)
        {
            lock (_lock)
            {
                return _houses.FirstOrDefault(h => h.EnitityRectangle.Contains(x, y));
            }
        }


        /// <summary>
        ///     Gets the houses.
        /// </summary>
        /// <returns>The houses.</returns>
        public IList<House> GetHouses()
        {
            lock (_lock)
            {
                return _houses.ToList();
            }
        }

        /// <summary>
        ///     Gets the shop containing the specified coordinates, or <c>null</c> if there is none.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>The shop, or <c>null</c> if there is none.</returns>
        public Shop GetShop(int x, int y)
        {
            lock (_lock)
            {
                return _shops.FirstOrDefault(h => h.EnitityRectangle.Contains(x, y));
            }
        }

        /// <summary>
        ///     Gets the shops.
        /// </summary>
        /// <returns>The shops.</returns>
        public IList<Shop> GetShops()
        {
            lock (_lock)
            {
                return _shops.ToList();
            }
        }

        /// <summary>
        ///     Loads the houses and shops.
        /// </summary>
        public void Load()
        {
            lock (_lock)
            {
                _houses.Clear();
                using (var reader = _connection.QueryReader("SELECT * FROM Houses WHERE WorldID = @0", Main.worldID))
                {
                    
                    while (reader.Read())
                    {
                        var houseId = reader.Get<int>("HOUSE_ID");
                        var ownerName = reader.Get<string>("OwnerName");
                        var name = reader.Get<string>("HouseName");
                        var regionId = reader.Get<int>("RegionID");
                        var playerId = reader.Get<int>("PlayerID");
                        var debt = (Money)reader.Get<long>("Debt");
                        var lastTaxed = DateTime.Parse(reader.Get<string>("LastTaxed"));
                        var forSale = reader.Get<int>("ForSale") == 1;
                        var price = (Money)reader.Get<long>("Price");

                        var house = new House(regionId, playerId, name, ownerName)
                        {
                            Debt = debt,
                            LastTaxed = lastTaxed,
                            ForSale = forSale,
                            Price = price,
                            House_ID = houseId
                        };
                        using (var reader2 = _connection.QueryReader(
                            "SELECT Username FROM HouseHasUser " +
                            "WHERE OwnerName = @0 AND HouseName = @1 AND WorldID = @2",
                            ownerName, name, Main.worldID))
                        {
                            while (reader2.Read())
                            {
                                var username = reader2.Get<string>("Username");
                                house.AllowedUsernames.Add(username);
                            }
                        }
                        _houses.Add(house);
                    }
                }

                _shops.Clear();
                using (var reader = _connection.QueryReader("SELECT * FROM Shops WHERE WorldID = @0", Main.worldID))
                {
                    while (reader.Read())
                    {
                        var shopID = reader.Get<int>("SHOP_ID");
                        var ownerName = reader.Get<string>("OwnerName");
                        var name = reader.Get<string>("ShopName");
                        var regionId = reader.Get<int>("RegionID");
                        var playerId = reader.Get<int>("PlayerID");
                        var chestX = reader.Get<int>("ChestX");
                        var chestY = reader.Get<int>("ChestY");
                        var isOpen = reader.Get<int>("IsOpen") == 1;
                        var message = reader.Get<string>("Message");

                        var shop = new Shop(regionId, playerId, name, ownerName,chestX, chestY)
                        {
                            IsOpen = isOpen,
                            Message = message,
                            Shop_ID = shopID
                        };
                        using (var reader2 = _connection.QueryReader(
                            "SELECT * FROM ShopHasItem WHERE OwnerName = @0 AND ShopName = @1 AND WorldId = @2",
                            ownerName, name, Main.worldID))
                        {
                            while (reader2.Read())
                            {
                                var index = reader2.Get<int>("ItemIndex");
                                var itemId = reader2.Get<int>("ItemId");
                                var stackSize = reader2.Get<int>("StackSize");
                                var prefixId = reader2.Get<byte>("PrefixId");
                                shop.Items.Add(new ShopItem(index, itemId, stackSize, prefixId));
                            }
                        }
                        using (var reader2 = _connection.QueryReader(
                            "SELECT * FROM ShopHasPrice WHERE OwnerName = @0 AND ShopName = @1 AND WorldId = @2",
                            ownerName, name, Main.worldID))
                        {
                            while (reader2.Read())
                            {
                                var itemId = reader2.Get<int>("ItemId");
                                var unitPrice = (Money)reader2.Get<long>("UnitPrice");
                                shop.UnitPrices[itemId] = unitPrice;
                            }
                        }
                        _shops.Add(shop);
                    }
                }
            }
        }

        /// <summary>
        ///     Removes the specified house.
        /// </summary>
        /// <param name="house">The shop, which must not be <c>null</c>.</param>
        public void Remove(House house)
        {
            Debug.Assert(house != null, "House must not be null.");

            lock (_lock)
            {
                TShock.Regions.DeleteRegion($"__House<>{house.OwnerName}<>{house.HouseName}");
                _connection.Query("DELETE FROM Houses WHERE HOUSE_ID = @0",
                                  house.House_ID);
                _houses.Remove(house);

                foreach (var shop in _shops.Where(s => house.EnitityRectangle.Contains(s.EnitityRectangle)))
                {
                    Remove(shop);
                }
            }
        }

        /// <summary>
        ///     Removes the specified shop.
        /// </summary>
        /// <param name="shop">The shop, which must not be <c>null</c>.</param>
        public void Remove(Shop shop)
        {
            Debug.Assert(shop != null, "House must not be null.");

            lock (_lock)
            {
                _connection.Query("DELETE FROM Shops WHERE SHOP_ID = @0",
                                  shop.Shop_ID);
                _shops.Remove(shop);
            }
        }

        /// <summary>
        ///     Updates the specified house.
        /// </summary>
        /// <param name="house">The shop, which must not be <c>null</c>.</param>
        public void Update(House house)
        {
            Debug.Assert(house != null, "House must not be null.");

            lock (_lock)
            {
                var region = house.EntityRegion;
                TShock.Regions.PositionRegion(house.EntityRegion.Name, house.EnitityRectangle.X, house.EnitityRectangle.Y, house.EnitityRectangle.Right - 1, house.EnitityRectangle.Bottom - 1);
                region.SetAllowedIDs(string.Join(",", house.AllowedUsernames.Select(au => TShock.Users.GetUserID(au))));
                
                _connection.Query(
                    "UPDATE Houses SET Debt = @0, LastTaxed = @1, ForSale = @2," +
                    "  Price = @3 WHERE HOUSE_ID = @4",
                    (long)house.Debt, house.LastTaxed.ToString("s"), house.ForSale ? 1 : 0, (long)house.Price,
                    house.House_ID);
                _connection.Query("DELETE FROM HouseHasUser WHERE OwnerName = @0 AND HouseName = @1 AND WorldId = @2",
                                  house.OwnerName, house.HouseName, Main.worldID);
                foreach (var username in house.AllowedUsernames)
                {
                    _connection.Query(
                        "INSERT INTO HouseHasUser (OwnerName, HouseName, WorldId, Username, House_ID) VALUES (@0, @1, @2, @3, @4)",
                        house.OwnerName, house.HouseName, Main.worldID, username, house.House_ID);
                }
            }
        }

        /// <summary>
        ///     Updates the specified shop.
        /// </summary>
        /// <param name="shop">The shop, which must not be <c>null</c>.</param>
        public void Update(Shop shop)
        {
            Debug.Assert(shop != null, "Shop must not be null.");

            lock (_lock)
            {
                var region = shop.EntityRegion;
                TShock.Regions.PositionRegion(shop.EntityRegion.Name, shop.EnitityRectangle.X, shop.EnitityRectangle.Y, shop.EnitityRectangle.Right - 1,
                                  shop.EnitityRectangle.Bottom - 1);
                _connection.Query("UPDATE Shops SET  IsOpen = @0, Message = @1 " +
                                  "WHERE SHOP_ID = @2"
                                 , shop.IsOpen ? 1 : 0, shop.Message, shop.Shop_ID);
                using (var db = _connection.CloneEx())
                {
                    db.Open();
                    using (var transaction = db.BeginTransaction())
                    {
                        using (var command = (SqliteCommand)db.CreateCommand())
                        {
                            command.CommandText = "INSERT INTO ShopHasItem (OwnerName, ShopName, WorldId, ItemIndex, " +
                                                  "  ItemId, StackSize, PrefixId, Shop_ID)" +
                                                  "VALUES (@0, @1, @2, @3, @4, @5, @6, @7)";
                            for (var i = 0; i <= 6; ++i)
                            {
                                command.AddParameter($"@{i}", null);
                            }
                            command.Parameters["@0"].Value = shop.OwnerName;
                            command.Parameters["@1"].Value = shop.ShopName;
                            command.Parameters["@2"].Value = Main.worldID;

                            foreach (var shopItem in shop.Items)
                            {
                                command.Parameters["@3"].Value = shopItem.Index;
                                command.Parameters["@4"].Value = shopItem.ItemId;
                                command.Parameters["@5"].Value = shopItem.StackSize;
                                command.Parameters["@6"].Value = shopItem.PrefixId;
                                command.Parameters["@7"].Value = shop.Shop_ID;
                                command.ExecuteNonQuery();
                            }
                        }
                        using (var command = (SqliteCommand)db.CreateCommand())
                        {
                            command.CommandText = "INSERT INTO ShopHasPrice (OwnerName, ShopName, WorldId, ItemId, " +
                                                  "  UnitPrice, Shop_ID)" +
                                                  "VALUES (@0, @1, @2, @3, @4, @5)";
                            for (var i = 0; i <= 4; ++i)
                            {
                                command.AddParameter($"@{i}", null);
                            }
                            command.Parameters["@0"].Value = shop.OwnerName;
                            command.Parameters["@1"].Value = shop.ShopName;
                            command.Parameters["@2"].Value = Main.worldID;

                            foreach (var kvp in shop.UnitPrices)
                            {
                                command.Parameters["@3"].Value = kvp.Key;
                                command.Parameters["@4"].Value = (long)kvp.Value;
                                command.Parameters["@5"].Value = shop.Shop_ID;
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
        }
    }
}
