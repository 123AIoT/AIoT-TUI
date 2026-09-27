using System;

namespace Byoe.LogNet
{
	/// <summary>
	/// 带有日志消息的事件
	/// </summary>
	public class ByoeEventArgs : EventArgs
	{
		/// <summary>
		/// 消息信息
		/// </summary>
		public ByoeMessageItem ByoeMessage { get; set; }
	}
}
