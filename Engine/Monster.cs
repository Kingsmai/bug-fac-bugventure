using System.Collections.Generic;
using System.Linq;

namespace Engine
{
	public class Monster : LivingCreature
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public int MaximumDamage { get; set; }
		public int RewardExperiencePoints { get; set; }
		public int RewardGold { get; set; }

		// 此怪兽拥有的掉落物（附掉率）
		public List<LootItem> LootTable { get; set; }
		// 这个怪物实例拥有的物品数量 
		internal List<InventoryItem> LootItems { get; }

		/// <summary>
		/// 怪物
		/// </summary>
		/// <param name="id">唯一ID</param>
		/// <param name="name">怪物名</param>
		/// <param name="maximumDamage">攻击力</param>
		/// <param name="rewardExperiencePoints">击杀获得奖励</param>
		/// <param name="rewardGold">击杀获得金币</param>
		/// <param name="currentHitPoints">当前生命值</param>
		/// <param name="maximumHitPoints">最大生命值</param>
		public Monster(int id, string name, int maximumDamage, int rewardExperiencePoints, int rewardGold, int currentHitPoints, int maximumHitPoints)
			: base(currentHitPoints, maximumHitPoints)
		{
			ID = id;
			Name = name;
			MaximumDamage = maximumDamage;
			RewardExperiencePoints = rewardExperiencePoints;
			RewardGold = rewardGold;

			LootTable = new List<LootItem>();
			LootItems = new List<InventoryItem>();
		}

		internal Monster NewInstanceOfMonster()
		{
			Monster newMonster =
				new Monster(ID, Name, MaximumDamage, RewardExperiencePoints, RewardGold, CurrentHitPoints, MaximumHitPoints);

			// Add items to the lootedItems list, comparing a random number to the drop percentage
			foreach (LootItem lootItem in LootTable.Where(lootItem => RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage))
			{
				newMonster.LootItems.Add(new InventoryItem(lootItem.Details, 1));
			}

			// If no items were randomly selected, add the default loot item(s).
			if (newMonster.LootItems.Count == 0)
			{
				foreach (LootItem lootItem in LootTable.Where(x => x.IsDefaultItem))
				{
					newMonster.LootItems.Add(new InventoryItem(lootItem.Details, 1));
				}
			}

			return newMonster;
		}
	}
}
