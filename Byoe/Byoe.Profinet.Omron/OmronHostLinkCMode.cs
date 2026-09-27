using Byoe.Core;
using Byoe.Core.Device;
using Byoe.Core.IMessage;
using Byoe.Profinet.Omron.Helper;
using Byoe.Reflection;

namespace Byoe.Profinet.Omron
{
	/// <summary>
	/// 欧姆龙的HostLink的C-Mode实现形式，地址支持携带站号信息，例如：s=2;D100<br />
	/// Omron's HostLink C-Mode implementation form, the address supports carrying station number information, for example: s=2;D100
	/// </summary>
	/// <remarks>
	/// 暂时只支持的字数据的读写操作，不支持位的读写操作。另外本模式下，程序要在监视模式运行才能写数据，欧姆龙官方回复的。
	/// </remarks>
	public class OmronHostLinkCMode : DeviceSerialPort, IHostLinkCMode, IReadWriteNet
	{
		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronHostLinkOverTcp.UnitNumber" />
		public byte UnitNumber { get; set; }

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.OmronFinsNet.#ctor" />
		public OmronHostLinkCMode()
		{
			base.ByteTransform = new RegularByteTransform(DataFormat.CDAB);
			base.WordLength = 1;
			base.ByteTransform.IsStringReverseByteWord = true;
			LogMsgFormatBinary = false;
			base.ReceiveEmptyDataCount = 5;
		}

		/// <inheritdoc />
		protected override INetMessage GetNewNetMessage()
		{
			return new SpecifiedCharacterMessage(13);
		}

		/// <inheritdoc />
		[ByoeMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			return ByoeHelper.ReadBool(this, address, length, 16, reverseByWord: true);
		}

		/// <inheritdoc />
		[ByoeMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] value)
		{
			return ByoeHelper.WriteBool(this, address, value, 16, reverseByWord: true);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.OmronFinsNet.Read(System.String,System.UInt16)" />
		[ByoeMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			return OmronHostLinkCModeHelper.Read(this, UnitNumber, address, length);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.OmronFinsNet.Write(System.String,System.Byte[])" />
		[ByoeMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			return OmronHostLinkCModeHelper.Write(this, UnitNumber, address, value);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcType(Byoe.Core.IReadWriteDevice,System.Byte)" />
		[ByoeMqttApi("读取PLC的当前的型号信息")]
		public OperateResult<string> ReadPlcType()
		{
			return ReadPlcType(UnitNumber);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcType(Byoe.Core.IReadWriteDevice,System.Byte)" />
		public OperateResult<string> ReadPlcType(byte unitNumber)
		{
			return OmronHostLinkCModeHelper.ReadPlcType(this, unitNumber);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcMode(Byoe.Core.IReadWriteDevice,System.Byte)" />
		[ByoeMqttApi("读取PLC当前的操作模式，0: 编程模式  1: 运行模式  2: 监视模式")]
		public OperateResult<int> ReadPlcMode()
		{
			return ReadPlcMode(UnitNumber);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcMode(Byoe.Core.IReadWriteDevice,System.Byte)" />
		public OperateResult<int> ReadPlcMode(byte unitNumber)
		{
			return OmronHostLinkCModeHelper.ReadPlcMode(this, unitNumber);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ChangePlcMode(Byoe.Core.IReadWriteDevice,System.Byte,System.Byte)" />
		[ByoeMqttApi("将当前PLC的模式变更为指定的模式，0: 编程模式  1: 运行模式  2: 监视模式")]
		public OperateResult ChangePlcMode(byte mode)
		{
			return ChangePlcMode(UnitNumber, mode);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ChangePlcMode(Byoe.Core.IReadWriteDevice,System.Byte,System.Byte)" />
		public OperateResult ChangePlcMode(byte unitNumber, byte mode)
		{
			return OmronHostLinkCModeHelper.ChangePlcMode(this, unitNumber, mode);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"OmronHostLinkCMode[{base.PortName}:{base.BaudRate}]";
		}
	}
}
