using System.IO;

namespace Byoe.Core.IMessage
{
	/// <summary>
	/// RKC温度控制器的消息类
	/// </summary>
	public class RkcTemperatureMessage : NetMessageBase, INetMessage
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
			if (array.Length == 1)
			{
				if (array[0] == 6 || array[0] == 21)
				{
					return true;
				}
			}
			else if (array.Length == 6 && array[0] == 4 && array[array.Length - 1] == 5)
			{
				return true;
			}
			if (array.Length > 3 && array[0] == 2 && array[array.Length - 2] == 3)
			{
				return true;
			}
			if (array.Length > 6 && array[0] == 4 && array[3] == 2 && array[array.Length - 2] == 3)
			{
				return true;
			}
			return false;
		}
	}
}
