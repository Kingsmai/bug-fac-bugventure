using System.Collections.Generic;

namespace Engine
{
	/// <summary>
	/// 世界类：保存所有存在于BugVenture世界里的游戏物品，
	/// <para>
	/// 保存内容：某个地方出现的怪物、杀死怪物后能得到的物品、各个地点之间的路径关系
	/// </para>
	/// </summary>
	public static class World
	{
		public static readonly List<Item> Items = new List<Item>();
		public static readonly List<Monster> Monsters = new List<Monster>();
		public static readonly List<Quest> Quests = new List<Quest>();
		public static readonly List<Location> Locations = new List<Location>();

		// 物品ID
		public const int ITEM_ID_RUSTY_SWORD = 1;
		public const int ITEM_ID_RAT_TAIL = 2;
		public const int ITEM_ID_PIECE_OF_FUR = 3;
		public const int ITEM_ID_SNAKE_FANG = 4;
		public const int ITEM_ID_SNAKESKIN = 5;
		public const int ITEM_ID_CLUB = 6;
		public const int ITEM_ID_HEALING_POTION = 7;
		public const int ITEM_ID_SPIDER_FANG = 8;
		public const int ITEM_ID_SPIDER_SILK = 9;
		public const int ITEM_ID_ADVENTURER_PASS = 10;

		// 怪物ID
		public const int MONSTER_ID_RAT = 1;
		public const int MONSTER_ID_SNAKE = 2;
		public const int MONSTER_ID_GIANT_SPIDER = 3;

		// 任务ID
		public const int QUEST_ID_CLEAR_ALCHEMIST_GARDEN = 1;
		public const int QUEST_ID_CLEAR_FARMERS_FIELD = 2;

		// 地点ID
		public const int LOCATION_ID_HOME = 1;
		public const int LOCATION_ID_TOWN_SQUARE = 2;
		public const int LOCATION_ID_GUARD_POST = 3;
		public const int LOCATION_ID_ALCHEMIST_HUT = 4;
		public const int LOCATION_ID_ALCHEMISTS_GARDEN = 5;
		public const int LOCATION_ID_FARMHOUSE = 6;
		public const int LOCATION_ID_FARM_FIELD = 7;
		public const int LOCATION_ID_BRIDGE = 8;
		public const int LOCATION_ID_SPIDER_FIELD = 9;

		// 非卖品价格（作为Flag）
		public const int UNSELLABLE_ITEM_PRICE = -1;

		static World()
		{
			PopulateItems();
			PopulateMonsters();
			PopulateQuests();
			PopulateLocations();
		}

		/// <summary>
		/// 创建游戏物品
		/// </summary>
		private static void PopulateItems()
		{
			// 锈剑
			Items.Add(new Weapon(ITEM_ID_RUSTY_SWORD, "Rusty sword", "Rusty swords", 0, 5, 5));
			// 老鼠尾巴
			Items.Add(new Item(ITEM_ID_RAT_TAIL, "Rat tail", "Rat tails", 1));
			// 毛皮
			Items.Add(new Item(ITEM_ID_PIECE_OF_FUR, "Piece of fur", "Pieces of fur", 1));
			// 毒蛇之牙
			Items.Add(new Item(ITEM_ID_SNAKE_FANG, "Snake fang", "Snake fangs", 1));
			// 蛇皮
			Items.Add(new Item(ITEM_ID_SNAKESKIN, "Snakeskin", "Snakeskins", 2));
			// 木棍
			Items.Add(new Weapon(ITEM_ID_CLUB, "Club", "Clubs", 3, 10, 8));
			// 恢复药水
			Items.Add(new HealingPotion(ITEM_ID_HEALING_POTION, "Healing potion", "Healing potions", 5, 3));
			// 蜘蛛之牙
			Items.Add(new Item(ITEM_ID_SPIDER_FANG, "Spider fang", "Spider fangs", 1));
			// 蜘蛛丝
			Items.Add(new Item(ITEM_ID_SPIDER_SILK, "Spider silk", "Spider silks", 1));
			// 冒险家通行证（任务奖励，非卖品）
			Items.Add(new Item(ITEM_ID_ADVENTURER_PASS, "Adventurer pass", "Adventurer passes", UNSELLABLE_ITEM_PRICE));
		}

