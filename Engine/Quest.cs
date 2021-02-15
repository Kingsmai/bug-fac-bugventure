using System.Collections.Generic;

namespace Engine
{
	public class Quest
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public List<QuestCompletionItem> QuestCompletionItems { get; set; }
		public int RewardExperiencePoints { get; set; }
		public int RewardGold { get; set; }
		public Item RewardItem { get; set; }

		/// <summary>
		/// 任务
		/// </summary>
		/// <param name="id">唯一ID</param>
		/// <param name="name">任务名</param>
		/// <param name="description">任务描述</param>
		/// <param name="rewardExperiencePoints">奖励经验值</param>
		/// <param name="rewardGold">奖励金币</param>
		public Quest(int id, string name, string description, int rewardExperiencePoints, int rewardGold)
		{
			ID = id;
			Name = name;
			Description = description;
			RewardExperiencePoints = rewardExperiencePoints;
			RewardGold = rewardGold;
			QuestCompletionItems = new List<QuestCompletionItem>();
		}
	}
}
