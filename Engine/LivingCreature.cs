using System.ComponentModel;

namespace Engine
{
	// INotifyPropertyChanged 在属性值改变时，会发送数据绑定通知
	public class LivingCreature : INotifyPropertyChanged
	{
		// 生命值
		private int _currentHitPoints;

		public int CurrentHitPoints
		{
			get { return _currentHitPoints; }
			set
			{
				_currentHitPoints = value;
				OnPropertyChanged("CurrentHitPoints");
			}
		}
		// 最大生命值
		public int MaximumHitPoints { get; set; }

		public bool IsDead { get { return CurrentHitPoints <= 0; } }

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

		// UI 订阅的事件监听
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// 这是“告诉”UI，什么东西(name)的值被改变了。
		/// The OnPropertyChanged() function checks if anything is subscribed to the event. If nothing
		/// is subscribed, then PropertyChanged will be null. If PropertyChanged is not null, then another
		/// class wants to be notified of changes, so the next line will run, and a PropertyChanged event
		/// will be raised(the notification will be sent out).
		/// </summary>
		/// <param name="name"></param>
		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
