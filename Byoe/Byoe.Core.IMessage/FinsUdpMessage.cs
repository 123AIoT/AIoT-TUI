namespace Byoe.Core.IMessage
{
	/// <summary>
	/// FinsUdp的消息
	/// </summary>
	public class FinsUdpMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="P:Byoe.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
		public int ProtocolHeadBytesLength => -1;

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			return 0;
		}

		/// <inheritdoc />
		public override int CheckMessageMatch(byte[] send, byte[] receive)
		{
			if (send == null || receive == null)
			{
				return 1;
			}
			if (send.Length >= 10 && receive.Length >= 10)
			{
				if (send[9] == receive[9])
				{
					return 1;
				}
				return -1;
			}
			return base.CheckMessageMatch(send, receive);
		}
	}
}
