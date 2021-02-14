using System.ComponentModel;
using System.Linq;

namespace Engine
{
	public class Vendor
	{
		// 商店名字
		public string Name { get; set; }
		// 商店物品栏
		public BindingList<InventoryItem> Inventory { get; private set; }

		public Vendor(string name)
		{
			Name = name;
			Inventory = new BindingList<InventoryItem>();
		}

		// 商店行为：增加物品
		public void AddItemToInventory(Item itemToAdd, int quantity = 1)
		{
			InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);

			if (item == null)
			{
				// 该商贩没有这个物品，所以添加进它的物品栏
				Inventory.Add(new InventoryItem(itemToAdd, quantity));
			}
			else
			{
				// 他们拥有这个道具，则直接增加数量
				item.Quantity += quantity;
			}

			OnPropertyChanged("Inventory");
		}

		// 商店行为：移除物品
		public void RemoveItemFromInventory(Item itemToRemove, int quantity = 1)
		{
			InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToRemove.ID);

			if (item == null)
			{
				// 找不到这个道具，需要个错误提示
			}
			else
			{
				// 商家用有这个道具，直接减少
				item.Quantity -= quantity;

				// 不让物品变成负数
				if (item.Quantity < 0)
				{
					item.Quantity = 0;
				}

				// 当物品的数量 = 0，则删除该物品
				if (item.Quantity == 0)
				{
					Inventory.Remove(item);
				}

				// UI给予通知
				OnPropertyChanged("Inventory");
			}
		}

		// Raise property changed notification
		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
