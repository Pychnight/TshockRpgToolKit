using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Housing.Models;
using MySql.Data.MySqlClient;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
//using Wolfje.Plugins.SEconomy;

namespace Housing.Database
{
	public class MySqlDatabase : DatabaseBase
	{
		internal const string DefaultDatabaseName = "db_housing";

		private readonly object locker = new object();
		
		public MySqlDatabase(string connectionString)
		{
			Debug.Assert(!string.IsNullOrWhiteSpace(connectionString), "Connection string must not be null or empty.");

			ConnectionString = ensureDatabase(connectionString);
			
			NonQuery("CREATE TABLE IF NOT EXISTS Houses (" +
								"  OwnerName VARCHAR(64)," +
								"  Name      VARCHAR(128)," +
								"  WorldId INTEGER," +
								"  X         INTEGER," +
								"  Y         INTEGER," +
								"  X2        INTEGER," +
								"  Y2        INTEGER," +
								"  Debt      INTEGER DEFAULT 0," +
								"  LastTaxed TEXT," +
								"  ForSale   INTEGER DEFAULT 0," +
								"  PRICE     INTEGER DEFAULT 0," +
								"  PRIMARY KEY(OwnerName, Name, WorldId))");

			NonQuery("CREATE TABLE IF NOT EXISTS Shops (" +
								"  OwnerName  VARCHAR(64)," +
								"  Name       VARCHAR(128)," +
								"  WorldId    INTEGER," +
								"  X          INTEGER," +
								"  Y          INTEGER," +
								"  X2         INTEGER," +
								"  Y2         INTEGER," +
								"  ChestX     INTEGER," +
								"  ChestY     INTEGER," +
								"  IsOpen     INTEGER," +
								"  Message    TEXT," +
								"  PRIMARY KEY(OwnerName, Name, WorldId))");

			NonQuery("CREATE TABLE IF NOT EXISTS HouseHasUser (" +
								"  OwnerName VARCHAR(64)," +
								"  HouseName VARCHAR(128)," +
								"  WorldId   INTEGER," +
								"  Username  VARCHAR(64)," +
								"  FOREIGN KEY(OwnerName, HouseName, WorldId)" +
								"    REFERENCES Houses(OwnerName, Name, WorldId) ON DELETE CASCADE," +
								"  UNIQUE(OwnerName, HouseName, WorldId, Username))");

			NonQuery("CREATE TABLE IF NOT EXISTS ShopHasItem (" +
								"  OwnerName          VARCHAR(64)," +
								"  ShopName           VARCHAR(128)," +
								"  WorldId            INTEGER," +
								"  ItemIndex          INTEGER," +
								"  ItemId             INTEGER," +
								"  StackSize          INTEGER," +
								"  PrefixId           INTEGER," +
								"  FOREIGN KEY(OwnerName, ShopName, WorldId)" +
								"    REFERENCES Shops(OwnerName, Name, WorldId) ON DELETE CASCADE," +
								"  UNIQUE(OwnerName, ShopName, WorldId, ItemIndex))");

			NonQuery("CREATE TABLE IF NOT EXISTS ShopHasPrice (" +
								"  OwnerName          VARCHAR(64)," +
								"  ShopName           VARCHAR(128)," +
								"  WorldId            INTEGER," +
								"  ItemId             INTEGER," +
								"  UnitPrice          INTEGER," +
								"  FOREIGN KEY(OwnerName, ShopName, WorldId)" +
								"    REFERENCES Shops(OwnerName, Name, WorldId) ON DELETE CASCADE," +
								"  UNIQUE(OwnerName, ShopName, WorldId, ItemId))");

			NonQuery("CREATE TABLE IF NOT EXISTS TaxCollectors (" +
								"  WorldId            INTEGER NOT NULL," +
								"  PlayerName         VARCHAR(64) NOT NULL," +
								"  PRIMARY KEY (WorldId, PlayerName) )");
		}

		private string ensureDatabase(string connectionString)
		{
			var builder = new MySqlConnectionStringBuilder(connectionString);
			var dbName = string.IsNullOrWhiteSpace(builder.Database) ? DefaultDatabaseName : builder.Database;

			builder.Database = null;

			//try to create db
			using( var con = new MySqlConnection(builder.ConnectionString) )
			using( var cmd = con.CreateCommand() )
			{
				cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS {dbName}";
				con.Open();
				cmd.ExecuteNonQuery();
			}

			builder.Database = dbName;

			return builder.ConnectionString;
		}

		private int NonQuery(string query)
		{
			using( var con = new MySqlConnection(ConnectionString) )
			{
				using( var cmd = con.CreateCommand() )
				{
					cmd.CommandText = query;

					con.Open();
					return cmd.ExecuteNonQuery();
				}
			}
		}

