using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
	public class LivingCreature
	{
		// 生命值
		public int CurrentHitPoints { get; set; }
		// 最大生命值
		public int MaximumHitPoints { get; set; }

		/// <summary>
		/// 生物
		/// </summary>
		/// <param name="currentHitPoints">当前生命值</param>
		/// <param name="maximumHitPoints">最大生命值</param>
		public LivingCreature(int currentHitPoints, int maximumHitPoints)
		{
			CurrentHitPoints = currentHitPoints;
			MaximumHitPoints = maximumHitPoints;
		}
	}
}
