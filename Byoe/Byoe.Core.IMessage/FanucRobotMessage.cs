using System;

namespace Byoe.Core.IMessage
{
	/// <summary>
	/// 发那科机器人的网络消息类
	/// </summary>
	public class FanucRobotMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="P:Byoe.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
		public int ProtocolHeadBytesLength => 56;

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			return BitConverter.ToUInt16(base.HeadBytes, 4);
		}
	}
}
