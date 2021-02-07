using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
	/// <summary>
	/// 背包中的物品，保存物品的信息和物品数量
	/// </summary>
	public class InventoryItem
	{
		// 物品详情
		public Item Details { get; set; }
		// 物品数量
		public int Quantity { get; set; }

		/// <summary>
		/// 背包中的物品
		/// </summary>
		/// <param name="details">物品详情</param>
		/// <param name="quantity">物品数量</param>
		public InventoryItem(Item details, int quantity)
		{
			Details = details;
			Quantity = quantity;
		}
	}
}
