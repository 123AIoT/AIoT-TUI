using System.Threading.Tasks;
using Byoe.Core;
using Byoe.Core.Device;
using Byoe.Core.IMessage;
using Byoe.Profinet.Omron.Helper;
using Byoe.Reflection;

namespace Byoe.Profinet.Omron
{
	/// <summary>
	/// 欧姆龙的HostLink的C-Mode实现形式，当前的类是通过以太网透传实现。地址支持携带站号信息，例如：s=2;D100<br />
	/// The C-Mode implementation form of Omron’s HostLink, the current class is realized through Ethernet transparent transmission. 
	/// Address supports carrying station number information, for example: s=2;D100
	/// </summary>
	/// <remarks>
	/// 暂时只支持的字数据的读写操作，不支持位的读写操作。另外本模式下，程序要在监视模式运行才能写数据，欧姆龙官方回复的。
	/// </remarks>
	public class OmronHostLinkCModeOverTcp : DeviceTcpNet, IHostLinkCMode, IReadWriteNet
	{
		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronHostLinkOverTcp.UnitNumber" />
		public byte UnitNumber { get; set; }

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.OmronFinsNet.#ctor" />
		public OmronHostLinkCModeOverTcp()
		{
			base.ByteTransform = new RegularByteTransform(DataFormat.CDAB);
			base.WordLength = 1;
			base.ByteTransform.IsStringReverseByteWord = true;
			LogMsgFormatBinary = false;
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.OmronCipNet.#ctor(System.String,System.Int32)" />
		public OmronHostLinkCModeOverTcp(string ipAddress, int port)
			: this()
		{
			IpAddress = ipAddress;
			Port = port;
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

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.Helper.OmronHostLinkCModeHelper.Read(Byoe.Core.IReadWriteDevice,System.Byte,System.String,System.UInt16)" />
		[ByoeMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			return OmronHostLinkCModeHelper.Read(this, UnitNumber, address, length);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.Helper.OmronHostLinkCModeHelper.Write(Byoe.Core.IReadWriteDevice,System.Byte,System.String,System.Byte[])" />
		[ByoeMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			return OmronHostLinkCModeHelper.Write(this, UnitNumber, address, value);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.OmronHostLinkCModeOverTcp.Read(System.String,System.UInt16)" />
		public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			return await OmronHostLinkCModeHelper.ReadAsync(this, UnitNumber, address, length);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.OmronHostLinkCModeOverTcp.Write(System.String,System.Byte[])" />
		public override async Task<OperateResult> WriteAsync(string address, byte[] value)
		{
			return await OmronHostLinkCModeHelper.WriteAsync(this, UnitNumber, address, value);
		}

		/// <inheritdoc />
		public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
		{
			return await ByoeHelper.ReadBoolAsync(this, address, length, 16, reverseByWord: true);
		}

		/// <inheritdoc />
		public override async Task<OperateResult> WriteAsync(string address, bool[] value)
		{
			return await ByoeHelper.WriteBoolAsync(this, address, value, 16, reverseByWord: true);
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
			return $"OmronHostLinkCModeOverTcp[{IpAddress}:{Port}]";
		}
	}
}
