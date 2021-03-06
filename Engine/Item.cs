﻿namespace Engine
{
	public class Item
	{
		// ID
		public int ID { get; set; }
		// 物品名（单数）
		public string Name { get; set; }
		// 物品名（多数）
		public string NamePlural { get; set; }
		// 单价
		public int Price { get; set; }

		/// <summary>
		/// 物品
		/// </summary>
		/// <param name="id">物品的唯一ID</param>
		/// <param name="name">物品名</param>
		/// <param name="namePlural">多个物品名</param>
		public Item(int id, string name, string namePlural, int price)
		{
			ID = id;
			Name = name;
			NamePlural = namePlural;
			Price = price;
		}
	}
}
