using System;

namespace Byoe.Core.IMessage
{
	/// <summary>
	/// Memobus协议的消息定义
	/// </summary>
	public class MemobusMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="P:Byoe.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
		public int ProtocolHeadBytesLength => 12;

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			if (base.HeadBytes?.Length >= ProtocolHeadBytesLength)
			{
				int num = BitConverter.ToUInt16(base.HeadBytes, 6) - 12;
				if (num < 0)
				{
					num = 0;
				}
				return num;
			}
			return 0;
		}
	}
}
