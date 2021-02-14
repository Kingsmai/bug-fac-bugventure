using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;

namespace Engine
{
	public class Player : LivingCreature
	{
		// 金币
		private int _gold;
		public int Gold
		{
			get { return _gold; }
			set
			{
				_gold = value;
				OnPropertyChanged("Gold");
			}
		}
		// 经验值
		private int _experiencePoints;
		public int ExperiencePoints
		{
			get { return _experiencePoints; }
			private set
			{
				_experiencePoints = value;
				OnPropertyChanged("ExperiencePoints");
				OnPropertyChanged("Level"); // 我们没有set等级的值，它的值是被计算的，所以根据经验值直接发出通知就行
			}
		}
		// 等级
		public int Level
		{
			// 每一次获取Level的值时，都会计算
			// 因为时整数型，会向下取整，所以需要+1
			get { return ((ExperiencePoints / 100) + 1); }
			// 我们不需要手动修改等级，所以把set删掉了。
		}
		// 物品栏
		// To bind a list property, you need to change its datatype to either BindingList or
		// ObservableCollection.BindingList gives more options than ObservableCollection – like searching and sorting.
		public BindingList<InventoryItem> Inventory { get; set; }
		// 当前任务
		public BindingList<PlayerQuest> Quests { get; set; }
		// 当前拥有武器列表
		public List<Weapon> Weapons
		{
			// 如果列表元素是武器类，则把该对象转成新的list（我们只需要InventoryItems的Details属性，不需要Quantity属性）
			get { return Inventory.Where(x => x.Details is Weapon).Select(x => x.Details as Weapon).ToList(); }
		}
		// 当前拥有药水列表
		public List<HealingPotion> Potions
		{
			get { return Inventory.Where(x => x.Details is HealingPotion).Select(x => x.Details as HealingPotion).ToList(); }
		}
		// 当前位置
		private Location _currentLocation;
		public Location CurrentLocation
		{
			get { return _currentLocation; }
			set
			{
				_currentLocation = value;
				OnPropertyChanged("CurrentLocation");
			}
		}
		// 当前使用武器
		public Weapon CurrentWeapon { get; set; }
		// 当前面对的怪物
		private Monster _currentMonster;

		/// <summary>
		/// 玩家对象
		/// </summary>
		/// <param name="currentHitPoints">当前生命值</param>
		/// <param name="maximumHitPoints">最大生命值</param>
		/// <param name="gold">当前拥有金币</param>
		/// <param name="experiencePoints">经验值</param>
		private Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints)
			: base(currentHitPoints, maximumHitPoints)
		{
			Gold = gold;
			ExperiencePoints = experiencePoints;

			Inventory = new BindingList<InventoryItem>();
			Quests = new BindingList<PlayerQuest>();
		}

		/// <summary>
		/// 创建新的默认角色
		/// </summary>
		/// <returns>创建的角色</returns>
		public static Player CreateDefaultPlayer()
		{
			Player player = new Player(10, 10, 20, 0);
			// 初始道具（锈剑）
			player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
			// 初始地点（家）
			player.CurrentLocation = World.LocationByID(World.LOCATION_ID_HOME);

			return player;
		}

		/// <summary>
		/// 增加角色经验值，并计算最大生命值
		/// </summary>
		/// <param name="experiencePointsToAdd">需要增加的生命值</param>
		public void AddExperiencePoints(int experiencePointsToAdd)
		{
			ExperiencePoints += experiencePointsToAdd;
			MaximumHitPoints = (Level * 10);
		}

		/// <summary>
		/// 移除玩家物品栏的道具
		/// </summary>
		/// <param name="itemToRemove">需要移除的道具</param>
		/// <param name="quantity">需要移除的数量</param>
		public void RemoveItemFromInventory(Item itemToRemove, int quantity = 1)
		{
			InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToRemove.ID);

