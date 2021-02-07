using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
	/// <summary>
	/// 怪物会掉落的物品、数量、掉落概率、保底战利品（如果没有任何战利品被随机出来）
	/// </summary>
	public class LootItem
	{
		// 物品详情
		public Item Details { get; set; }
		// 掉落概率
		public int DropPercentage { get; set; }
		/// <summary>
		/// 如果没有任何掉落物，此物品会不会作为保底掉落？
		/// </summary>
		public bool IsDefaultItem { get; set; }

		/// <summary>
		/// 会掉落物品，使用时，作为怪物的LootTable。保存掉落概率和是否为默认保底掉落物
		/// </summary>
		/// <param name="details">物品详情</param>
		/// <param name="dropPercentage">掉落概率</param>
		/// <param name="isDefaultItem">是否保底掉落</param>
		public LootItem(Item details, int dropPercentage, bool isDefaultItem)
		{

		}
	}
}
