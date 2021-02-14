namespace Engine
{
	public class HealingPotion : Item
	{
		// 恢复量
		public int AmountToHeal { get; set; }

		/// <summary>
		/// 回复药水
		/// </summary>
		/// <param name="id">物品的唯一ID</param>
		/// <param name="name">物品名称（单数）</param>
		/// <param name="namePlural">物品名称（复数）</param>
		/// <param name="amountToHeal">恢复量</param>
		public HealingPotion(int id, string name, string namePlural, int amountToHeal, int price)
			: base(id, name, namePlural, price) // base() 是父类的构造函数
		{
			AmountToHeal = amountToHeal;
		}
	}
}
