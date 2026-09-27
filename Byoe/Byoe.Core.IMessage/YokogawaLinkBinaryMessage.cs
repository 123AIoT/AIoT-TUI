namespace Byoe.Core.IMessage
{
	/// <summary>
	/// 横河PLC的以太网的二进制协议
	/// </summary>
	public class YokogawaLinkBinaryMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="P:Byoe.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
		public int ProtocolHeadBytesLength => 4;

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			return base.HeadBytes[2] * 256 + base.HeadBytes[3];
		}
	}
}
