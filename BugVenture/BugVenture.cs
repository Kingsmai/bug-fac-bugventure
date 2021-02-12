using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Engine;
namespace BugVenture
{
	public partial class BugVenture : Form
	{
		private Player _player;

		// 保存玩家存档的文件
		private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

		public BugVenture()
		{
			InitializeComponent();

			if (File.Exists(PLAYER_DATA_FILE_NAME))
			{
				_player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
			}
			else
			{
				_player = Player.CreateDefaultPlayer();
			}

			// 绑定数据（玩家数值）
			// For the lblHitPoints control, add a databinding – a subscription to a property's notifications.
			// The databinding will connect to the Text property of lblHitPoints to the 
			// CurrentHitPoints property of the _player object.
			lblHitPoints.DataBindings.Add("Text", _player, "CurrentHitPoints");
			lblGold.DataBindings.Add("Text", _player, "Gold");
			lblExperience.DataBindings.Add("Text", _player, "ExperiencePoints");
			lblLevel.DataBindings.Add("Text", _player, "Level");

			// 绑定数据（物品栏列表）
			// 不显示左边的header
			dgvInventory.RowHeadersVisible = false;
			// 阻止datagridview根据类的字段产生列表
			dgvInventory.AutoGenerateColumns = false;
			// 列表dataSource是角色的物品栏
			dgvInventory.DataSource = _player.Inventory;
			dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
			{
				HeaderText = "Name", // 显示在DataGridView里的字段
				Width = 197,
				DataPropertyName = "Description" // 数据段
			});
			dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
			{
				HeaderText = "Quantity",
				DataPropertyName = "Quantity"
			});

			// 绑定数据（任务栏列表）
			dgvQuest.RowHeadersVisible = false;
			dgvQuest.AutoGenerateColumns = false;
			dgvQuest.DataSource = _player.Quests;
			dgvQuest.Columns.Add(new DataGridViewTextBoxColumn
			{
				HeaderText = "Name",
				Width = 197,
				DataPropertyName = "Name"
			});
			dgvQuest.Columns.Add(new DataGridViewTextBoxColumn
			{
				HeaderText = "Done?",
				DataPropertyName = "IsCompleted"
			});

			// 绑定数据（武器列表和药品列表）
			cboWeapons.DataSource = _player.Weapons;
			cboWeapons.DisplayMember = "Name";
			cboWeapons.ValueMember = "ID";
			if (_player.CurrentWeapon != null)
			{
				cboWeapons.SelectedItem = _player.CurrentWeapon;
			}
			cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChanged;

			cboPotions.DataSource = _player.Potions;
			cboPotions.DisplayMember = "Name";
			cboPotions.ValueMember = "ID";

			_player.PropertyChanged += PlayerOnPropertyChange;

			// 信息事件监听
			_player.OnMessage += DisplayMessage;

