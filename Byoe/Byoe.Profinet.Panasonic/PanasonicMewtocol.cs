using System.Threading.Tasks;
using Byoe.Core;
using Byoe.Core.Device;
using Byoe.Core.IMessage;
using Byoe.Profinet.Panasonic.Helper;
using Byoe.Reflection;

namespace Byoe.Profinet.Panasonic
{
	/// <summary>
	/// 松下PLC的数据交互协议，采用Mewtocol协议通讯，支持的地址列表参考api文档<br />
	/// The data exchange protocol of Panasonic PLC adopts Mewtocol protocol for communication. For the list of supported addresses, refer to the api document.
	/// </summary>
	/// <remarks>
	/// 地址支持携带站号的访问方式，例如：s=2;D100
	/// </remarks>
	public class PanasonicMewtocol : DeviceSerialPort
	{
		/// <inheritdoc cref="P:Byoe.Profinet.Panasonic.PanasonicMewtocolOverTcp.Station" />
		public byte Station { get; set; }

		/// <inheritdoc cref="M:Byoe.Profinet.Panasonic.PanasonicMewtocolOverTcp.#ctor(System.Byte)" />
		public PanasonicMewtocol(byte station = 238)
		{
			base.ByteTransform = new RegularByteTransform();
			Station = station;
			base.ByteTransform.DataFormat = DataFormat.DCBA;
			base.WordLength = 1;
			base.ReceiveEmptyDataCount = 5;
			LogMsgFormatBinary = false;
		}

		/// <inheritdoc />
		protected override INetMessage GetNewNetMessage()
		{
			return new SpecifiedCharacterMessage(13);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Panasonic.PanasonicMewtocolOverTcp.Read(System.String,System.UInt16)" />
		[ByoeMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			return MewtocolHelper.Read(this, Station, address, length);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Panasonic.PanasonicMewtocolOverTcp.Write(System.String,System.Byte[])" />
		[ByoeMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			return MewtocolHelper.Write(this, Station, address, value);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Panasonic.PanasonicMewtocolOverTcp.ReadBool(System.String,System.UInt16)" />
		[ByoeMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			return MewtocolHelper.ReadBool(this, Station, address, length);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Panasonic.Helper.MewtocolHelper.ReadBool(Byoe.Core.IReadWriteDevice,System.Byte,System.String[])" />
		public OperateResult<bool[]> ReadBool(string[] address)
		{
			return MewtocolHelper.ReadBool(this, Station, address);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Panasonic.PanasonicMewtocolOverTcp.ReadBool(System.String)" />
		[ByoeMqttApi("ReadBool", "")]
		public override OperateResult<bool> ReadBool(string address)
		{
			return MewtocolHelper.ReadBool(this, Station, address);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Panasonic.PanasonicMewtocolOverTcp.Write(System.String,System.Boolean[])" />
		[ByoeMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] values)
		{
			return MewtocolHelper.Write(this, Station, address, values);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Panasonic.PanasonicMewtocolOverTcp.Write(System.String,System.Boolean)" />
		[ByoeMqttApi("WriteBool", "")]
		public override OperateResult Write(string address, bool value)
		{
			return MewtocolHelper.Write(this, Station, address, value);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Panasonic.Helper.MewtocolHelper.Write(Byoe.Core.IReadWriteDevice,System.Byte,System.String[],System.Boolean[])" />
		public OperateResult Write(string[] address, bool[] value)
		{
			return MewtocolHelper.Write(this, Station, address, value);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Panasonic.PanasonicMewtocol.ReadBool(System.String)" />
		public override async Task<OperateResult<bool>> ReadBoolAsync(string address)
		{
			return await Task.Run(() => ReadBool(address));
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Panasonic.PanasonicMewtocol.Write(System.String,System.Boolean)" />
		public override async Task<OperateResult> WriteAsync(string address, bool value)
		{
			return await Task.Run(() => Write(address, value));
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Panasonic.PanasonicMewtocol.ReadBool(System.String[])" />
		public async Task<OperateResult<bool[]>> ReadBoolAsync(string[] address)
		{
			return await MewtocolHelper.ReadBoolAsync(this, Station, address);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Panasonic.PanasonicMewtocol.Write(System.String[],System.Boolean[])" />
		public async Task<OperateResult> WriteAsync(string[] address, bool[] value)
		{
			return await MewtocolHelper.WriteAsync(this, Station, address, value);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Panasonic.Helper.MewtocolHelper.ReadPlcType(Byoe.Core.IReadWriteDevice,System.Byte)" />
		public OperateResult<string> ReadPlcType()
		{
			return MewtocolHelper.ReadPlcType(this, Station);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"PanasonicMewtocol[{base.PortName}:{base.BaudRate}]";
		}
	}
}
