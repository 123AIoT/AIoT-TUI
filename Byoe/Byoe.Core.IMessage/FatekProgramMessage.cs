using System.IO;
using Byoe.Profinet.FATEK.Helper;

namespace Byoe.Core.IMessage
{
	/// <summary>
	/// 永宏PLC编程口协议的消息类
	/// </summary>
	public class FatekProgramMessage : NetMessageBase, INetMessage
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
			return FatekProgramHelper.CheckReceiveDataComplete(ms);
		}
	}
}
