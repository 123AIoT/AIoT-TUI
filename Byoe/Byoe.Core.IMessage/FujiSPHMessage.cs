using System;

namespace Byoe.Core.IMessage
{
	/// <summary>
	/// 富士SPH协议的报文消息
	/// </summary>
	public class FujiSPHMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="P:Byoe.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
		public int ProtocolHeadBytesLength => 20;

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			if (base.HeadBytes == null)
			{
				return 0;
			}
			return BitConverter.ToUInt16(base.HeadBytes, 18);
		}
	}
}
