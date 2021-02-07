using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
	/// <summary>
	/// 完成任务所需物品和其数量
	/// </summary>
	public class QuestCompletionItem
	{
		// 物品详情
		public Item Details { get; set; }
		// 物品数量
		public int Quantity { get; set; }

		/// <summary>
		/// 完成任务所需物品及其数量
		/// </summary>
		/// <param name="details">物品详情</param>
		/// <param name="quantity">物品数量</param>
		public QuestCompletionItem(Item details, int quantity)
		{
			Details = details;
			Quantity = quantity;
		}
	}
}
