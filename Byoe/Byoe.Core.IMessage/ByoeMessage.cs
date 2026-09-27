using System;
using Byoe.BasicFramework;

namespace Byoe.Core.IMessage
{
	/// <summary>
	/// 本组件系统使用的默认的消息规则，说明解析和反解析规则的
	/// </summary>
	public class ByoeMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="P:Byoe.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
		public int ProtocolHeadBytesLength => 32;

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.CheckHeadBytesLegal(System.Byte[])" />
		public override bool CheckHeadBytesLegal(byte[] token)
		{
			if (base.HeadBytes == null)
			{
				return false;
			}
			byte[] headBytes = base.HeadBytes;
			if (headBytes != null && headBytes.Length >= 32)
			{
				return SoftBasic.IsTwoBytesEquel(base.HeadBytes, 12, token, 0, 16);
			}
			return false;
		}

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			byte[] headBytes = base.HeadBytes;
			if (headBytes != null && headBytes.Length >= 32)
			{
				return BitConverter.ToInt32(base.HeadBytes, 28);
			}
			return 0;
		}

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.GetHeadBytesIdentity" />
		public override int GetHeadBytesIdentity()
		{
			byte[] headBytes = base.HeadBytes;
			if (headBytes != null && headBytes.Length >= 32)
			{
				return BitConverter.ToInt32(base.HeadBytes, 4);
			}
			return 0;
		}
	}
}
