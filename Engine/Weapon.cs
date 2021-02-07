using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
	public class Weapon : Item
	{
		// 武器最大攻击力
		public int MinimumDamage { get; set; }
		// 武器最小攻击力
		public int MaximumDamage { get; set; }

		/// <summary>
		/// 武器
		/// </summary>
		/// <param name="id">唯一ID</param>
		/// <param name="name">武器名</param>
		/// <param name="namePlural">复数武器名</param>
		/// <param name="minimumDamage">最小伤害</param>
		/// <param name="maximumDamage">最大伤害</param>
		public Weapon(int id, string name, string namePlural, int minimumDamage, int maximumDamage)
			: base (id, name, namePlural)
		{
			MinimumDamage = minimumDamage;
			MaximumDamage = maximumDamage;
		}
	}
}
