using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;

namespace Engine
{
	public class Player : LivingCreature
	{
		private int _gold;
		private int _experiencePoints;
		private Location _currentLocation;

		/// <summary>
		/// UI事件监听handler
		/// <para>The EventHandler<MessageEventArgs> signifies that the Player class will send an event
		/// notification with a MessageEventArgs object – the object with the message text we want to
		/// display.</para>
		/// </summary>
		public event EventHandler<MessageEventArgs> OnMessage;

		public int Gold
		{
			get { return _gold; }
			set
			{
				_gold = value;
				OnPropertyChanged("Gold");
			}
		}

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

		public int Level
		{
			// 每一次获取Level的值时，都会计算；因为是整数型，会向下取整，所以需要+1
			get { return (ExperiencePoints / 100) + 1; }
		}

		public Location CurrentLocation
		{
			get { return _currentLocation; }
			set
			{
				_currentLocation = value;
				OnPropertyChanged("CurrentLocation");
			}
		}

		public Weapon CurrentWeapon { get; set; }

		// To bind a list property, you need to change its datatype to either BindingList or
		// ObservableCollection.BindingList gives more options than ObservableCollection – like searching and sorting.
		public BindingList<InventoryItem> Inventory { get; set; }
		public BindingList<PlayerQuest> Quests { get; set; }

		public List<Weapon> Weapons
		{
			// 如果列表元素是武器类，则把该对象转成新的list（只需要InventoryItems的Details属性，不需要Quantity属性）
			get { return Inventory.Where(x => x.Details is Weapon).Select(x => x.Details as Weapon).ToList(); }
		}
		public List<HealingPotion> Potions
		{
			get { return Inventory.Where(x => x.Details is HealingPotion).Select(x => x.Details as HealingPotion).ToList(); }
		}

		private Monster CurrentMonster { get; set; }

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
			player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
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
			InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToRemove.ID && ii.Quantity > quantity);

			if (item != null)
			{
				// 该道具有在玩家物品栏里，所以需要减少数量
				item.Quantity -= quantity;

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
			InventoryItem existingItemInInventory = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);

			if (existingItemInInventory == null)
			{
				// 物品栏里没有这个道具，所以增加新的
				Inventory.Add(new InventoryItem(itemToAdd, quantity));
			}
			else
			{
				// 物品栏里有这个道具，所以添加数量
				existingItemInInventory.Quantity += quantity;
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
				return CreateDefaultPlayer();
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
		public void RemoveQuestCompletionItems(Quest quest)
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
			CreateNewChildXmlNode(playerData, stats, "CurrentHitPoints", CurrentHitPoints);
            CreateNewChildXmlNode(playerData, stats, "MaximumHitPoints", MaximumHitPoints);
            CreateNewChildXmlNode(playerData, stats, "Gold", Gold);
            CreateNewChildXmlNode(playerData, stats, "ExperiencePoints", ExperiencePoints);
            CreateNewChildXmlNode(playerData, stats, "CurrentLocation", CurrentLocation.ID);

			if (CurrentWeapon != null)
			{
				CreateNewChildXmlNode(playerData, stats, "CurrentWeapon", CurrentWeapon.ID);
			}

			// 保存物品栏
			XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
			player.AppendChild(inventoryItems);

			// 遍历玩家所有的物品栏
			foreach (InventoryItem item in this.Inventory)
			{
				XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

				AddXmlAttributeToNode(playerData, inventoryItem, "ID", item.Details.ID);
				AddXmlAttributeToNode(playerData, inventoryItem, "Quantity", item.Quantity);

				inventoryItems.AppendChild(inventoryItem);
			}

			// 保存任务列表
			XmlNode playerQuests = playerData.CreateElement("PlayerQuests");
			player.AppendChild(playerQuests);

			// 遍历玩家的任务栏
			foreach (PlayerQuest quest in this.Quests)
			{
				XmlNode playerQuest = playerData.CreateElement("PlayerQuest");

				AddXmlAttributeToNode(playerData, playerQuest, "ID", quest.Details.ID);
				AddXmlAttributeToNode(playerData, playerQuest, "IsCompleted", quest.IsCompleted);

				playerQuests.AppendChild(playerQuest);
			}

			//playerData.Save("pretty-save.xml");
			return playerData.InnerXml; // XML文档，字符串，可以保存到文件
		}

		private void CreateNewChildXmlNode(XmlDocument document, XmlNode parentNode, string elementName, object value)
		{
			XmlNode node = document.CreateElement(elementName);
			node.AppendChild(document.CreateTextNode(value.ToString()));
			parentNode.AppendChild(node);
		}