		/// <summary>
		/// 创建游戏怪物
		/// </summary>
		private static void PopulateMonsters()
		{
			// 老鼠
			Monster rat = new Monster(MONSTER_ID_RAT, "Rat", 5, 3, 10, 3, 3);
			// 掉落物:老鼠尾巴
			rat.LootTable.Add(new LootItem(ItemByID(ITEM_ID_RAT_TAIL), 75, false));
			// 掉落物:毛皮
			rat.LootTable.Add(new LootItem(ItemByID(ITEM_ID_PIECE_OF_FUR), 75, true));

			// 蛇
			Monster snake = new Monster(MONSTER_ID_SNAKE, "Snake", 5, 3, 10, 3, 3);
			// 掉落物:毒蛇之牙
			snake.LootTable.Add(new LootItem(ItemByID(ITEM_ID_SNAKE_FANG), 75, false));
			// 掉落物:蛇皮
			snake.LootTable.Add(new LootItem(ItemByID(ITEM_ID_SNAKESKIN), 75, true));

			// 大蜘蛛
			Monster giantSpider = new Monster(MONSTER_ID_GIANT_SPIDER, "Giant spider", 20, 5, 40, 10, 10);
			// 蜘蛛之牙
			giantSpider.LootTable.Add(new LootItem(ItemByID(ITEM_ID_SPIDER_FANG), 75, true));
			// 蜘蛛丝
			giantSpider.LootTable.Add(new LootItem(ItemByID(ITEM_ID_SPIDER_SILK), 25, false));

			Monsters.Add(rat);
			Monsters.Add(snake);
			Monsters.Add(giantSpider);
		}

		/// <summary>
		/// 创建游戏任务
		/// </summary>
		private static void PopulateQuests()
		{
			Quest clearAlchemistGarden = new Quest(QUEST_ID_CLEAR_ALCHEMIST_GARDEN,
			"Clear the alchemist's garden",
			"Kill rats in the alchemist's garden and bring back 3 rat tails.You will receive a healing potion and 10 gold pieces.", 20, 10);

			clearAlchemistGarden.QuestCompletionItems.Add(new QuestCompletionItem(ItemByID(ITEM_ID_RAT_TAIL), 3));

			clearAlchemistGarden.RewardItem = ItemByID(ITEM_ID_HEALING_POTION);

			Quest clearFarmersField = new Quest(QUEST_ID_CLEAR_FARMERS_FIELD,
			"Clear the farmer's field",
			"Kill snakes in the farmer's field and bring back 3 snake fangs.You will receive an adventurer's pass and 20 gold pieces.", 20, 20);

			clearFarmersField.QuestCompletionItems.Add(new QuestCompletionItem(ItemByID(ITEM_ID_SNAKE_FANG), 3));

			clearFarmersField.RewardItem = ItemByID(ITEM_ID_ADVENTURER_PASS);

			Quests.Add(clearAlchemistGarden);
			Quests.Add(clearFarmersField);
		}

