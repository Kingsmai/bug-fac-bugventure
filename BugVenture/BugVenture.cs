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
	public partial class BugVenture : Form
	{
		private Player _player;
		private Monster _currentMonster; // 当前面对的怪物

		public BugVenture()
		{
			InitializeComponent();

			// 创建角色
			_player = new Player(10, 10, 20, 0);
			// 初始位置（家）
			MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
			// 初始武器（锈剑）
			_player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));

			// UI显示玩家信息
			lblHitPoints.Text = _player.CurrentHitPoints.ToString();
			lblGold.Text = _player.Gold.ToString();
			lblExperience.Text = _player.ExperiencePoints.ToString();
			lblLevel.Text = _player.Level.ToString();
		}

		private void btnNorth_Click(object sender, EventArgs e)
		{
			MoveTo(_player.CurrentLocation.LocationToNorth);
		}

		private void btnSouth_Click(object sender, EventArgs e)
		{
			MoveTo(_player.CurrentLocation.LocationToSouth);
		}

		private void btnEast_Click(object sender, EventArgs e)
		{
			MoveTo(_player.CurrentLocation.LocationToEast);
		}

		private void btnWest_Click(object sender, EventArgs e)
		{
			MoveTo(_player.CurrentLocation.LocationToWest);
		}

		private void btnUseWeapon_Click(object sender, EventArgs e)
		{
			// 从下拉框中获取当前选中的武器
			Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

			// 确定对怪物造成的伤害量
			int damageToMonster = RandomNumberGenerator.NumberBetween(
				currentWeapon.MinimumDamage,
				currentWeapon.MaximumDamage);

			// 对怪物造成伤害
			_currentMonster.CurrentHitPoints -= damageToMonster;

			// 显示信息
			rtbMessages.Text += "You hit the " + _currentMonster.Name + " for " + damageToMonster.ToString() + " points." + Environment.NewLine;

			// 检查怪物是否死亡
			if (_currentMonster.CurrentHitPoints <= 0)
			{
				// 怪物死了
				rtbMessages.Text += Environment.NewLine;
				rtbMessages.Text += "You defeated the " + _currentMonster.Name + Environment.NewLine;

				// 奖励经验值
				_player.ExperiencePoints += _currentMonster.RewardExperiencePoints;
				rtbMessages.Text += "You receive " + _currentMonster.RewardExperiencePoints.ToString() + " experience points." + Environment.NewLine;

				// 奖励金币
				_player.Gold += _currentMonster.RewardGold;
				rtbMessages.Text += "You receive " + _currentMonster.RewardGold.ToString() + " gold." + Environment.NewLine;

				// 获取随机掉落物
				List<InventoryItem> lootedItems = new List<InventoryItem>();
				// 添加道具到lootedItems列表里，根据掉落率比较一个随机值
				foreach (LootItem lootItem in _currentMonster.LootTable)
				{
					if (RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
					{
						lootedItems.Add(new InventoryItem(lootItem.Details, 1));
					}
				}
				// 如果没有道具被随机选中，那么增加默认道具
				if (lootedItems.Count == 0)
				{
					foreach (LootItem lootItem in _currentMonster.LootTable)
					{
						if (lootItem.IsDefaultItem)
						{
							lootedItems.Add(new InventoryItem(lootItem.Details, 1));
						}
					}
				}

				// 将掉落物添加到玩家道具栏里。
				foreach (InventoryItem inventoryItem in lootedItems)
				{
					_player.AddItemToInventory(inventoryItem.Details);

					if (inventoryItem.Quantity == 1)
					{
						rtbMessages.Text += "You loot " + inventoryItem.Quantity.ToString() + " " + inventoryItem.Details.Name + Environment.NewLine;
					}
					else
					{
						rtbMessages.Text += "You loot " + inventoryItem.Quantity.ToString() + " " + inventoryItem.Details.NamePlural + Environment.NewLine;
					}
				}

				// 刷新角色信息和物品栏控制
				lblHitPoints.Text = _player.CurrentHitPoints.ToString();
				lblGold.Text = _player.Gold.ToString();
				lblExperience.Text = _player.ExperiencePoints.ToString();
				lblLevel.Text = _player.Level.ToString();

				UpdateInventoryListInUI();
				UpdateWeaponListInUI();
				UpdatePotionListInUI();

				// 在信息框中添加一行空行，美观
				rtbMessages.Text += Environment.NewLine;

				// 移动玩家到当前位置（用于刷新玩家、更新新的怪物）
				MoveTo(_player.CurrentLocation);
			}
			else
			{
				// 怪物仍然活着
				// 计算怪物对玩家造成的伤害
				int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

				// 显示信息
				rtbMessages.Text += "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine;

				// 扣去玩家生命值
				_player.CurrentHitPoints -= damageToPlayer;

				// 刷新玩家生命值
				lblHitPoints.Text = _player.CurrentHitPoints.ToString();

				// 检查玩家是否死亡
				if (_player.CurrentHitPoints <= 0)
				{
					// 显示信息
					rtbMessages.Text += "The " + _currentMonster.Name + " killed you." + Environment.NewLine;

					// 回到出生点
					MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
				}
			}
		}

		private void btnUsePotion_Click(object sender, EventArgs e)
		{
			// 获取当前选中的药品
			HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

			// 恢复玩家生命值
			_player.CurrentHitPoints += potion.AmountToHeal;

			// 当前生命值不能超过最大生命值
			if (_player.CurrentHitPoints > _player.MaximumHitPoints)
			{
				_player.CurrentHitPoints = _player.MaximumHitPoints;
			}

			// 从物品栏中删除药品
			foreach (InventoryItem inventoryItem in _player.Inventory)
			{
				if (inventoryItem.Details.ID == potion.ID)
				{
					inventoryItem.Quantity--;
					break;
				}
			}

			// 显示信息
			rtbMessages.Text += "You drink a " + potion.Name + Environment.NewLine;

			// 轮到怪物展开攻击
			// 计算怪物对玩家造成的伤害
			int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

			// 显示信息
			rtbMessages.Text += "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine;

			// 扣去玩家生命值
			_player.CurrentHitPoints -= damageToPlayer;

			// 刷新玩家生命值
			lblHitPoints.Text = _player.CurrentHitPoints.ToString();

			// 检查玩家是否死亡
			if (_player.CurrentHitPoints <= 0)
			{
				// 显示信息
				rtbMessages.Text += "The " + _currentMonster.Name + " killed you." + Environment.NewLine;

				// 回到出生点
				MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
			}

			// 刷新玩家UI
			lblHitPoints.Text = _player.CurrentHitPoints.ToString();
			UpdateInventoryListInUI();
			UpdatePotionListInUI();
		}

		private void MoveTo(Location newLocation)
		{
			// 如果玩家没有进入该场景的道具
			if (!_player.HasRequiredItemToEnterThisLocation(newLocation))
			{
				// 显示信息
				rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name +
					" to enter this location." + Environment.NewLine;
				return; // 结束事件操作（不让玩家进入）
			}

			// 更新玩家当前位置
			_player.CurrentLocation = newLocation;

			// 显示/隐藏上下左右的按钮
			btnNorth.Visible = (newLocation.LocationToNorth != null); // 当位置存在，则显示按钮
			btnEast.Visible = (newLocation.LocationToEast != null);
			btnSouth.Visible = (newLocation.LocationToSouth != null);
			btnWest.Visible = (newLocation.LocationToWest != null);

			// 显示当前位置名字和描述
			rtbLocation.Text = newLocation.Name + Environment.NewLine;
			rtbLocation.Text += newLocation.Description + Environment.NewLine;

			// 完全恢复角色
			_player.CurrentHitPoints = _player.MaximumHitPoints;

			// UI更新HP值
			lblHitPoints.Text = _player.CurrentHitPoints.ToString();

			// 当前地点是否有任务
			if (newLocation.QuestAvailableHere != null)
			{
				// 检查玩家是否拥有任务
				bool playerAlreadyHasQuest = _player.HasThisQuest(newLocation.QuestAvailableHere);
				// 检查玩家是否已经完成任务
				bool playerAlreadyCompletedQuest = _player.CompletedThisQuest(newLocation.QuestAvailableHere);

				// 遍历结束后，如果玩家已经拥有这个任务
				if (playerAlreadyHasQuest)
				{
					// 如果玩家还没完成这项任务
					if (!playerAlreadyCompletedQuest)
					{
						// 如果玩家拥有所有的任务道具
						bool playerHasAllItemsToCompleteQuest = _player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

						// 当玩家拥有通关任务所需要的道具
						if (playerHasAllItemsToCompleteQuest)
						{
							// 显示信息
							rtbMessages.Text += Environment.NewLine;
							rtbMessages.Text += "You complete the " +
								newLocation.QuestAvailableHere.Name +
								" quest." + Environment.NewLine;

							// 删除玩家任务栏里的任务道具
							_player.RemoveQuestCompletetionItems(newLocation.QuestAvailableHere);

							// 给予任务奖励道具，并显示信息
							rtbMessages.Text += "You receive: " + Environment.NewLine;
							rtbMessages.Text += newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine;
							rtbMessages.Text += newLocation.QuestAvailableHere.RewardGold.ToString() + " gold" + Environment.NewLine;
							rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
							rtbMessages.Text += Environment.NewLine;

							_player.ExperiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
							_player.Gold += newLocation.QuestAvailableHere.RewardGold;

							// 添加奖励道具到玩家任务栏
							_player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

							// 标记该任务已经被完成
							_player.MarkQuestCompleted(newLocation.QuestAvailableHere);
						}
					}
				}
				else // 如果玩家没有这个任务
				{
					// 显示信息
					rtbMessages.Text += "You received the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine;
					rtbMessages.Text += newLocation.QuestAvailableHere.Description + Environment.NewLine;
					rtbMessages.Text += "To complete it, return with: " + Environment.NewLine;
					foreach (QuestCompletionItem questCompletionItem in newLocation.QuestAvailableHere.QuestCompletionItems)
					{
						if (questCompletionItem.Quantity == 1)
						{
							rtbMessages.Text += questCompletionItem.Quantity.ToString() + " " + questCompletionItem.Details.Name + Environment.NewLine;
						}
						else
						{
							rtbMessages.Text += questCompletionItem.Quantity.ToString() + " " + questCompletionItem.Details.NamePlural + Environment.NewLine;
						}
					}
					rtbMessages.Text += Environment.NewLine;

					// 将任务添加到玩家人物列表里
					_player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
				}
			}

			// 检查该地点是否有怪物存在
			if (newLocation.MonsterLivingHere != null)
			{
				// 有怪物
				rtbMessages.Text += "You see a " + newLocation.MonsterLivingHere.Name + Environment.NewLine;
				// 用World类的值创建新的怪物对象
				Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

				_currentMonster = new Monster(
					standardMonster.ID,
					standardMonster.Name,
					standardMonster.MaximumDamage,
					standardMonster.RewardExperiencePoints,
					standardMonster.RewardGold,
					standardMonster.CurrentHitPoints,
					standardMonster.MaximumHitPoints);

				foreach (LootItem lootItem in standardMonster.LootTable)
				{
					_currentMonster.LootTable.Add(lootItem);
				}

				cboWeapons.Visible = true;
				cboPotions.Visible = true;
				btnUseWeapon.Visible = true;
				btnUsePotion.Visible = true;
			}
			else
			{
				_currentMonster = null;

				cboWeapons.Visible = false;
				cboPotions.Visible = false;
				btnUseWeapon.Visible = false;
				btnUsePotion.Visible = false;
			}

			// 更新UI
			UpdateInventoryListInUI();
			UpdateQuestListInUI();
			UpdateWeaponListInUI();
			UpdatePotionListInUI();
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
				cboWeapons.DataSource = weapons;
				cboWeapons.DisplayMember = "Name";
				cboWeapons.ValueMember = "ID";
				cboWeapons.SelectedIndex = 0;
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

		// 更新玩家状态
		private void UpdatePlayerStatsInUI()
		{
			lblHitPoints.Text = _player.CurrentHitPoints.ToString();
			lblGold.Text = _player.Gold.ToString();
			lblExperience.Text = _player.ExperiencePoints.ToString();
			lblLevel.Text = _player.Level.ToString();
		}

		// 滚动到最下方
		private void ScrollToBottomOfMessages()
		{
			rtbMessages.SelectionStart = rtbMessages.Text.Length;
			rtbMessages.ScrollToCaret();
		}
	}
}
