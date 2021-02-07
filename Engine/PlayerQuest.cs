using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
	/// <summary>
	/// 玩家现有任务，保存玩家是否完成该任务
	/// </summary>
	public class PlayerQuest
	{
		// 任务详情
		public Quest Details { get; set; }
		// 是否完成
		public bool IsCompleted { get; set; }

		/// <summary>
		/// 玩家现有任务。使用时，将它作为一个列表放在玩家的类里边，用于保存玩家所有的任务状态。
		/// </summary>
		/// <param name="details">任务详情</param>
		public PlayerQuest(Quest details)
		{
			Details = details;
			IsCompleted = false; // 默认未完成
		}
	}
}