			_player.MoveTo(_player.CurrentLocation);
		}

		private void btnNorth_Click(object sender, EventArgs e)
		{
			_player.MoveNorth();
		}

		private void btnSouth_Click(object sender, EventArgs e)
		{
			_player.MoveSouth();
		}

		private void btnEast_Click(object sender, EventArgs e)
		{
			_player.MoveEast();
		}

		private void btnWest_Click(object sender, EventArgs e)
		{
			_player.MoveWest();
		}

		private void cboWeapons_SelectedIndexChanged(object sender, EventArgs e)
		{
			_player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
		}

		private void btnUseWeapon_Click(object sender, EventArgs e)
		{
			// 从下拉框中获取当前选中的武器
			Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

			_player.UseWeapon(currentWeapon);
		}

		private void btnUsePotion_Click(object sender, EventArgs e)
		{
			// 获取当前选中的药品
			HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

			_player.UsePotion(potion);
		}

		private void PlayerOnPropertyChange(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs.PropertyName == "Weapons")
			{
				cboWeapons.DataSource = _player.Weapons;
				if (!_player.Weapons.Any())
				{
					cboWeapons.Visible = false;
					btnUseWeapon.Visible = false;
				}
			}
			if (propertyChangedEventArgs.PropertyName == "Potions")
			{
				cboPotions.DataSource = _player.Potions;
				if (!_player.Potions.Any())
				{
					cboPotions.Visible = false;
					btnUsePotion.Visible = false;
				}
			}

			if (propertyChangedEventArgs.PropertyName == "CurrentLocation")
			{
				// 显示/隐藏移动按钮（当有路的时候显示按钮）
				btnNorth.Visible = (_player.CurrentLocation.LocationToNorth != null);
				btnEast.Visible = (_player.CurrentLocation.LocationToEast != null);
				btnSouth.Visible = (_player.CurrentLocation.LocationToSouth != null);
				btnWest.Visible = (_player.CurrentLocation.LocationToWest != null);

				// 显示当前位置信息
				rtbLocation.Text += _player.CurrentLocation.Name + Environment.NewLine;
				rtbLocation.Text += _player.CurrentLocation.Description + Environment.NewLine;

				//
				if (_player.CurrentLocation.MonsterLivingHere == null)
				{
					cboWeapons.Visible = false;
					cboPotions.Visible = false;
					btnUseWeapon.Visible = false;
					btnUsePotion.Visible = false;
				}
				else
				{
					cboWeapons.Visible = _player.Weapons.Any();
					cboPotions.Visible = _player.Potions.Any();
					btnUseWeapon.Visible = _player.Weapons.Any();
					btnUsePotion.Visible = _player.Potions.Any();
				}
			}
		}

		// 更新玩家物品栏
		private void UpdateInventoryListInUI()
		{
			dgvInventory.RowHeadersVisible = false;
			dgvInventory.ColumnCount = 2;
			dgvInventory.Columns[0].Name = "Name";
			dgvInventory.Columns[0].Width = 197;
			dgvInventory.Columns[1].Name = "Quantity";
			dgvInventory.Rows.Clear();
			foreach (InventoryItem inventoryItem in _player.Inventory)
			{
				if (inventoryItem.Quantity > 0)
				{
					dgvInventory.Rows.Add(new[]
					{
						inventoryItem.Details.Name,
						inventoryItem.Quantity.ToString()
					});
				}
			}
		}

		// 更新玩家任务栏
		private void UpdateQuestListInUI()
		{
			dgvQuest.RowHeadersVisible = false;
			dgvQuest.ColumnCount = 2;
			dgvQuest.Columns[0].Name = "Name";
			dgvQuest.Columns[0].Width = 197;
			dgvQuest.Columns[1].Name = "Done?";
			dgvQuest.Rows.Clear();
			foreach (PlayerQuest playerQuest in _player.Quests)
			{
				dgvQuest.Rows.Add(new[]
				{
					playerQuest.Details.Name,
					playerQuest.IsCompleted.ToString()
				});
			}
		}

		// 更新玩家武器列表
		private void UpdateWeaponListInUI()
		{
			List<Weapon> weapons = new List<Weapon>();

			foreach (InventoryItem inventoryItem in _player.Inventory)
			{
				if (inventoryItem.Details is Weapon)
				{
					if (inventoryItem.Quantity > 0)
					{
						weapons.Add((Weapon)inventoryItem.Details);
					}
				}
			}
			if (weapons.Count == 0)
			{
				// 玩家没有任何武器，所以隐藏武器列表和使用按钮
				cboWeapons.Visible = false;
				btnUseWeapon.Visible = false;
			}
			else
			{
				// 断开事件监听（因为在切换数据的时候，会触发SelectedIndexChanged监听事件）
				cboWeapons.SelectedIndexChanged -= cboWeapons_SelectedIndexChanged;
				cboWeapons.DataSource = weapons;
				// 增加事件监听
				cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChanged;
				cboWeapons.DisplayMember = "Name";
				cboWeapons.ValueMember = "ID";
				if (_player.CurrentWeapon != null)
				{
					cboWeapons.SelectedItem = _player.CurrentWeapon;
				}
				else
				{
					cboWeapons.SelectedIndex = 0;
				}
			}
		}

		// 更新玩家恢复药水列表
		private void UpdatePotionListInUI()
		{
			List<HealingPotion> healingPotions = new List<HealingPotion>();
			foreach (InventoryItem inventoryItem in _player.Inventory)
			{
				if (inventoryItem.Details is HealingPotion)
				{
					if (inventoryItem.Quantity > 0)
					{
						healingPotions.Add((HealingPotion)inventoryItem.Details);
					}
				}
			}
			if (healingPotions.Count == 0)
			{
				// 玩家没有任何药水，所以隐藏药水列表和使用按钮
				cboPotions.Visible = false;
				btnUsePotion.Visible = false;
			}
			else
			{
				cboPotions.DataSource = healingPotions;
				cboPotions.DisplayMember = "Name";
				cboPotions.ValueMember = "ID";
				cboPotions.SelectedIndex = 0;
			}
		}

		// 更新玩家状态（绑定数据之后就不需要更新UI了，因为会自动根据玩家的属性更新）
		private void UpdatePlayerStatsInUI()
		{
			lblHitPoints.Text = _player.CurrentHitPoints.ToString();
			lblGold.Text = _player.Gold.ToString();
			lblExperience.Text = _player.ExperiencePoints.ToString();
			lblLevel.Text = _player.Level.ToString();
		}

		private void BugVenture_FormClosing(object sender, FormClosingEventArgs e)
		{
			File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXMLString());
		}

		private void DisplayMessage(object sender, MessageEventArgs messageEventArgs)
		{
			rtbMessages.Text += messageEventArgs.Message + Environment.NewLine;

			if (messageEventArgs.AddExtraNewLine)
			{
				rtbMessages.Text += Environment.NewLine;

			}

			// 滚动到最下方
			rtbMessages.SelectionStart = rtbMessages.Text.Length;
			rtbMessages.ScrollToCaret();
		}
	}
}
