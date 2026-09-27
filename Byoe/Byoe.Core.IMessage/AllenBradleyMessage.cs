using System;

namespace Byoe.Core.IMessage
{
	/// <summary>
	/// 用于和 AllenBradley PLC 交互的消息协议类
	/// </summary>
	public class AllenBradleyMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="P:Byoe.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
		public int ProtocolHeadBytesLength => 24;

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			return BitConverter.ToUInt16(base.HeadBytes, 2);
		}
	}
}
