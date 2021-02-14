using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using Engine;

namespace BugVentureConsole
{
	public class Program
	{
		private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

		private static Player _player;

		static void Main(string[] args)
		{
			// 加载玩家
			LoadGameData();

			Console.WriteLine("Type 'Help' to see a list of commands");
			Console.WriteLine("");

			DisplayCurrentLocation();

			// 链接玩家事件到显示UI的函数
			_player.PropertyChanged += Player_OnPropertyChanged;
			_player.OnMessage += Player_OnMessage;

			// 死循环，知道玩家输入exit
			while (true)
			{
				// 显示提示，以便用户知道要输入
				Console.Write("> ");

				// 等待用户输入并按下<Enter>。
				string userInput = Console.ReadLine();

				if (string.IsNullOrWhiteSpace(userInput))
				{
					continue;
				}

				// 转换成小写，以便容易比较
				string cleanedInput = userInput.ToLower();

				// 保存游戏数据，然后结束循环
				if ("exit".Equals(cleanedInput))
				{
					SaveGameData();

					break;
				}

				// 如果用户输入了一些东西，尝试将它做信息处理
				ParseInput(cleanedInput);
			}
		}

		private static void Player_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentLocation")
			{
				DisplayCurrentLocation();

				if (_player.CurrentLocation.VendorWorkingHere != null)
				{
					Console.WriteLine("You see a vendor here: {0}", _player.CurrentLocation.VendorWorkingHere.Name);
				}
			}
		}

		private static void Player_OnMessage(object sender, MessageEventArgs e)
		{
			Console.WriteLine(e.Message);

			if (e.AddExtraNewLine)
			{
				Console.WriteLine("");
			}
		}

