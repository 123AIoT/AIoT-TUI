using System.IO;

namespace Byoe.Core.IMessage
{
	/// <summary>
	/// 消息类的基类
	/// </summary>
	public class NetMessageBase
	{
		/// <inheritdoc cref="P:Byoe.Core.IMessage.INetMessage.HeadBytes" />
		public byte[] HeadBytes { get; set; }

		/// <inheritdoc cref="P:Byoe.Core.IMessage.INetMessage.ContentBytes" />
		public byte[] ContentBytes { get; set; }

		/// <inheritdoc cref="P:Byoe.Core.IMessage.INetMessage.SendBytes" />
		public byte[] SendBytes { get; set; }

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.PependedUselesByteLength(System.Byte[])" />
		public virtual int PependedUselesByteLength(byte[] headByte)
		{
			return 0;
		}

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.GetHeadBytesIdentity" />
		public virtual int GetHeadBytesIdentity()
		{
			return 0;
		}

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.CheckHeadBytesLegal(System.Byte[])" />
		public virtual bool CheckHeadBytesLegal(byte[] token)
		{
			return true;
		}

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.CheckReceiveDataComplete(System.Byte[],System.IO.MemoryStream)" />
		public virtual bool CheckReceiveDataComplete(byte[] send, MemoryStream ms)
		{
			return true;
		}

		/// <inheritdoc cref="M:Byoe.Core.IMessage.INetMessage.CheckMessageMatch(System.Byte[],System.Byte[])" />
		public virtual int CheckMessageMatch(byte[] send, byte[] receive)
		{
			return 1;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return GetType().Name ?? "";
		}
	}
}
