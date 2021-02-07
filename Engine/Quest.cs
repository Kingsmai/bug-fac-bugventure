using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
	public class Quest
	{
		// ID
		public int ID { get; set; }
		// 任务名
		public string Name { get; set; }
		// 任务详情
		public string Description { get; set; }
		/// <summary>
		/// 完成任务需要的物品（任务条件）
		/// </summary>
		public List<QuestCompletionItem> QuestCompletionItems { get; set; }
		// 任务奖励经验值
		public int RewardExperiencePoints { get; set; }
		// 任务奖励金币
		public int RewardGold { get; set; }
		// 任务奖励物品
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