		/// <summary>
		/// 创建游戏场景
		/// </summary>
		private static void PopulateLocations()
		{
			// Create each location
			Location home = new Location(LOCATION_ID_HOME, "Home", "Your house. You really need to clean up the place.");

			Location townSquare = new Location(LOCATION_ID_TOWN_SQUARE, "Town square", "You see a fountain.");
			Vendor bobTheRatCatcher = new Vendor("Bob the Rat-Catcher");
			bobTheRatCatcher.AddItemToInventory(ItemByID(ITEM_ID_PIECE_OF_FUR), 5);
			bobTheRatCatcher.AddItemToInventory(ItemByID(ITEM_ID_RAT_TAIL), 3);
			townSquare.VendorWorkingHere = bobTheRatCatcher;

			Location alchemistHut = new Location(LOCATION_ID_ALCHEMIST_HUT, "Alchemist's hut", "There are many strange plants on the shelves.");
			alchemistHut.QuestAvailableHere = QuestByID(QUEST_ID_CLEAR_ALCHEMIST_GARDEN);

			Location alchemistsGarden = new Location(LOCATION_ID_ALCHEMISTS_GARDEN, "Alchemist's garden", "Many plants are growing here.");
			alchemistsGarden.MonsterLivingHere = MonsterByID(MONSTER_ID_RAT);

			Location farmhouse = new Location(LOCATION_ID_FARMHOUSE, "Farmhouse", "There is a small farmhouse, with a farmer in front.");
			farmhouse.QuestAvailableHere = QuestByID(QUEST_ID_CLEAR_FARMERS_FIELD);

			Location farmersField = new Location(LOCATION_ID_FARM_FIELD, "Farmer's field", "You see rows of vegetables growing here.");
			farmersField.MonsterLivingHere = MonsterByID(MONSTER_ID_SNAKE);

			Location guardPost = new Location(LOCATION_ID_GUARD_POST, "Guard post", "There is a large, tough-looking guard here.", ItemByID(ITEM_ID_ADVENTURER_PASS));

			Location bridge = new Location(LOCATION_ID_BRIDGE, "Bridge", "A stone bridge crosses a wide river.");

			Location spiderField = new Location(LOCATION_ID_SPIDER_FIELD, "Forest", "You see spider webs covering covering the trees in this forest.");
			spiderField.MonsterLivingHere = MonsterByID(MONSTER_ID_GIANT_SPIDER);

			// Link the locations together
			home.LocationToNorth = townSquare;

			townSquare.LocationToNorth = alchemistHut;
			townSquare.LocationToSouth = home;
			townSquare.LocationToEast = guardPost;
			townSquare.LocationToWest = farmhouse;

			farmhouse.LocationToEast = townSquare;
			farmhouse.LocationToWest = farmersField;

			farmersField.LocationToEast = farmhouse;

			alchemistHut.LocationToSouth = townSquare;
			alchemistHut.LocationToNorth = alchemistsGarden;

			alchemistsGarden.LocationToSouth = alchemistHut;

			guardPost.LocationToEast = bridge;
			guardPost.LocationToWest = townSquare;

			bridge.LocationToWest = guardPost;
			bridge.LocationToEast = spiderField;

			spiderField.LocationToWest = bridge;

			// Add the locations to the static list
			Locations.Add(home);
			Locations.Add(townSquare);
			Locations.Add(guardPost);
			Locations.Add(alchemistHut);
			Locations.Add(alchemistsGarden);
			Locations.Add(farmhouse);
			Locations.Add(farmersField);
			Locations.Add(bridge);
			Locations.Add(spiderField);
		}

		/// <summary>
		/// 通过ID值获取物品
		/// </summary>
		/// <param name="id">物品ID值</param>
		/// <returns>Item</returns>
		public static Item ItemByID(int id)
		{
			foreach (Item item in Items)
			{
				if (item.ID == id)
				{
					return item;
				}
			}

			return null;
		}

		/// <summary>
		/// 通过ID值获取怪物
		/// </summary>
		/// <param name="id">怪物ID值</param>
		/// <returns>Monster</returns>
		public static Monster MonsterByID(int id)
		{
			foreach (Monster monster in Monsters)
			{
				if (monster.ID == id)
				{
					return monster;
				}
			}

			return null;
		}

		/// <summary>
		/// 通过ID值获取任务
		/// </summary>
		/// <param name="id">任务ID值</param>
		/// <returns>Quest</returns>
		public static Quest QuestByID(int id)
		{
			foreach (Quest quest in Quests)
			{
				if (quest.ID == id)
				{
					return quest;
				}
			}

			return null;
		}

		/// <summary>
		/// 通过ID值获取地点
		/// </summary>
		/// <param name="id">地点ID值</param>
		/// <returns>Location</returns>
		public static Location LocationByID(int id)
		{
			foreach (Location location in Locations)
			{
				if (location.ID == id)
				{
					return location;
				}
			}

			return null;
		}
	}
}
