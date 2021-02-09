using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Engine
{
	public class Player : LivingCreature
	{
		// 金币
		public int Gold { get; set; }
		// 经验值
		public int ExperiencePoints { get; private set; }
		// 等级
		public int Level
		{
			// 每一次获取Level的值时，都会计算
			// 因为时整数型，会向下取整，所以需要+1
			get { return ((ExperiencePoints / 100) + 1); }
			// 我们不需要手动修改等级，所以把set删掉了。
		}
		// 物品栏
		public List<InventoryItem> Inventory { get; set; }
		// 当前任务
		public List<PlayerQuest> Quests { get; set; }
		// 当前位置
		public Location CurrentLocation { get; set; }
		// 当前使用武器
		public Weapon CurrentWeapon { get; set; }

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

			Inventory = new List<InventoryItem>();
			Quests = new List<PlayerQuest>();
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
				if(playerData.SelectSingleNode("/Player/Stats/CurrentWeapon") != null)
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
			return Inventory.Exists(inventoryItem => inventoryItem.Details.ID == location.ItemRequiredToEnter.ID);
		}

		/// <summary>
		/// 检查玩家是否拥有这个任务
		/// </summary>
		/// <param name="quest">需要检查的任务</param>
		/// <returns>如果有任务则返回true</returns>
		public bool HasThisQuest(Quest quest)
		{
			return Quests.Exists(playerQuest => playerQuest.Details.ID == quest.ID);
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
				if (!Inventory.Exists(ii => ii.Details.ID == qci.Details.ID && ii.Quantity >= qci.Quantity))
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
					item.Quantity -= qci.Quantity;
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
	}
}