		private int NonQuery(string query, params object[] args)
		{
			using( var con = new MySqlConnection(ConnectionString) )
			{
				using( var cmd = con.CreateCommand() )
				{
					cmd.CommandText = query;

					for( var i = 0; i < args.Length; i++ )
						cmd.AddParameter($"@{i}", args[i]);
					
					con.Open();
					return cmd.ExecuteNonQuery();
				}
			}
		}
		
		private IDbCommand QueryCommand(IDbConnection connection, string query, params object[] args)
		{
			var cmd = connection.CreateCommand();

			cmd.CommandText = query;

			for( var i = 0; i < args.Length; i++ )
				cmd.AddParameter($"@{i}", args[i]);

			connection.Open();

			return cmd;
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
		public override House AddHouse(TSPlayer owner, string name, int x, int y, int x2, int y2)
		{
			Debug.Assert(name != null, "Name must not be null.");
			Debug.Assert(owner != null, "Owner must not be null.");
			Debug.Assert(owner.User != null, "Owner must be logged in.");
			Debug.Assert(x2 >= x, "Second X coordinate must be at least the first.");
			Debug.Assert(y2 >= y, "Second Y coordinate must be at least the first.");

			lock( locker )
			{
				TShock.Regions.AddRegion(
					x, y, x2 - x + 1, y2 - y + 1, $"__House<>{owner.User.Name}<>{name}", owner.User.Name,
					Main.worldID.ToString(), 100);
				NonQuery(
					"INSERT INTO Houses (OwnerName, Name, WorldId, X, Y, X2, Y2, LastTaxed)" +
					"VALUES (@0, @1, @2, @3, @4, @5, @6, @7)",
					owner.User.Name, name, Main.worldID, x, y, x2, y2, DateTime.UtcNow.ToString("s"));
				var house = new House(owner.User.Name, name, x, y, x2, y2);
				Houses.Add(house);
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
		public override Shop AddShop(TSPlayer owner, string name, int x, int y, int x2, int y2, int chestX, int chestY)
		{
			Debug.Assert(name != null, "Name must not be null.");
			Debug.Assert(owner != null, "Owner must not be null.");
			Debug.Assert(owner.User != null, "Owner must be logged in.");
			Debug.Assert(x2 >= x, "Second X coordinate must be at least the first.");
			Debug.Assert(y2 >= y, "Second Y coordinate must be at least the first.");

			lock( locker )
			{
				NonQuery(
					"INSERT INTO Shops (OwnerName, Name, WorldId, X, Y, X2, Y2, ChestX, ChestY, IsOpen)" +
					"VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9)",
					owner.User.Name, name, Main.worldID, x, y, x2, y2, chestX, chestY, 0);
				var shop = new Shop(owner.User.Name, name, x, y, x2, y2, chestX, chestY);
				Shops.Add(shop);
				return shop;
			}
		}

		/// <summary>
		/// Adds a player name to the tax collector list.
		/// </summary>
		/// <param name="playerName"></param>
		public override TaxCollector AddTaxCollector(string playerName)
		{
			Debug.Assert(playerName != null, "playerName must not be null.");

			lock( locker )
			{
				if( TaxCollectorNames.Contains(playerName) )
					return null;

				NonQuery(
					"INSERT INTO TaxCollectors (WorldId, PlayerName)" +
					"VALUES (@0, @1)",
					Main.worldID, playerName);

				var tc = new TaxCollector(playerName);
				TaxCollectors.Add(tc);
				TaxCollectorNames.Add(playerName);
				return tc;
			}
		}

		/// <summary>
		///     Loads the houses and shops.
		/// </summary>
		public override void Load()
		{
			lock( locker )
			{
				Houses.Clear();
				using( var connection = new MySqlConnection(ConnectionString) )
				using( var command = QueryCommand(connection, "SELECT * FROM Houses WHERE WorldId = @0", Main.worldID) )
				using( var reader = command.ExecuteReader() )
				{
					while( reader.Read() )
					{
						var ownerName = reader.Get<string>("OwnerName");
						var name = reader.Get<string>("Name");
						var x = reader.Get<int>("X");
						var y = reader.Get<int>("Y");
						var x2 = reader.Get<int>("X2");
						var y2 = reader.Get<int>("Y2");
						var debt = (decimal)reader.Get<long>("Debt");
						var lastTaxed = DateTime.Parse(reader.Get<string>("LastTaxed"));
						var forSale = reader.Get<int>("ForSale") == 1;
						var price = (decimal)reader.Get<long>("Price");

						var house = new House(ownerName, name, x, y, x2, y2)
						{
							Debt = debt,
							LastTaxed = lastTaxed,
							ForSale = forSale,
							Price = price
						};
						using( var connection2 = new MySqlConnection(ConnectionString) )
						using( var command2 = QueryCommand(connection2,
							"SELECT Username FROM HouseHasUser " +
							"WHERE OwnerName = @0 AND HouseName = @1 AND WorldId = @2",
							ownerName, name, Main.worldID) )
						using( var reader2 = command2.ExecuteReader())
						{
							while( reader2.Read() )
							{
								var username = reader2.Get<string>("Username");
								house.AllowedUsernames.Add(username);
							}
						}
						Houses.Add(house);
					}
				}

				Shops.Clear();
				using( var connection = new MySqlConnection(ConnectionString) )
				using( var command = QueryCommand(connection, "SELECT * FROM Shops WHERE WorldID = @0", Main.worldID) )
				using( var reader = command.ExecuteReader())
				{
					while( reader.Read() )
					{
						var ownerName = reader.Get<string>("OwnerName");
						var name = reader.Get<string>("Name");
						var x = reader.Get<int>("X");
						var y = reader.Get<int>("Y");
						var x2 = reader.Get<int>("X2");
						var y2 = reader.Get<int>("X2");
						var chestX = reader.Get<int>("ChestX");
						var chestY = reader.Get<int>("ChestY");
						var isOpen = reader.Get<int>("IsOpen") == 1;
						var message = reader.Get<string>("Message");

						var shop = new Shop(ownerName, name, x, y, x2, y2, chestX, chestY)
						{
							IsOpen = isOpen,
							Message = message
						};
						using( var connection2 = new MySqlConnection(ConnectionString) )
						using( var command2 = QueryCommand(connection2,
							"SELECT * FROM ShopHasItem WHERE OwnerName = @0 AND ShopName = @1 AND WorldId = @2",
							ownerName, name, Main.worldID) )
						using( var reader2 = command2.ExecuteReader())
						{
							while( reader2.Read() )
							{
								var index = reader2.Get<int>("ItemIndex");
								var itemId = reader2.Get<int>("ItemId");
								var stackSize = reader2.Get<int>("StackSize");
								var prefixId = (byte)reader2.Get<int>("PrefixId");
								shop.Items.Add(new ShopItem(index, itemId, stackSize, prefixId));
							}
						}
						using( var connection2 = new MySqlConnection(ConnectionString) )
						using( var command2 = QueryCommand(connection2,
							"SELECT * FROM ShopHasPrice WHERE OwnerName = @0 AND ShopName = @1 AND WorldId = @2",
							ownerName, name, Main.worldID) )
						using( var reader2 = command2.ExecuteReader())
						{
							while( reader2.Read() )
							{
								var itemId = reader2.Get<int>("ItemId");
								var unitPrice = (decimal)reader2.Get<long>("UnitPrice");
								shop.UnitPrices[itemId] = unitPrice;
							}
						}
						Shops.Add(shop);
					}
				}

				//load in tax collectors.
				TaxCollectors.Clear();
				TaxCollectorNames.Clear();
				using( var connection = new MySqlConnection(ConnectionString) )
				using( var command = QueryCommand(connection, "SELECT * FROM TaxCollectors WHERE WorldID = @0", Main.worldID) )
				using( var reader = command.ExecuteReader())
				{
					while( reader.Read() )
					{
						var playerName = reader.Get<string>("PlayerName");
						var tc = new TaxCollector(playerName);
						TaxCollectors.Add(tc);
						TaxCollectorNames.Add(playerName);
					}
				}
			}
		}

		/// <summary>
		///     Removes the specified house.
		/// </summary>
		/// <param name="house">The shop, which must not be <c>null</c>.</param>
		public override void Remove(House house)
		{
			Debug.Assert(house != null, "House must not be null.");

			lock( locker )
			{
				TShock.Regions.DeleteRegion($"__House<>{house.OwnerName}<>{house.Name}");
				NonQuery("DELETE FROM Houses WHERE OwnerName = @0 AND Name = @1 AND WorldId = @2",
								  house.OwnerName, house.Name, Main.worldID);
				Houses.Remove(house);

				foreach( var shop in Shops.Where(s => house.Rectangle.Contains(s.Rectangle)) )
				{
					Remove(shop);
				}
			}
		}

		/// <summary>
		///     Removes the specified shop.
		/// </summary>
		/// <param name="shop">The shop, which must not be <c>null</c>.</param>
		public override void Remove(Shop shop)
		{
			Debug.Assert(shop != null, "House must not be null.");

			lock( locker )
			{
				NonQuery("DELETE FROM Shops WHERE OwnerName = @0 AND Name = @1 AND WorldId = @2",
								  shop.OwnerName, shop.Name, Main.worldID);
				Shops.Remove(shop);
			}
		}

		/// <summary>
		///     Removes the specified tax collector.
		/// </summary>
		/// <param name="taxCollector">The tax collector, which must not be <c>null</c>.</param>
		public override void Remove(TaxCollector taxCollector)
		{
			Debug.Assert(taxCollector != null, "playerName must not be null.");

			lock( locker )
			{
				var indexOf = TaxCollectors.IndexOf(taxCollector);

				if( indexOf == -1 )
					return;

				NonQuery("DELETE FROM TaxCollectors WHERE WorldId = @0 AND PlayerName = @1",
								  Main.worldID, taxCollector.PlayerName);
				TaxCollectors.RemoveAt(indexOf);
			}
		}

		/// <summary>
		///     Updates the specified house.
		/// </summary>
		/// <param name="house">The shop, which must not be <c>null</c>.</param>
		public override void Update(House house)
		{
			Debug.Assert(house != null, "House must not be null.");

			lock( locker )
			{
				var region = TShock.Regions.GetRegionByName($"__House<>{house.OwnerName}<>{house.Name}");
				region.SetAllowedIDs(string.Join(",", house.AllowedUsernames.Select(au => TShock.Users.GetUserID(au))));

				NonQuery(
					"UPDATE Houses SET X = @0, Y = @1, X2 = @2, Y2 = @3, Debt = @4, LastTaxed = @5, ForSale = @6," +
					"  Price = @7 WHERE OwnerName = @8 AND Name = @9 AND WorldId = @10",
					house.Rectangle.X, house.Rectangle.Y, house.Rectangle.Right - 1, house.Rectangle.Bottom - 1,
					(long)house.Debt, house.LastTaxed.ToString("s"), house.ForSale ? 1 : 0, (long)house.Price,
					house.OwnerName, house.Name, Main.worldID);
				NonQuery("DELETE FROM HouseHasUser WHERE OwnerName = @0 AND HouseName = @1 AND WorldId = @2",
								  house.OwnerName, house.Name, Main.worldID);
				foreach( var username in house.AllowedUsernames )
				{
					NonQuery(
						"INSERT IGNORE INTO HouseHasUser (OwnerName, HouseName, WorldId, Username) VALUES (@0, @1, @2, @3)",
						house.OwnerName, house.Name, Main.worldID, username);
				}
			}
		}

		/// <summary>
		///     Updates the specified shop.
		/// </summary>
		/// <param name="shop">The shop, which must not be <c>null</c>.</param>
		public override void Update(Shop shop)
		{
			Debug.Assert(shop != null, "Shop must not be null.");

			lock( locker )
			{
				NonQuery("UPDATE Shops SET X = @0, Y = @1, X2 = @2, Y2 = @3, IsOpen = @4, Message = @5 " +
								  "WHERE OwnerName = @6 AND Name = @7 AND WorldId = @8",
								  shop.Rectangle.X, shop.Rectangle.Y, shop.Rectangle.Right - 1,
								  shop.Rectangle.Bottom - 1, shop.IsOpen ? 1 : 0, shop.Message, shop.OwnerName,
								  shop.Name, Main.worldID);
				using( var connection = new MySqlConnection(ConnectionString) )
				{
					connection.Open();
					using( var transaction = connection.BeginTransaction() )
					{
						using( var command = connection.CreateCommand() )
						{
							command.CommandText = "REPLACE INTO ShopHasItem (OwnerName, ShopName, WorldId, ItemIndex, " +
												  "  ItemId, StackSize, PrefixId)" +
												  "VALUES (@0, @1, @2, @3, @4, @5, @6)";
							for( var i = 0; i <= 6; ++i )
							{
								command.AddParameter($"@{i}", null);
							}
							command.Parameters["@0"].Value = shop.OwnerName;
							command.Parameters["@1"].Value = shop.Name;
							command.Parameters["@2"].Value = Main.worldID;

							foreach( var shopItem in shop.Items )
							{
								command.Parameters["@3"].Value = shopItem.Index;
								command.Parameters["@4"].Value = shopItem.ItemId;
								command.Parameters["@5"].Value = shopItem.StackSize;
								command.Parameters["@6"].Value = shopItem.PrefixId;
								command.ExecuteNonQuery();
							}
						}
						using( var command = connection.CreateCommand() )
						{
							command.CommandText = "REPLACE INTO ShopHasPrice (OwnerName, ShopName, WorldId, ItemId, " +
												  "  UnitPrice)" +
												  "VALUES (@0, @1, @2, @3, @4)";
							for( var i = 0; i <= 4; ++i )
							{
								command.AddParameter($"@{i}", null);
							}
							command.Parameters["@0"].Value = shop.OwnerName;
							command.Parameters["@1"].Value = shop.Name;
							command.Parameters["@2"].Value = Main.worldID;

							foreach( var kvp in shop.UnitPrices )
							{
								command.Parameters["@3"].Value = kvp.Key;
								command.Parameters["@4"].Value = (long)kvp.Value;
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