			if (item == null)
			{
				// 该道具没有出现在玩家的物品栏里
				// 可能需要根据此情况给出报错信息
			}
			else
			{
				// 该道具有在玩家物品栏里，所以需要减少数量
				item.Quantity -= quantity;

				// 确保我们得到的值不会变成负数
				if (item.Quantity < 0)
				{
					item.Quantity = 0;
				}

				// 当物品数量 = 0，则在物品栏里删除该物品
				if (item.Quantity == 0)
				{
					Inventory.Remove(item);
				}

				// 通知UI，物品栏被更新了
				RaiseInventoryChangedEvent(itemToRemove);
			}
		}

		/// <summary>
		/// 增加道具到玩家物品栏
		/// </summary>
		/// <param name="itemToAdd">需要增加的道具</param>
		/// <param name="quantity">增加道具的数量</param>
		public void AddItemToInventory(Item itemToAdd, int quantity = 1)
		{
			InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);

			if (item == null)
			{
				// 物品栏里没有这个道具，所以增加新的
				Inventory.Add(new InventoryItem(itemToAdd, quantity));
			}
			else
			{
				// 物品栏里有这个道具，所以添加数量
				item.Quantity += quantity;
			}
			RaiseInventoryChangedEvent(itemToAdd);
		}

		/// <summary>
		/// 读取XML存档，然后创建角色
		/// </summary>
		/// <param name="xmlPlayerData"></param>
		/// <returns>玩家保存的角色，如果存档损坏，则创建新角色</returns>
		public static Player CreatePlayerFromXmlString(string xmlPlayerData)
		{
			try
			{
				XmlDocument playerData = new XmlDocument();

				playerData.LoadXml(xmlPlayerData);

				int currentHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
				int maximumHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
				int gold = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
				int experiencePoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);

				Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints);

				int currentLocationID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);
				player.CurrentLocation = World.LocationByID(currentLocationID);

				// 因为是更新，所以确保旧的存档不会出错，如果获取不到当前武器则不执行
				if (playerData.SelectSingleNode("/Player/Stats/CurrentWeapon") != null)
				{
					int currentWeaponID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentWeapon").InnerText);
					player.CurrentWeapon = (Weapon)World.ItemByID(currentWeaponID);
				}

				foreach (XmlNode node in playerData.SelectNodes("/Player/InventoryItems/InventoryItem"))
				{
					int id = Convert.ToInt32(node.Attributes["ID"].Value);
					int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);

					for (int i = 0; i < quantity; i++)
					{
						player.AddItemToInventory(World.ItemByID(id));
					}
				}

				foreach (XmlNode node in playerData.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
				{
					int id = Convert.ToInt32(node.Attributes["ID"].Value);
					bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);

					PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(id));
					playerQuest.IsCompleted = isCompleted;

					player.Quests.Add(playerQuest);
				}

				return player;
			}
			catch
			{
				// 如果xml文件有问题，则创建新的角色
				return Player.CreateDefaultPlayer();
				throw;
			}
		}

		/// <summary>
		/// 读取database传回来的值，然后创建角色
		/// </summary>
		/// <param name="currentHitPoints">当前生命值</param>
		/// <param name="maximumHitPoints">最大生命值</param>
		/// <param name="gold">金币</param>
		/// <param name="experiencePoints">经验值</param>
		/// <param name="currentLocationID">当前位置ID</param>
		/// <returns></returns>
		public static Player CreatePlayerFromDatabase(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints, int currentLocationID)
		{
			Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints);
			player.MoveTo(World.LocationByID(currentLocationID));
			return player;
		}

		/// <summary>
		/// 检查玩家是否拥有进入这个场景所需要的关键道具
		/// </summary>
		/// <param name="location">所需要进入的场景</param>
		/// <returns>如果拥有钥匙道具或者不需要钥匙道具，则返回true，否则返回false</returns>
		public bool HasRequiredItemToEnterThisLocation(Location location)
		{
			if (location.ItemRequiredToEnter == null)
			{
				// 如果不需要关键道具进入这个场景，则直接返回true
				return true;
			}
			// 检查玩家物品栏里是否有所需要的关键道具，使用LINQ写法遍历玩家物品栏
			// Exists方法用于检查Inventory列表里有没有该道具，则返回true，否则返回false
			// 改成BindingList之后，将Exists方法改为Any方法（作用一样）
			return Inventory.Any(inventoryItem => inventoryItem.Details.ID == location.ItemRequiredToEnter.ID);
		}

		/// <summary>
		/// 检查玩家是否拥有这个任务
		/// </summary>
		/// <param name="quest">需要检查的任务</param>
		/// <returns>如果有任务则返回true</returns>
		public bool HasThisQuest(Quest quest)
		{
			return Quests.Any(playerQuest => playerQuest.Details.ID == quest.ID);
		}

		/// <summary>
		/// 检查玩家是否已经完成该任务
		/// </summary>
		/// <param name="quest">需要检查的任务</param>
		/// <returns>返回任务完成情况，如果没有接过这个任务，则返回false</returns>
		public bool CompletedThisQuest(Quest quest)
		{
			foreach (PlayerQuest playerQuest in Quests)
			{
				if (playerQuest.Details.ID == quest.ID)
				{
					return playerQuest.IsCompleted;
				}
			}
			return false;
		}

		/// <summary>
		/// 检查玩家是否拥有完成任务的所有条件道具
		/// </summary>
		/// <param name="quest">需要检查的任务</param>
		/// <returns>玩家拥有所有条件道具，并且所有道具数量足够则返回true</returns>
		public bool HasAllQuestCompletionItems(Quest quest)
		{
			foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
			{
				// 检查玩家物品栏里是否有对应的道具，如果有，检查他们的数量是否达到要求
				if (!Inventory.Any(ii => ii.Details.ID == qci.Details.ID && ii.Quantity >= qci.Quantity))
				{
					// 如果没有达到其中一项要求，则直接返回false，不用继续检查后面的条件。
					return false;
				}
			}
			// 如果玩家拥有所有条件道具，并且所有道具数量足够，可以完成任务
			return true;
		}

		/// <summary>
		/// 删除玩家任务栏里的任务道具
		/// </summary>
		/// <param name="quest">执行的任务</param>
		public void RemoveQuestCompletetionItems(Quest quest)
		{
			foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
			{
				// SingleOrDefault 函数用来检查列表里唯一一个匹配的物品
				InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == qci.Details.ID);
				if (item != null)
				{
					// 减去任务道具
					RemoveItemFromInventory(item.Details, qci.Quantity);
				}
			}
		}

		/// <summary>
		/// 添加物品到物品栏
		/// </summary>
		/// <param name="itemToAdd">需要添加的物品</param>
		public void AddItemToInventory(Item itemToAdd)
		{
			InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);
			if (item == null)
			{
				// 如果物品栏里没有这个道具，则添加新的道具，数量为1
				Inventory.Add(new InventoryItem(itemToAdd, 1));
			}
			else
			{
				// 这个物品已在物品栏，所以直接增加物品数量就行
				item.Quantity++;
				return; // 添加完了之后，结束函数
			}
		}

		/// <summary>
		/// 将任务标记完成
		/// </summary>
		/// <param name="quest"></param>
		public void MarkQuestCompleted(Quest quest)
		{
			// 在玩家任务列表里找到该任务
			PlayerQuest playerQuest = Quests.SingleOrDefault(pq => pq.Details.ID == quest.ID);
			if (playerQuest != null)
			{
				// 把它标记任务完成
				playerQuest.IsCompleted = true;
			}
		}

		/// <summary>
		/// 获取玩家信息，并将其转换为XML字符串
		/// </summary>
		/// <returns>转换之后的XML字符串</returns>
		public string ToXMLString()
		{
			XmlDocument playerData = new XmlDocument();

			// 创建顶层 XML 节点
			XmlNode player = playerData.CreateElement("Player");
			playerData.AppendChild(player);

			// 创建"Stats"节点用于保存用户的Statistics
			XmlNode stats = playerData.CreateElement("Stats");
			player.AppendChild(stats);

			// 在"Stats"节点中创建子节点
			XmlNode currentHitPoints = playerData.CreateElement("CurrentHitPoints");
			currentHitPoints.AppendChild(playerData.CreateTextNode(this.CurrentHitPoints.ToString()));
			stats.AppendChild(currentHitPoints);

			XmlNode maximumHitPoints = playerData.CreateElement("MaximumHitPoints");
			maximumHitPoints.AppendChild(playerData.CreateTextNode(this.MaximumHitPoints.ToString()));
			stats.AppendChild(maximumHitPoints);

			XmlNode gold = playerData.CreateElement("Gold");
			gold.AppendChild(playerData.CreateTextNode(this.Gold.ToString()));
			stats.AppendChild(gold);

			XmlNode experiencePoints = playerData.CreateElement("ExperiencePoints");
			experiencePoints.AppendChild(playerData.CreateTextNode(this.ExperiencePoints.ToString()));
			stats.AppendChild(experiencePoints);

			XmlNode currentLocation = playerData.CreateElement("CurrentLocation");
			currentLocation.AppendChild(playerData.CreateTextNode(this.CurrentLocation.ID.ToString()));
			stats.AppendChild(currentLocation);

			if (CurrentWeapon != null)
			{
				XmlNode currentWeapon = playerData.CreateElement("CurrentWeapon");
				currentWeapon.AppendChild(playerData.CreateTextNode(this.CurrentWeapon.ID.ToString()));
				stats.AppendChild(currentWeapon);
			}

			// 保存物品栏
			XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
			player.AppendChild(inventoryItems);

			// 遍历玩家所有的物品栏
			foreach (InventoryItem item in this.Inventory)
			{
				XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

				XmlAttribute idAttribute = playerData.CreateAttribute("ID");
				idAttribute.Value = item.Details.ID.ToString();
				inventoryItem.Attributes.Append(idAttribute);

				XmlAttribute quantityAttribute = playerData.CreateAttribute("Quantity");
				quantityAttribute.Value = item.Quantity.ToString();
				inventoryItem.Attributes.Append(quantityAttribute);

				inventoryItems.AppendChild(inventoryItem);
			}

			// 保存任务列表
			XmlNode playerQuests = playerData.CreateElement("PlayerQuests");
			player.AppendChild(playerQuests);

			// 遍历玩家的任务栏
			foreach (PlayerQuest quest in this.Quests)
			{
				XmlNode playerQuest = playerData.CreateElement("PlayerQuest");

				XmlAttribute idAttribute = playerData.CreateAttribute("ID");
				idAttribute.Value = quest.Details.ID.ToString();
				playerQuest.Attributes.Append(idAttribute);

				XmlAttribute isCompletedAttribute = playerData.CreateAttribute("IsCompleted");
				isCompletedAttribute.Value = quest.IsCompleted.ToString();
				playerQuest.Attributes.Append(isCompletedAttribute);

				playerQuests.AppendChild(playerQuest);
			}

			//playerData.Save("pretty-save.xml");
			return playerData.InnerXml; // XML文档，字符串，可以保存到文件
		}

		/// <summary>
		/// 当Inventory改变的时候，调用此方法，通知UI界面。
		/// </summary>
		/// <param name="item">检测物品</param>
		private void RaiseInventoryChangedEvent(Item item)
		{
			if (item is Weapon)
			{
				OnPropertyChanged("Weapons");
			}
			if (item is HealingPotion)
			{
				OnPropertyChanged("Potions");
			}
		}

		/// <summary>
		/// 移动到新的地图
		/// </summary>
		/// <param name="newLocation">新的目的地</param>
		public void MoveTo(Location newLocation)
		{
			// 如果玩家没有进入该场景的道具
			if (!HasRequiredItemToEnterThisLocation(newLocation))
			{
				// 显示信息
				RaiseMessage("You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location.");
				return; // 结束事件操作（不让玩家进入）
			}

			// 更新玩家当前位置
			CurrentLocation = newLocation;

			// 完全恢复角色
			CurrentHitPoints = MaximumHitPoints;

			// 当前地点是否有任务
			if (newLocation.QuestAvailableHere != null)
			{
				// 检查玩家是否拥有任务
				bool playerAlreadyHasQuest = HasThisQuest(newLocation.QuestAvailableHere);
				// 检查玩家是否已经完成任务
				bool playerAlreadyCompletedQuest = CompletedThisQuest(newLocation.QuestAvailableHere);

				// 遍历结束后，如果玩家已经拥有这个任务
				if (playerAlreadyHasQuest)
				{
					// 如果玩家还没完成这项任务
					if (!playerAlreadyCompletedQuest)
					{
						// 如果玩家拥有所有的任务道具
						bool playerHasAllItemsToCompleteQuest = HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

						// 当玩家拥有通关任务所需要的道具
						if (playerHasAllItemsToCompleteQuest)
						{
							// 显示信息
							RaiseMessage("");
							RaiseMessage("You complete the " + newLocation.QuestAvailableHere.Name + " quest.");

							// 删除玩家任务栏里的任务道具
							RemoveQuestCompletetionItems(newLocation.QuestAvailableHere);

							// 给予任务奖励道具，并显示信息
							RaiseMessage("You receive: ");
							RaiseMessage(newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + " experience points");
							RaiseMessage(newLocation.QuestAvailableHere.RewardGold.ToString() + " gold");
							RaiseMessage(newLocation.QuestAvailableHere.RewardItem.Name);
							RaiseMessage("");

							AddExperiencePoints(newLocation.QuestAvailableHere.RewardExperiencePoints);
							Gold += newLocation.QuestAvailableHere.RewardGold;

							// 添加奖励道具到玩家任务栏
							AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

							// 标记该任务已经被完成
							MarkQuestCompleted(newLocation.QuestAvailableHere);
						}
					}
				}
				else // 如果玩家没有这个任务
				{
					// 显示信息
					RaiseMessage("You received the " + newLocation.QuestAvailableHere.Name + " quest.");
					RaiseMessage(newLocation.QuestAvailableHere.Description);
					RaiseMessage("To complete it, return with: ");
					foreach (QuestCompletionItem questCompletionItem in newLocation.QuestAvailableHere.QuestCompletionItems)
					{
						if (questCompletionItem.Quantity == 1)
						{
							RaiseMessage(questCompletionItem.Quantity + " " + questCompletionItem.Details.Name);
						}
						else
						{
							RaiseMessage(questCompletionItem.Quantity.ToString() + " " + questCompletionItem.Details.NamePlural);
						}
					}
					RaiseMessage("");

					// 将任务添加到玩家人物列表里
					Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
				}
			}

			// 检查该地点是否有怪物存在
			if (newLocation.MonsterLivingHere != null)
			{
				// 有怪物
				RaiseMessage("You see a " + newLocation.MonsterLivingHere.Name);
				// 用World类的值创建新的怪物对象
				Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

				_currentMonster = new Monster(
					standardMonster.ID,
					standardMonster.Name,
					standardMonster.MaximumDamage,
					standardMonster.RewardExperiencePoints,
					standardMonster.RewardGold,
					standardMonster.CurrentHitPoints,
					standardMonster.MaximumHitPoints);

				foreach (LootItem lootItem in standardMonster.LootTable)
				{
					_currentMonster.LootTable.Add(lootItem);
				}
			}
			else
			{
				_currentMonster = null;
			}
		}

		public void MoveNorth()
		{
			if (CurrentLocation.LocationToNorth != null)
			{
				MoveTo(CurrentLocation.LocationToNorth);
			}
		}

		public void MoveEast()
		{
			if (CurrentLocation.LocationToEast != null)
			{
				MoveTo(CurrentLocation.LocationToEast);
			}
		}

		public void MoveSouth()
		{
			if (CurrentLocation.LocationToSouth != null)
			{
				MoveTo(CurrentLocation.LocationToSouth);
			}
		}

		public void MoveWest()
		{
			if (CurrentLocation.LocationToWest != null)
			{
				MoveTo(CurrentLocation.LocationToWest);
			}
		}

		private void MoveHome()
		{
			MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
		}

		public void UseWeapon(Weapon weapon)
		{
			// 确定对怪物造成的伤害量
			int damageToMonster = RandomNumberGenerator.NumberBetween(
				weapon.MinimumDamage,
				weapon.MaximumDamage);

			// 对怪物造成伤害
			_currentMonster.CurrentHitPoints -= damageToMonster;

			// 显示信息
			RaiseMessage("You hit the " + _currentMonster.Name + " for " + damageToMonster + " points.");

			// 检查怪物是否死亡
			if (_currentMonster.CurrentHitPoints <= 0)
			{
				// 怪物死了
				RaiseMessage("");
				RaiseMessage("You defeated the " + _currentMonster.Name);

				// 奖励经验值
				AddExperiencePoints(_currentMonster.RewardExperiencePoints);
				RaiseMessage("You receive " + _currentMonster.RewardExperiencePoints.ToString() + " experience points.");

				// 奖励金币
				Gold += _currentMonster.RewardGold;
				RaiseMessage("You receive " + _currentMonster.RewardGold.ToString() + " gold.");

				// 获取随机掉落物
				List<InventoryItem> lootedItems = new List<InventoryItem>();

				// 添加道具到lootedItems列表里，根据掉落率比较一个随机值
				foreach (LootItem lootItem in _currentMonster.LootTable)
				{
					if (RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
					{
						lootedItems.Add(new InventoryItem(lootItem.Details, 1));
					}
				}
				// 如果没有道具被随机选中，那么增加默认道具
				if (lootedItems.Count == 0)
				{
					foreach (LootItem lootItem in _currentMonster.LootTable)
					{
						if (lootItem.IsDefaultItem)
						{
							lootedItems.Add(new InventoryItem(lootItem.Details, 1));
						}
					}
				}
				// 将掉落物添加到玩家道具栏里。
				foreach (InventoryItem inventoryItem in lootedItems)
				{
					AddItemToInventory(inventoryItem.Details);

					if (inventoryItem.Quantity == 1)
					{
						RaiseMessage("You loot " + inventoryItem.Quantity.ToString() + " " + inventoryItem.Details.Name);
					}
					else
					{
						RaiseMessage("You loot " + inventoryItem.Quantity.ToString() + " " + inventoryItem.Details.NamePlural);
					}
				}

				// 在信息框中添加一行空行，美观
				RaiseMessage("");

				// 移动玩家到当前位置（用于刷新玩家、更新新的怪物）
				MoveTo(CurrentLocation);
			}
			else
			{
				// 怪物仍然活着，轮到怪物回合
				// 计算怪物对玩家造成的伤害
				int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

				// 显示信息
				RaiseMessage("The " + _currentMonster.Name + " did " + damageToPlayer + " points of damage.");

				// 扣去玩家生命值
				CurrentHitPoints -= damageToPlayer;

				// 检查玩家是否死亡
				if (CurrentHitPoints <= 0)
				{
					// 显示信息
					RaiseMessage("The " + _currentMonster.Name + " killed you.");

					// 回到出生点
					MoveHome();
				}
			}
		}

		public void UsePotion(HealingPotion potion)
		{
			// 恢复玩家生命值
			CurrentHitPoints += potion.AmountToHeal;

			// 当前生命值不能超过最大生命值
			if (CurrentHitPoints > MaximumHitPoints)
			{
				CurrentHitPoints = MaximumHitPoints;
			}

			// 从物品栏中删除药品
			RemoveItemFromInventory(potion, 1);

			// 显示信息
			RaiseMessage("You drink a " + potion.Name + " and healed " + potion.AmountToHeal + " hit points.");

			// 轮到怪物展开攻击
			int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

			// 显示信息
			RaiseMessage("The " + _currentMonster.Name + " did " + damageToPlayer + " points of damage.");

			// 扣去玩家生命值
			CurrentHitPoints -= damageToPlayer;

			// 检查玩家是否死亡
			if (CurrentHitPoints <= 0)
			{
				// 显示信息
				RaiseMessage("The " + _currentMonster.Name + " killed you.");

				// 回到出生点
				MoveHome();
			}
		}

		/// <summary>
		/// UI事件监听handler
		/// <para>
		/// The EventHandler<MessageEventArgs> signifies that the Player class will send an event
		/// notification with a MessageEventArgs object – the object with the message text we want to
		/// display.
		/// </para>
		/// </summary>
		public event EventHandler<MessageEventArgs> OnMessage;

		// 提升事件
		private void RaiseMessage(string message, bool addExtraNewLine = false)
		{
			if (OnMessage != null)
			{
				OnMessage(this, new MessageEventArgs(message, addExtraNewLine));
			}
		}
	}
}
