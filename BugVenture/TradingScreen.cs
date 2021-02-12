using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;

namespace BugVenture
{
	public partial class TradingScreen : Form
	{
		private Player _currentPlayer;

		public TradingScreen(Player player)
		{
			_currentPlayer = player;

			InitializeComponent();

			// 设置样式，用于将文字右侧对其
			DataGridViewCellStyle rightAlignedCellStyle = new DataGridViewCellStyle();
			rightAlignedCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

			// 填充玩家物品栏列表
			dgvMyItems.RowHeadersVisible = false;
			dgvMyItems.AutoGenerateColumns = false;

			// 此隐藏列保存物品ID，所以系统知道我们卖的物品是哪一个
			dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
			{
				DataPropertyName = "ItemID",
				Visible = false // 设置不可见，因为这个数据对玩家来说没意义
			});

			dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
			{
				HeaderText = "Name",
				Width = 100,
				DataPropertyName = "Description"
			});

			dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
			{
				HeaderText = "Qty",
				Width = 30,
				DefaultCellStyle = rightAlignedCellStyle, // 设置样式（上面定义的）
				DataPropertyName = "Quantity"
			});

			dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
			{
				HeaderText = "Price",
				Width = 35,
				DefaultCellStyle = rightAlignedCellStyle,
				DataPropertyName = "Price"
			});

			dgvMyItems.Columns.Add(new DataGridViewButtonColumn
			{
				Text = "Sell 1",
				UseColumnTextForButtonValue = true,
				Width = 50,
				DataPropertyName = "ItemID"
			});

			// 绑定角色物品栏到DGV
			dgvMyItems.DataSource = _currentPlayer.Inventory;

			// 当用户点击某一行，调用方法
			dgvMyItems.CellClick += dgvMyItems_CellClick;

			// 填充商家物品栏列表
			dgvVendorItems.RowHeadersVisible = false;
			dgvVendorItems.AutoGenerateColumns = false;

			// 此隐藏列保存物品ID，所以系统知道我们卖的物品是哪一个
			dgvVendorItems.Columns.Add(new DataGridViewTextBoxColumn
			{
				DataPropertyName = "ItemID",
				Visible = false
			});

			dgvVendorItems.Columns.Add(new DataGridViewTextBoxColumn
			{
				HeaderText = "Name",
				Width = 100,
				DataPropertyName = "Description"
			});

			dgvVendorItems.Columns.Add(new DataGridViewTextBoxColumn
			{
				HeaderText = "Price",
				Width = 35,
				DefaultCellStyle = rightAlignedCellStyle,
				DataPropertyName = "Price"
			});

			dgvVendorItems.Columns.Add(new DataGridViewButtonColumn
			{
				Text = "Buy 1",
				UseColumnTextForButtonValue = true,
				Width = 50,
				DataPropertyName = "ItemID"
			});

			// 绑定商贩物品栏到DGV
			dgvVendorItems.DataSource = _currentPlayer.CurrentLocation.VendorWorkingHere.Inventory;

			// 当用户点击某一行，调用方法
			dgvVendorItems.CellClick += dgvVendorItems_CellClick;
		}

		private void dgvMyItems_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			// ColumnIndex的下标是zero-based的
			// 第五个column (ColumnIndex = 4) 是按钮那一列
			// 当玩家点击按钮，将出售该物品
			if (e.ColumnIndex == 4)
			{
				// 获取第一列的ItemID
				var itemID = dgvMyItems.Rows[e.RowIndex].Cells[0].Value;

				// 获取当前需要卖出的道具
				Item itemBeingSold = World.ItemByID(Convert.ToInt32(itemID));

				if (itemBeingSold.Price == World.UNSELLABLE_ITEM_PRICE)
				{
					MessageBox.Show("You cannot sell the " + itemBeingSold.Name);
				}
				else
				{
					// 从角色物品栏里移除该道具
					_currentPlayer.RemoveItemFromInventory(itemBeingSold);

					// 增加玩家金钱
					_currentPlayer.Gold += itemBeingSold.Price;
				}
			}
		}

		private void dgvVendorItems_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			// 找到按钮的那一列（第4列）
			if (e.ColumnIndex == 3)
			{
				// 获取第一列的ItemID
				var itemID = dgvVendorItems.Rows[e.RowIndex].Cells[0].Value;

				// 获取当前需要购买的道具
				Item itemBeingBought = World.ItemByID(Convert.ToInt32(itemID));

				// 检查玩家是否拥有足够的金钱购买这个道具
				if (_currentPlayer.Gold >= itemBeingBought.Price)
				{
					// 将道具增加到玩家物品栏中
					_currentPlayer.AddItemToInventory(itemBeingBought);

					// 扣除玩家金币
					_currentPlayer.Gold -= itemBeingBought.Price;
				}
				else
				{
					MessageBox.Show("You do not have enough gold to buy the " + itemBeingBought.Name);
				}
			}
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
