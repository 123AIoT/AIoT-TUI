namespace Byoe.Core.IMessage
{
	/// <summary>
	/// IEC104消息结构信息
	/// </summary>
	public class IEC104Message : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="P:Byoe.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
		public int ProtocolHeadBytesLength => 2;

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			return base.HeadBytes[1];
		}
	}
}
