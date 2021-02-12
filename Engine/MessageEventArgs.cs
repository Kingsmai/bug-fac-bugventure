using System;

namespace Engine
{
	/// <summary>
	/// 之前的信息更新通知，我们通过Player类提升一个事件，然后UI读取_player对象的新属性值
	/// 但对于message（显示信息）的处理，我们不能通过这个方式。因为Player类里没有message属性
	/// UI读取不了message属性值（因为没有）。
	/// <para>
	/// 因为如此，我们需要和event notification一起发送文字信息。
	/// 这就好比和UI说：“这个事件发生了，这是所有你所需要的信息”
	/// </para>
	/// <para>
	/// EventArgs class, a built-in class for event notifications.
	/// All custom event argument classes need to inherit from EventArgs.
	/// </para>
	/// </summary>
	public class MessageEventArgs : EventArgs
	{
		public string Message { get; private set; }
		public bool AddExtraNewLine { get; private set; }

		public MessageEventArgs(string message, bool addExtraNewLine)
		{
			Message = message;
			AddExtraNewLine = addExtraNewLine;
		}
	}
}
