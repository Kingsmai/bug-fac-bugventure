using System.Collections.Generic;

namespace Engine
{
	public class Monster : LivingCreature
	{
		// ID
		public int ID { get; set; }
		// 怪物名
		public string Name { get; set; }
		// 怪物最大攻击力
		public int MaximumDamage { get; set; }
		// 击杀得到经验值
		public int RewardExperiencePoints { get; set; }
		// 击杀得到金币
		public int RewardGold { get; set; }
		/// <summary>
		/// 战利品表
		/// </summary>
		public List<LootItem> LootTable { get; set; }

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
		public Monster(int id, string name, int maximumDamage, int rewardExperiencePoints, int rewardGold,
			int currentHitPoints, int maximumHitPoints)
			: base(currentHitPoints, maximumHitPoints)
		{
			ID = id;
			Name = name;
			MaximumDamage = maximumDamage;
			RewardExperiencePoints = rewardExperiencePoints;
			RewardGold = rewardGold;

			LootTable = new List<LootItem>();
		}
	}
}
