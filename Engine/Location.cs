using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
	public class Location
	{
		// ID
		public int ID { get; set; }
		// 地点名
		public string Name { get; set; }
		// 详情
		public string Description { get; set; }
		// 进入场景需要的物品（比如钥匙）
		public Item ItemRequiredToEnter { get; set; }
		// 这个场景的任务
		public Quest QuestAvailableHere { get; set; }
		// 在这个场景的生物
		public Monster MonsterLivingHere { get; set; }
		// 北方
		public Location LocationToNorth { get; set; }
		// 东方
		public Location LocationToEast { get; set; }
		// 南方
		public Location LocationToSouth { get; set; }
		// 西方
		public Location LocationToWest { get; set; }

		// 构造方法
		/// <summary>
		/// 地点（地图位置）
		/// </summary>
		/// <param name="id">唯一ID</param>
		/// <param name="name">位置名</param>
		/// <param name="description">位置描述</param>
		/// <param name="itemRequiredToEnter">进入该场景所需要的物品（比如钥匙）</param>
		/// <param name="questAvailableHere">这个场景会触发的任务</param>
		/// <param name="monsterLivingHere">在这个场景的怪物</param>
		public Location(int id, string name, string description,
			Item itemRequiredToEnter = null,
			Quest questAvailableHere = null,
			Monster monsterLivingHere = null)
		{
			ID = id;
			Name = name;
			Description = description;
			ItemRequiredToEnter = itemRequiredToEnter;
			QuestAvailableHere = questAvailableHere;
			MonsterLivingHere = monsterLivingHere;
		}
	}
}
