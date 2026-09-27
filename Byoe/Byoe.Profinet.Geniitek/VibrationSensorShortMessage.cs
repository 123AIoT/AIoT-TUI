using Byoe.Core.IMessage;

namespace Byoe.Profinet.Geniitek
{
	/// <summary>
	/// 短消息的报文内容
	/// </summary>
	public class VibrationSensorShortMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="P:Byoe.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
		public int ProtocolHeadBytesLength => 9;

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.CheckHeadBytesLegal(System.Byte[])" />
		public override bool CheckHeadBytesLegal(byte[] token)
		{
			if (base.HeadBytes == null)
			{
				return false;
			}
			if (base.HeadBytes[0] == 170)
			{
				return true;
			}
			return false;
		}

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			return 0;
		}
	}
}
