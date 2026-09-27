namespace Byoe.Core.IMessage
{
	/// <summary>
	/// 西门子PPI的消息信息
	/// </summary>
	public class SiemensPPIServerMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="P:Byoe.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
		public int ProtocolHeadBytesLength => 6;

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			return base.HeadBytes[1];
		}
	}
}
