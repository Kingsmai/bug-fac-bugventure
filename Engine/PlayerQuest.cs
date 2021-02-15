using System.ComponentModel;

namespace Engine
{
	/// <summary>
	/// 玩家现有任务，保存玩家是否完成该任务
	/// </summary>
	public class PlayerQuest : INotifyPropertyChanged
	{
		private Quest _details;
		private bool _isCompleted;

		public Quest Details
		{
			get { return _details; }
			set
			{
				_details = value;
				OnPropertyChanged("Details");
			}
		}
		public bool IsCompleted
		{
			get { return _isCompleted; }
			set
			{
				_isCompleted = value;
				OnPropertyChanged("IsCompleted");
				OnPropertyChanged("Name");
			}
		}
		// 任务名（因为UI绑定时，不能通过Details.Name访问物品名）
		public string Name
		{
			get { return Details.Name; }
		}

		/// <summary>
		/// 玩家现有任务。使用时，将它作为一个列表放在玩家的类里边，用于保存玩家所有的任务状态。
		/// </summary>
		/// <param name="details">任务详情</param>
		public PlayerQuest(Quest details)
		{
			Details = details;
			IsCompleted = false; // 默认未完成
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
