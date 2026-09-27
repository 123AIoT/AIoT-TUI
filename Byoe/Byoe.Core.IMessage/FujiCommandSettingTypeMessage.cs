namespace Byoe.Core.IMessage
{
	/// <summary>
	/// Fuji的CommandSettingType的消息类
	/// </summary>
	public class FujiCommandSettingTypeMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="P:Byoe.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
		public int ProtocolHeadBytesLength => 5;

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			if (base.HeadBytes == null)
			{
				return 0;
			}
			return base.HeadBytes[4];
		}
	}
}
