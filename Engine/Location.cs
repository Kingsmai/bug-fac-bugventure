namespace Engine
{
	public class Location
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public Item ItemRequiredToEnter { get; set; }
		public Quest QuestAvailableHere { get; set; }
		public Monster MonsterLivingHere { get; set; }
		public Vendor VendorWorkingHere { get; set; }
		public Location LocationToNorth { get; set; }
		public Location LocationToEast { get; set; }
		public Location LocationToSouth { get; set; }
		public Location LocationToWest { get; set; }

		public bool HasAQuest { get { return QuestAvailableHere != null; } }
		public bool DoesNotHaveAnItemRequiredToEnter { get { return ItemRequiredToEnter == null; } }

		// 构造方法
		/// <summary>
		/// 地点（地图位置）
		/// </summary>
		/// <param name="id">唯一ID</param>
		/// <param name="name">位置名</param>
		/// <param name="description">位置描述</param>
		/// <param name="itemRequiredToEnter">进入该场景所需要的物品（比如钥匙）</param>
		/// <param name="questAvailableHere">这个场景会触发的任务</param>
		/// <param name="monsterLivingHere">在这个场景的怪物</param>
		public Location(int id, string name, string description,
			Item itemRequiredToEnter = null,
			Quest questAvailableHere = null,
			Monster monsterLivingHere = null)
		{
			ID = id;
			Name = name;
			Description = description;
			ItemRequiredToEnter = itemRequiredToEnter;
			QuestAvailableHere = questAvailableHere;
			MonsterLivingHere = monsterLivingHere;
		}

		public Monster NewInstanceOfMonsterLivingHere()
		{
			return MonsterLivingHere == null ? null : MonsterLivingHere.NewInstanceOfMonster();
		}
	}
}