		private void AddXmlAttributeToNode(XmlDocument document, XmlNode node, string attributeName, object value)
		{
			XmlAttribute attribute = document.CreateAttribute(attributeName);
			attribute.Value = value.ToString();
			node.Attributes.Append(attribute);
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
		/// <param name="location">新的目的地</param>
		public void MoveTo(Location location)
		{
			if (PlayerDoesNotHaveTheRequiredItemToEnter(location))
			{
				RaiseMessage("You must have a " + location.ItemRequiredToEnter.Name + " to enter this location.");
				return;
			}

			// The player can enter this location
			CurrentLocation = location;

			CompletelyHeal();

			// 当前地点有任务
			if (location.HasAQuest)
			{
				// 检查玩家没有这个任务
				if (PlayerDoesNotHaveThisQuest(location.QuestAvailableHere))
				{
					GiveQuestToPlayer(location.QuestAvailableHere);
				}
				else
				{
					if (PlayerHasNotCompleted(location.QuestAvailableHere) && PlayerHasAllQuestCompletionItemsFor(location.QuestAvailableHere))
					{
						GivePlayerQuestRewards(location.QuestAvailableHere);
					}
				}
			}

			SetTheCurrentMonsterForTheCurrentLocation(location);
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
			int damageToMonster = RandomNumberGenerator.NumberBetween(weapon.MinimumDamage, weapon.MaximumDamage);

			if (damageToMonster == 0)
			{
				RaiseMessage("You missed the " + CurrentMonster.Name);
			}
			else
			{
				CurrentMonster.CurrentHitPoints -= damageToMonster;
				RaiseMessage("You hit the " + CurrentMonster.Name + " for " + damageToMonster + " points.");
			}


			// 检查怪物是否死亡
			if (CurrentMonster.IsDead)
			{
				LootTheCurrentMonster();
				MoveTo(CurrentLocation); // 移动玩家到当前位置（用于刷新玩家、更新新的怪物）
			}
			else
			{
				LetTheMonsterAttack();
			}
		}

		private void LootTheCurrentMonster()
		{
			RaiseMessage("");
			RaiseMessage("You defeated the " + CurrentMonster.Name);
			RaiseMessage("You receive " + CurrentMonster.RewardExperiencePoints.ToString() + " experience points.");
			RaiseMessage("You receive " + CurrentMonster.RewardGold.ToString() + " gold.");

			AddExperiencePoints(CurrentMonster.RewardExperiencePoints);
			Gold += CurrentMonster.RewardGold;

			// 将掉落物给予玩家
			foreach (InventoryItem inventoryItem in CurrentMonster.LootItems)
			{
				AddItemToInventory(inventoryItem.Details);

				RaiseMessage(string.Format("You loot {0} {1}", inventoryItem.Quantity, inventoryItem.Description));
			}

			// 在信息框中添加一行空行，美观
			RaiseMessage("");
		}

		public void UsePotion(HealingPotion potion)
		{
			// 显示信息
			RaiseMessage("You drink a " + potion.Name + " and healed " + potion.AmountToHeal + " hit points.");
			HealPlayer(potion.AmountToHeal);
			RemoveItemFromInventory(potion);
			// The player used their turn to drink the potion, so let the monster attack now
			LetTheMonsterAttack();
		}

		private void SetTheCurrentMonsterForTheCurrentLocation(Location location)
		{
			// Populate the current monster with this location's monster (or null, if there is no monster here)
			CurrentMonster = location.NewInstanceOfMonsterLivingHere();

			if (CurrentMonster != null)
			{
				RaiseMessage("You see a " + location.MonsterLivingHere.Name);
			}
		}

		private bool PlayerDoesNotHaveTheRequiredItemToEnter(Location location)
		{
			return !HasRequiredItemToEnterThisLocation(location);
		}

		private bool PlayerDoesNotHaveThisQuest(Quest quest)
		{
			return Quests.All(pq => pq.Details.ID != quest.ID);
		}

		private bool PlayerHasNotCompleted(Quest quest)
		{
			return Quests.Any(pq => pq.Details.ID == quest.ID && !pq.IsCompleted);
		}

		private void GiveQuestToPlayer(Quest quest)
		{
			RaiseMessage("You receive the " + quest.Name + " quest.");
			RaiseMessage(quest.Description);
			RaiseMessage("To complete it, return with:");

			foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
			{
				RaiseMessage(string.Format("{0} {1}", qci.Quantity,
					qci.Quantity == 1 ? qci.Details.Name : qci.Details.NamePlural));
			}

			RaiseMessage("");

			Quests.Add(new PlayerQuest(quest));
		}

		private bool PlayerHasAllQuestCompletionItemsFor(Quest quest)
		{
			// See if the player has all the items needed to complete the quest here
			foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
			{
				// Check each item in the player's inventory, to see if they have it, and enough of it
				if (!Inventory.Any(ii => ii.Details.ID == qci.Details.ID && ii.Quantity >= qci.Quantity))
				{
					return false;
				}
			}

			// If we got here, then the player must have all the required items, and enough of them, to complete the quest.
			return true;
		}

		private void GivePlayerQuestRewards(Quest quest)
		{
			RaiseMessage("");
			RaiseMessage("You complete the '" + quest.Name + "' quest.");
			RaiseMessage("You receive: ");
			RaiseMessage(quest.RewardExperiencePoints + " experience points");
			RaiseMessage(quest.RewardGold + " gold");
			RaiseMessage(quest.RewardItem.Name, true);

			AddExperiencePoints(quest.RewardExperiencePoints);
			Gold += quest.RewardGold;

			RemoveQuestCompletionItems(quest);
			AddItemToInventory(quest.RewardItem);

			MarkPlayerQuestCompleted(quest);
		}

		private void MarkPlayerQuestCompleted(Quest quest)
		{
			PlayerQuest playerQuest = Quests.SingleOrDefault(pq => pq.Details.ID == quest.ID);

			if (playerQuest != null)
			{
				playerQuest.IsCompleted = true;
			}
		}

		private void LetTheMonsterAttack()
		{
			int damageToPlayer = RandomNumberGenerator.NumberBetween(0, CurrentMonster.MaximumDamage);

			RaiseMessage("The " + CurrentMonster.Name + " did " + damageToPlayer + " points of damage.");

			CurrentHitPoints -= damageToPlayer;

			if (IsDead)
			{
				RaiseMessage("The " + CurrentMonster.Name + " killed you.");

				MoveHome();
			}
		}

		private void HealPlayer(int hitPointsToHeal)
		{
			CurrentHitPoints = Math.Min(CurrentHitPoints + hitPointsToHeal, MaximumHitPoints);
		}

		private void CompletelyHeal()
		{
			CurrentHitPoints = MaximumHitPoints;
		}

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