		private static void ParseInput(string input)
		{
			if (input.Contains("help") || input == "?")
			{
				Console.WriteLine("Available commands");
				Console.WriteLine("====================================");
				Console.WriteLine("Stats - Display player information");
				Console.WriteLine("Look - Get the description of your location");
				Console.WriteLine("Inventory - Display your inventory");
				Console.WriteLine("Quests - Display your quests");
				Console.WriteLine("Attack - Fight the monster");
				Console.WriteLine("Equip <weapon name> - Set your current weapon");
				Console.WriteLine("Drink <potion name> - Drink a potion");
				Console.WriteLine("Trade - display your inventory and vendor's inventory");
				Console.WriteLine("Buy <item name> - Buy an item from a vendor");
				Console.WriteLine("Sell <item name> - Sell an item to a vendor");
				Console.WriteLine("North - Move North");
				Console.WriteLine("South - Move South");
				Console.WriteLine("East - Move East");
				Console.WriteLine("West - Move West");
				Console.WriteLine("Exit - Save the game and exit");
			}
			else if (input == "stats")
			{
				Console.WriteLine("Current hit points: {0}", _player.CurrentHitPoints);
				Console.WriteLine("Maximum hit points: {0}", _player.MaximumHitPoints);
				Console.WriteLine("Experience Points: {0}", _player.ExperiencePoints);
				Console.WriteLine("Level: {0}", _player.Level);
				Console.WriteLine("Gold: {0}", _player.Gold);
			}
			else if (input == "look")
			{
				DisplayCurrentLocation();
			}
			else if (input.Contains("north"))
			{
				if (_player.CurrentLocation.LocationToNorth == null)
				{
					Console.WriteLine("You cannot move North");
				}
				else
				{
					_player.MoveNorth();
				}
			}
			else if (input.Contains("east"))
			{
				if (_player.CurrentLocation.LocationToEast == null)
				{
					Console.WriteLine("You cannot move East");
				}
				else
				{
					_player.MoveEast();
				}
			}
			else if (input.Contains("south"))
			{
				if (_player.CurrentLocation.LocationToSouth == null)
				{
					Console.WriteLine("You cannot move South");
				}
				else
				{
					_player.MoveSouth();
				}
			}
			else if (input.Contains("west"))
			{
				if (_player.CurrentLocation.LocationToWest == null)
				{
					Console.WriteLine("You cannot move West");
				}
				else
				{
					_player.MoveWest();
				}
			}
			else if (input == "inventory")
			{
				foreach (InventoryItem inventoryItem in _player.Inventory)
				{
					Console.WriteLine("{0}: {1}", inventoryItem.Description, inventoryItem.Quantity);
				}
			}
			else if (input == "quests")
			{
				if (_player.Quests.Count == 0)
				{
					Console.WriteLine("You don't have any quests");
				}
				else
				{
					foreach (PlayerQuest playerQuest in _player.Quests)
					{
						Console.WriteLine("{0} : {1}", playerQuest.Name, playerQuest.IsCompleted ? "Completed" : "Incomplete");
					}
				}
			}
			else if (input.Contains("attack"))
			{
				if (_player.CurrentLocation.MonsterLivingHere == null)
				{
					Console.WriteLine("There is nothing here to attack");
				}
				else
				{
					if (_player.CurrentWeapon == null)
					{
						// 选择玩家第一个武器（如果没有武器，则返回null）
						_player.CurrentWeapon = _player.Weapons.FirstOrDefault();
					}

					if (_player.CurrentWeapon == null)
					{
						Console.WriteLine("You don't have any weapons");
					}
					else
					{
						_player.UseWeapon(_player.CurrentWeapon);
					}
				}
			}
			else if (input.StartsWith("equip "))
			{
				string inputWeaponName = input.Substring(6).Trim();

				if (string.IsNullOrEmpty(inputWeaponName))
				{
					Console.WriteLine("You must enter the name of the weapon to equip");
				}
				else
				{
					Weapon weaponToEquip = _player.Weapons.SingleOrDefault(x => x.Name.ToLower() == inputWeaponName || x.NamePlural == inputWeaponName);

					if (weaponToEquip == null)
					{
						Console.WriteLine("You do not have the weapon: {0}", inputWeaponName);
					}
					else
					{
						_player.CurrentWeapon = weaponToEquip;

						Console.WriteLine("You equip your {0}", _player.CurrentWeapon.Name);
					}
				}
			}
			else if (input.StartsWith("drink "))
			{
				string inputPotionName = input.Substring(6).Trim();

				if (string.IsNullOrEmpty(inputPotionName))
				{
					Console.WriteLine("You must enter the name of the potion to drink");
				}
				else
				{
					HealingPotion potionToDrink = _player.Potions.SingleOrDefault(x => x.Name.ToLower() == inputPotionName || x.NamePlural.ToLower() == inputPotionName);

					if (potionToDrink == null)
					{
						Console.WriteLine("You do not have the potion: {0}", inputPotionName);
					}
					else
					{
						_player.UsePotion(potionToDrink);
					}
				}
			}
			else if (input == "trade")
			{
				if (_player.CurrentLocation.VendorWorkingHere == null)
				{
					Console.WriteLine("There is no vendor here");
				}
				else
				{
					Console.WriteLine("PLAYER INVENTORY");
					Console.WriteLine("================");

					if (_player.Inventory.Count(x => x.Price != World.UNSELLABLE_ITEM_PRICE) == 0)
					{
						Console.WriteLine("You do not have any inventory");
					}
					else
					{
						foreach (InventoryItem inventoryItem in _player.Inventory.Where(x => x.Price != World.UNSELLABLE_ITEM_PRICE))
						{
							Console.WriteLine("{0} {1} Price: {2}", inventoryItem.Quantity, inventoryItem.Description, inventoryItem.Price);
						}
					}

					Console.WriteLine("");

					Console.WriteLine("VENDOR INVENTORY");
					Console.WriteLine("================");

					if (_player.CurrentLocation.VendorWorkingHere.Inventory.Count == 0)
					{
						Console.WriteLine("The vendor does not have any inventory");
					}
					else
					{
						foreach (InventoryItem inventoryItem in _player.CurrentLocation.VendorWorkingHere.Inventory)
						{
							Console.WriteLine("{0} {1} Price: {2}", inventoryItem.Quantity, inventoryItem.Description, inventoryItem.Price);
						}
					}
				}
			}
			else if (input.StartsWith("buy "))
			{
				if (_player.CurrentLocation.VendorWorkingHere == null)
				{
					Console.WriteLine("There is no vendor at this location");
				}
				else
				{
					string itemName = input.Substring(4).Trim();

					if (string.IsNullOrEmpty(itemName))
					{
						Console.WriteLine("You must enter the name of the item to buy");
					}
					else
					{
						// 从商家物品栏获取道具
						InventoryItem itemToBuy = _player.CurrentLocation.VendorWorkingHere.Inventory.SingleOrDefault(x => x.Details.Name.ToLower() == itemName);

						// 检查商家是否拥有该物品
						if (itemToBuy == null)
						{
							Console.WriteLine("The vendor does not have any {0}", itemName);
						}
						else
						{
							// 检查玩家是否拥有足够的金钱购买
							if (_player.Gold < itemToBuy.Price)
							{
								Console.WriteLine("You do not have enough gold to buy a {0}", itemToBuy.Description);
							}
							else
							{
								// 成功购买商品
								_player.AddItemToInventory(itemToBuy.Details);
								_player.Gold -= itemToBuy.Price;

								Console.WriteLine("You bought one {0} for {1} gold", itemToBuy.Details.Name, itemToBuy.Price);
							}
						}
					}
				}
			}
			else if (input.StartsWith("sell "))
			{
				if (_player.CurrentLocation.VendorWorkingHere == null)
				{
					Console.WriteLine("There is no vendor at this location");
				}
				else
				{
					string itemName = input.Substring(5).Trim();

					if (string.IsNullOrEmpty(itemName))
					{
						Console.WriteLine("You must enter the name of the item to sell");
					}
					else
					{
						// 获取玩家物品栏里的物品
						InventoryItem itemToSell = _player.Inventory.SingleOrDefault(x => x.Details.Name.ToLower() == itemName && x.Quantity > 0 && x.Price != World.UNSELLABLE_ITEM_PRICE);

						// 检查玩家是否有该道具
						if (itemToSell == null)
						{
							Console.WriteLine("The player cannot sell any {0}", itemName);
						}
						else
						{
							// 卖出物品
							_player.RemoveItemFromInventory(itemToSell.Details, 1);
							_player.Gold += itemToSell.Price;

							Console.WriteLine("You receive {0} gold for your {1}", itemToSell.Price, itemToSell.Details.Name);
						}
					}
				}
			}
			else
			{
				Console.WriteLine("I do not understand");
				Console.WriteLine("Type 'Help' to see a list of available commands");
			}

			// 空一行，让UI看上去更干净一些
			Console.WriteLine("");
		}

		private static void DisplayCurrentLocation()
		{
			Console.WriteLine("You are at: {0}", _player.CurrentLocation.Name);

			if (_player.CurrentLocation.Description != "")
			{
				Console.WriteLine(_player.CurrentLocation.Description);
			}
		}

		private static void LoadGameData()
		{
			_player = PlayerDataMapper.CreateFromDatabase();

			if (_player == null)
			{
				if (File.Exists(PLAYER_DATA_FILE_NAME))
				{
					_player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
				}
				else
				{
					_player = Player.CreateDefaultPlayer();
				}
			}
		}

		private static void SaveGameData()
		{
			File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXMLString());

			PlayerDataMapper.SaveToDatabase(_player);
		}
	}
}
