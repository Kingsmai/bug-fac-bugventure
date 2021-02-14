using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace Engine
{
	/// <summary>
	/// 操作数据库的类
	/// </summary>
	public static class PlayerDataMapper
	{
		// 链接字符串：https://www.connectionstrings.com/
		private static readonly string _connectionString = 
			"server=localhost;" +
			"uid=root;" +
			"pwd=123456;" +
			"database=bugventure;";

		/// <summary>
		/// 从数据库创建角色
		/// </summary>
		/// <returns>如果创建失败，则返回null</returns>
		public static Player CreateFromDatabase()
		{
			try
			{
				// 连接数据库
				using (MySqlConnection connection = new MySqlConnection(_connectionString))
				{
					// 打开链接以执行SQL指令
					connection.Open();

					Player player;

					// 创建一个SQL命令对象，该对象使用到我们数据库的连接。SqlCommand对象是我们创建SQL语句的地方。
					using (MySqlCommand savedGameCommand = connection.CreateCommand())
					{
						savedGameCommand.CommandType = CommandType.Text;
						// 该SQL语句读取SavedGame表中的第一行。对于此程序，我们应该永远只有一行，但这将确保我们在SQL查询结果中仅获得一条记录。
						savedGameCommand.CommandText = "SELECT * FROM saved_game LIMIT 1";

						// 当您希望查询返回一行或多行时，请使用ExecuteReader
						MySqlDataReader reader = savedGameCommand.ExecuteReader();

						// 如果查询之后没有返回查询结果
						if (!reader.HasRows)
						{
							// 那就是在 saved_game 表里没有数据
							return null; // 没有玩家数据
						}

						// 获取查询结果
						reader.Read();

						// 从查询结果获取列的值
						int currentHitPoints = (int)reader["CurrentHitPoints"];
						int MaximumHitPoints = (int)reader["MaximumHitPoints"];
						int gold = (int)reader["Gold"];
						int experiencePoints = (int)reader["ExperiencePoints"];
						int currentLocationID = (int)reader["CurrentLocationID"];

						// 用保存的值创建玩家对象
						player = Player.CreatePlayerFromDatabase(currentHitPoints, MaximumHitPoints, gold, experiencePoints, currentLocationID);

						reader.Close();
					}

					// 读取Quest表中的数据，然后添加到玩家对象里
					using (MySqlCommand questCommand = connection.CreateCommand())
					{
						questCommand.CommandType = CommandType.Text;
						questCommand.CommandText = "SELECT * FROM quest";

						MySqlDataReader reader = questCommand.ExecuteReader();

						if (reader.HasRows)
						{
							while (reader.Read())
							{
								int questID = (int)reader["QuestID"];
								bool isCompleted = (bool)reader["IsCompleted"];

								// 利用当前数据创建PlayerQuest对象
								PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(questID));
								playerQuest.IsCompleted = isCompleted;

								// 添加该数据到角色属性里
								player.Quests.Add(playerQuest);
							}
						}

						reader.Close();
					}

					// 读取Inventory表中的数据，然后添加到玩家对象里
					using (MySqlCommand inventoryCommand = connection.CreateCommand())
					{
						inventoryCommand.CommandType = CommandType.Text;
						inventoryCommand.CommandText = "SELECT * FROM inventory";

						MySqlDataReader reader = inventoryCommand.ExecuteReader();

						if (reader.HasRows)
						{
							while (reader.Read())
							{
								int inventoryItemID = (int)reader["InventoryItemID"];
								int quantity = (int)reader["Quantity"];

								// 添加到角色物品栏
								player.AddItemToInventory(World.ItemByID(inventoryItemID), quantity);
							}
						}

						reader.Close();
					}

					return player;
				}
			}
			catch (Exception ex)
			{
				// 无视报错，如果报错，则返回null
			}

			return null;
		}

		/// <summary>
		/// 保存到数据库
		/// </summary>
		/// <param name="player">玩家对象</param>
		public static void SaveToDatabase(Player player)
		{
			try
			{
				using (MySqlConnection connection = new MySqlConnection(_connectionString))
				{
					// 打开链接以执行SQL指令
					connection.Open();

					// 插入/更新数据到saved_game表
					using (MySqlCommand existingRowCountCommand = connection.CreateCommand())
					{
						existingRowCountCommand.CommandType = CommandType.Text;
						existingRowCountCommand.CommandText = "SELECT count(*) FROM saved_game"; // 获取SQL查询之后返回多少行数据

						// 当你的查询会返回一个值时，用ExecuteScalar
						int existingRowCount = Convert.ToInt32(existingRowCountCommand.ExecuteScalar());

						if (existingRowCount == 0)
						{
							// 这里还没有已存在的行，所以我们需要插入INSERT
							using (MySqlCommand insertSavedGame = connection.CreateCommand())
							{
								insertSavedGame.CommandType = CommandType.Text;
								insertSavedGame.CommandText =
									"INSERT INTO saved_game " +
									"(CurrentHitPoints, MaximumHitPoints, Gold, ExperiencePoints, CurrentLocationID) " +
									"VALUES " +
									"(@CurrentHitPoints, @MaximumHitPoints, @Gold, @ExperiencePoints, @CurrentLocationID)";

								// 使用参数将值从播放器对象传递到SQL查询
								insertSavedGame.Parameters.Add("@CurrentHitPoints", MySqlDbType.Int32);
								insertSavedGame.Parameters["@CurrentHitPoints"].Value = player.CurrentHitPoints;
								insertSavedGame.Parameters.Add("@MaximumHitPoints", MySqlDbType.Int32);
								insertSavedGame.Parameters["@MaximumHitPoints"].Value = player.MaximumHitPoints;
								insertSavedGame.Parameters.Add("@Gold", MySqlDbType.Int32);
								insertSavedGame.Parameters["@Gold"].Value = player.Gold;
								insertSavedGame.Parameters.Add("@ExperiencePoints", MySqlDbType.Int32);
								insertSavedGame.Parameters["@ExperiencePoints"].Value = player.ExperiencePoints;
								insertSavedGame.Parameters.Add("@CurrentLocationID", MySqlDbType.Int32);
								insertSavedGame.Parameters["@CurrentLocationID"].Value = player.CurrentLocation.ID;

								// 执行SQL指令
								// 当SQL语句不会返回数据，则使用ExecuteNonQuery
								insertSavedGame.ExecuteNonQuery();
							}
						}
						else
						{
							// 这里已经有存在的行，所以需要更新UPDATE
							using (MySqlCommand updateSavedGame = connection.CreateCommand())
							{
								updateSavedGame.CommandType = CommandType.Text;
								updateSavedGame.CommandText =
									"UPDATE saved_game " +
									"SET " +
									"CurrentHitPoints = @CurrentHitPoints, " +
									"MaximumHitPoints = @MaximumHitPoints, " +
									"Gold = @Gold, " +
									"ExperiencePoints = @ExperiencePoints, " +
									"CurrentLocationID = @CurrentLocationID";

								// 使用参数有助于使程序更安全。
								// 它将防止SQL注入攻击。 
								updateSavedGame.Parameters.Add("@CurrentHitPoints", MySqlDbType.Int32);
								updateSavedGame.Parameters["@CurrentHitPoints"].Value = player.CurrentHitPoints;
								updateSavedGame.Parameters.Add("@MaximumHitPoints", MySqlDbType.Int32);
								updateSavedGame.Parameters["@MaximumHitPoints"].Value = player.MaximumHitPoints;
								updateSavedGame.Parameters.Add("@Gold", MySqlDbType.Int32);
								updateSavedGame.Parameters["@Gold"].Value = player.Gold;
								updateSavedGame.Parameters.Add("@ExperiencePoints", MySqlDbType.Int32);
								updateSavedGame.Parameters["@ExperiencePoints"].Value = player.ExperiencePoints;
								updateSavedGame.Parameters.Add("@CurrentLocationID", MySqlDbType.Int32);
								updateSavedGame.Parameters["@CurrentLocationID"].Value = player.CurrentLocation.ID;

								// 执行SQL语句
								updateSavedGame.ExecuteNonQuery();
							}
						}
					}

					// Quest和Inventory表在数据库中的行可能比播放器属性中的行多或少。
					// 因此，当我们保存玩家的游戏时，我们将删除所有旧行并添加所有新行。
					// 这比尝试添加 / 删除 / 更新每行要容易得多

					// 删除已存在的数据
					using (MySqlCommand deleteQuestCommand = connection.CreateCommand())
					{
						deleteQuestCommand.CommandType = CommandType.Text;
						deleteQuestCommand.CommandText = "DELETE FROM quest";

						deleteQuestCommand.ExecuteNonQuery();
					}

					// 插入数据
					foreach (PlayerQuest playerQuest in player.Quests)
					{
						using (MySqlCommand insertQuestCommand = connection.CreateCommand())
						{
							insertQuestCommand.CommandType = CommandType.Text;
							insertQuestCommand.CommandText =
								"INSERT INTO quest (QuestID, IsCompleted)" +
								"VALUES (@QuestID, @IsCompleted)";

							insertQuestCommand.Parameters.Add("@QuestID", MySqlDbType.Int32);
							insertQuestCommand.Parameters["@QuestID"].Value = playerQuest.Details.ID;
							insertQuestCommand.Parameters.Add("@IsCompleted", MySqlDbType.Bit);
							insertQuestCommand.Parameters["@IsCompleted"].Value = playerQuest.IsCompleted;

							insertQuestCommand.ExecuteNonQuery();
						}
					}

					// 删除已存在的物品栏数据
					using (MySqlCommand deleteInventoryCommand = connection.CreateCommand())
					{
						deleteInventoryCommand.CommandType = CommandType.Text;
						deleteInventoryCommand.CommandText = "DELETE FROM inventory";

						deleteInventoryCommand.ExecuteNonQuery();
					}

					// 插入数据
					foreach (InventoryItem inventoryItem in player.Inventory)
					{
						using(MySqlCommand insertInventoryCommand = connection.CreateCommand())
						{
							insertInventoryCommand.CommandType = CommandType.Text;
							insertInventoryCommand.CommandText =
								"INSERT INTO inventory (InventoryItemID, Quantity)" +
								"VALUES (@InventoryItemID, @Quantity)";

							insertInventoryCommand.Parameters.Add("@InventoryItemID", MySqlDbType.Int32);
							insertInventoryCommand.Parameters["@InventoryItemID"].Value = inventoryItem.Details.ID;
							insertInventoryCommand.Parameters.Add("@Quantity", MySqlDbType.Int32);
							insertInventoryCommand.Parameters["@Quantity"].Value = inventoryItem.Quantity;

							insertInventoryCommand.ExecuteNonQuery();
						}
					}
				}
			}
			catch (Exception ex)
			{
				// 暂时忽略异常
				Console.WriteLine(ex);
			}
		}
	}
}
