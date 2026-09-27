using System.IO;

namespace Byoe.Core.IMessage
{
	/// <summary>
	/// 基恩士上位链路协议的消息
	/// </summary>
	public class KeyenceNanoSerialMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="P:Byoe.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
		public int ProtocolHeadBytesLength => -1;

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			return 0;
		}

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.CheckReceiveDataComplete(System.Byte[],System.IO.MemoryStream)" />
		public override bool CheckReceiveDataComplete(byte[] send, MemoryStream ms)
		{
			byte[] array = ms.ToArray();
			if (array.Length > 2)
			{
				return array[array.Length - 2] == 13 && array[array.Length - 1] == 10;
			}
			return true;
		}
	}
}
