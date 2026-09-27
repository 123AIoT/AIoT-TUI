using System;
using System.Threading.Tasks;
using Byoe.Core;
using Byoe.Core.Device;
using Byoe.Core.IMessage;
using Byoe.Reflection;

namespace Byoe.ModBus
{
	/// <summary>
	/// Modbus-Rtu通讯协议的类库，多项式码0xA001，支持标准的功能码，也支持扩展的功能码实现，地址采用富文本的形式，详细见备注说明<br />
	/// Modbus-Rtu communication protocol class library, polynomial code 0xA001, supports standard function codes, 
	/// and also supports extended function code implementation. The address is in rich text. For details, see the remark
	/// </summary>
	/// <remarks>
	/// 本客户端支持的标准的modbus协议，Modbus-Tcp及Modbus-Udp内置的消息号会进行自增，地址支持富文本格式，具体参考示例代码。<br />
	/// 读取线圈，输入线圈，寄存器，输入寄存器的方法中的读取长度对商业授权用户不限制，内部自动切割读取，结果合并。
	/// </remarks>
	/// <example>
	/// <inheritdoc cref="T:Byoe.ModBus.ModbusTcpNet" path="example" />
	/// </example>
	public class ModbusRtu : DeviceSerialPort, IModbus, IReadWriteDevice, IReadWriteNet
	{
		private byte station = 1;

		private bool isAddressStartWithZero = true;

		private Func<string, byte, OperateResult<string>> addressMapping = (string address, byte modbusCode) => OperateResult.CreateSuccessResult(address);

		/// <inheritdoc cref="P:Byoe.ModBus.ModbusTcpNet.AddressStartWithZero" />
		public bool AddressStartWithZero
		{
			get
			{
				return isAddressStartWithZero;
			}
			set
			{
				isAddressStartWithZero = value;
			}
		}

		/// <inheritdoc cref="P:Byoe.ModBus.ModbusTcpNet.Station" />
		public byte Station
		{
			get
			{
				return station;
			}
			set
			{
				station = value;
			}
		}

		/// <inheritdoc cref="P:Byoe.ModBus.ModbusTcpNet.DataFormat" />
		public DataFormat DataFormat
		{
			get
			{
				return base.ByteTransform.DataFormat;
			}
			set
			{
				base.ByteTransform.DataFormat = value;
			}
		}

		/// <inheritdoc cref="P:Byoe.ModBus.ModbusTcpNet.IsStringReverse" />
		public bool IsStringReverse
		{
			get
			{
				return base.ByteTransform.IsStringReverseByteWord;
			}
			set
			{
				base.ByteTransform.IsStringReverseByteWord = value;
			}
		}

		/// <inheritdoc cref="P:Byoe.ModBus.IModbus.EnableWriteMaskCode" />
		public bool EnableWriteMaskCode { get; set; } = true;


		/// <inheritdoc cref="P:Byoe.ModBus.IModbus.BroadcastStation" />
		public int BroadcastStation { get; set; } = -1;


		/// <summary>
		/// 获取或设置当前是否启用站号检查的功能，默认启用，读写数据时将进行站号确认操作，如果需要忽略站号，则设置为 false 即可。<br />
		/// Gets or sets whether the station ID check function is enabled. It is enabled by default. When reading or writing data, the station ID is confirmed.
		/// </summary>
		public bool StationCheckMacth { get; set; } = true;


		/// <summary>
		/// 获取或设置是否启用CRC16校验码的检查功能，默认启用，如果需要忽略检查CRC16，则设置为 false 即可。<br />
		/// Gets or sets whether to enable the check function of CRC16 check code. It is enabled by default. If you need to ignore the check of CRC16, you can set it to false.
		/// </summary>
		public bool Crc16CheckEnable { get; set; } = true;


		/// <inheritdoc cref="P:Byoe.ModBus.IModbus.WordReadBatchLength" />
		public int WordReadBatchLength { get; set; } = 120;


		/// <summary>
		/// 实例化一个Modbus-Rtu协议的客户端对象<br />
		/// Instantiate a client object of the Modbus-Rtu protocol
		/// </summary>
		public ModbusRtu()
		{
			base.ByteTransform = new RegularByteTransform(DataFormat.CDAB);
			base.ReceiveEmptyDataCount = 8;
		}

		/// <summary>
		/// 指定Modbus从站的站号来初始化<br />
		/// Specify the station number of the Modbus slave to initialize
		/// </summary>
		/// <param name="station">Modbus从站的站号</param>
		public ModbusRtu(byte station = 1)
			: this()
		{
			this.station = station;
		}

		/// <inheritdoc />
		protected override INetMessage GetNewNetMessage()
		{
			return new ModbusRtuMessage(StationCheckMacth);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.IModbus.TranslateToModbusAddress(System.String,System.Byte)" />
		public virtual OperateResult<string> TranslateToModbusAddress(string address, byte modbusCode)
		{
			if (addressMapping != null)
			{
				return addressMapping(address, modbusCode);
			}
			return OperateResult.CreateSuccessResult(address);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.IModbus.RegisteredAddressMapping(System.Func{System.String,System.Byte,Byoe.OperateResult{System.String}})" />
		public void RegisteredAddressMapping(Func<string, byte, OperateResult<string>> mapping)
		{
			addressMapping = mapping;
		}

		/// <inheritdoc />
		public override byte[] PackCommandWithHeader(byte[] command)
		{
			return ModbusInfo.PackCommandToRtu(command);
		}

		/// <inheritdoc />
		public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
		{
			return ModbusHelper.ExtraRtuResponseContent(send, response, Crc16CheckEnable, BroadcastStation);
		}

		/// <summary>
		/// 将Modbus报文数据发送到当前的通道中，并从通道中接收Modbus的报文，通道将根据当前连接自动获取，本方法是线程安全的。<br />
		/// Send Modbus message data to the current channel, and receive Modbus messages from the channel. The channel will automatically obtain it according to the current connection. This method is thread-safe.
		/// </summary>
		/// <param name="send">发送的完整的报文信息</param>
		/// <returns>接收到的Modbus报文信息</returns>
		/// <remarks>
		/// 需要注意的是，本方法的发送和接收都只需要输入Modbus核心报文，例如读取寄存器0的字数据 01 03 00 00 00 01，最后面两个字节的CRC是自动添加的，收到的数据也是只有modbus核心报文，例如：01 03 02 00 00 , 已经成功校验CRC校验并移除了，所以在解析的时候需要注意。<br />
		/// It should be noted that the sending and receiving of this method only need to input Modbus core messages, for example, read the word data 01 03 00 00 00 01 of register 0, the last two bytes of CRC are automatically added, 
		/// and received The data is also only modbus core messages, for example: 01 03 02 00 00, CRC has been successfully checked and removed, so you need to pay attention when parsing.
		/// </remarks>
		public override OperateResult<byte[]> ReadFromCoreServer(byte[] send)
		{
			if (BroadcastStation >= 0 && send[0] == BroadcastStation)
			{
				return ReadFromCoreServer(send, hasResponseData: false, usePackAndUnpack: true);
			}
			return base.ReadFromCoreServer(send);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusRtu.ReadFromCoreServer(System.Byte[])" />
		public override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send)
		{
			if (BroadcastStation >= 0 && send[0] == BroadcastStation)
			{
				return ReadFromCoreServer(send, hasResponseData: false, usePackAndUnpack: true);
			}
			return await base.ReadFromCoreServerAsync(send);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusTcpNet.ReadCoil(System.String)" />
		public OperateResult<bool> ReadCoil(string address)
		{
			return ReadBool(address);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusTcpNet.ReadCoil(System.String,System.UInt16)" />
		public OperateResult<bool[]> ReadCoil(string address, ushort length)
		{
			return ReadBool(address, length);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusTcpNet.ReadDiscrete(System.String)" />
		public OperateResult<bool> ReadDiscrete(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadDiscrete(address, 1));
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusTcpNet.ReadDiscrete(System.String,System.UInt16)" />
		public OperateResult<bool[]> ReadDiscrete(string address, ushort length)
		{
			return ModbusHelper.ReadBoolHelper(this, address, length, 2);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusTcpNet.Read(System.String,System.UInt16)" />
		[ByoeMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			return ModbusHelper.Read(this, address, length);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusHelper.ReadWrite(Byoe.ModBus.IModbus,System.String,System.UInt16,System.String,System.Byte[])" />
		[ByoeMqttApi("ReadWrite", "Use 0x17 function code to write and read data at the same time, and use a message to implement it")]
		public OperateResult<byte[]> ReadWrite(string readAddress, ushort length, string writeAddress, byte[] value)
		{
			return ModbusHelper.ReadWrite(this, readAddress, length, writeAddress, value);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusTcpNet.Write(System.String,System.Byte[])" />
		[ByoeMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			return ModbusHelper.Write(this, address, value);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusTcpNet.Write(System.String,System.Int16)" />
		[ByoeMqttApi("WriteInt16", "")]
		public override OperateResult Write(string address, short value)
		{
			return ModbusHelper.Write(this, address, value);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusTcpNet.Write(System.String,System.UInt16)" />
		[ByoeMqttApi("WriteUInt16", "")]
		public override OperateResult Write(string address, ushort value)
		{
			return ModbusHelper.Write(this, address, value);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusTcpNet.WriteMask(System.String,System.UInt16,System.UInt16)" />
		[ByoeMqttApi("WriteMask", "")]
		public OperateResult WriteMask(string address, ushort andMask, ushort orMask)
		{
			return ModbusHelper.WriteMask(this, address, andMask, orMask);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusHelper.ReadFile(Byoe.ModBus.IModbus,System.Byte,System.UInt16,System.UInt16,System.UInt16)" />
		[ByoeMqttApi("ReadFile", "")]
		public OperateResult<byte[]> ReadFile(ushort fileNumber, ushort address, ushort length)
		{
			return ModbusHelper.ReadFile(this, Station, fileNumber, address, length);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusHelper.WriteFile(Byoe.ModBus.IModbus,System.Byte,System.UInt16,System.UInt16,System.Byte[])" />"/&gt;
		[ByoeMqttApi("WriteFile", "")]
		public OperateResult WriteFile(ushort fileNumber, ushort address, byte[] data)
		{
			return ModbusHelper.WriteFile(this, Station, fileNumber, address, data);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusRtu.Write(System.String,System.Int16)" />
		public OperateResult WriteOneRegister(string address, short value)
		{
			return Write(address, value);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusRtu.Write(System.String,System.UInt16)" />
		public OperateResult WriteOneRegister(string address, ushort value)
		{
			return Write(address, value);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusRtu.Write(System.String,System.Int16)" />/param&gt;
		public override async Task<OperateResult> WriteAsync(string address, short value)
		{
			return await Task.Run(() => Write(address, value));
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusRtu.Write(System.String,System.UInt16)" />/param&gt;
		public override async Task<OperateResult> WriteAsync(string address, ushort value)
		{
			return await Task.Run(() => Write(address, value));
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusRtu.ReadCoil(System.String)" />
		public async Task<OperateResult<bool>> ReadCoilAsync(string address)
		{
			return await Task.Run(() => ReadCoil(address));
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusRtu.ReadCoil(System.String,System.UInt16)" />
		public async Task<OperateResult<bool[]>> ReadCoilAsync(string address, ushort length)
		{
			return await Task.Run(() => ReadCoil(address, length));
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusRtu.ReadDiscrete(System.String)" />
		public async Task<OperateResult<bool>> ReadDiscreteAsync(string address)
		{
			return await Task.Run(() => ReadDiscrete(address));
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusRtu.ReadDiscrete(System.String,System.UInt16)" />
		public async Task<OperateResult<bool[]>> ReadDiscreteAsync(string address, ushort length)
		{
			return await Task.Run(() => ReadDiscrete(address, length));
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusRtu.WriteOneRegister(System.String,System.Int16)" />
		public async Task<OperateResult> WriteOneRegisterAsync(string address, short value)
		{
			return await Task.Run(() => WriteOneRegister(address, value));
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusRtu.WriteOneRegister(System.String,System.UInt16)" />
		public async Task<OperateResult> WriteOneRegisterAsync(string address, ushort value)
		{
			return await Task.Run(() => WriteOneRegister(address, value));
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusRtu.WriteMask(System.String,System.UInt16,System.UInt16)" />
		public async Task<OperateResult> WriteMaskAsync(string address, ushort andMask, ushort orMask)
		{
			return await Task.Run(() => WriteMask(address, andMask, orMask));
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusTcpNet.ReadBool(System.String,System.UInt16)" />
		[ByoeMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			return ModbusHelper.ReadBoolHelper(this, address, length, 1);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusTcpNet.Write(System.String,System.Boolean[])" />
		[ByoeMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] values)
		{
			return ModbusHelper.Write(this, address, values);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusTcpNet.Write(System.String,System.Boolean)" />
		[ByoeMqttApi("WriteBool", "")]
		public override OperateResult Write(string address, bool value)
		{
			return ModbusHelper.Write(this, address, value);
		}

		/// <inheritdoc cref="M:Byoe.ModBus.ModbusRtu.Write(System.String,System.Boolean)" />
		public override async Task<OperateResult> WriteAsync(string address, bool value)
		{
			return await Task.Run(() => Write(address, value));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.ReadInt32(System.String,System.UInt16)" />
		[ByoeMqttApi("ReadInt32Array", "")]
		public override OperateResult<int[]> ReadInt32(string address, ushort length)
		{
			IByteTransform transform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 2)), (byte[] m) => transform.TransInt32(m, 0, length));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.ReadUInt32(System.String,System.UInt16)" />
		[ByoeMqttApi("ReadUInt32Array", "")]
		public override OperateResult<uint[]> ReadUInt32(string address, ushort length)
		{
			IByteTransform transform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 2)), (byte[] m) => transform.TransUInt32(m, 0, length));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.ReadFloat(System.String,System.UInt16)" />
		[ByoeMqttApi("ReadFloatArray", "")]
		public override OperateResult<float[]> ReadFloat(string address, ushort length)
		{
			IByteTransform transform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 2)), (byte[] m) => transform.TransSingle(m, 0, length));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.ReadInt64(System.String,System.UInt16)" />
		[ByoeMqttApi("ReadInt64Array", "")]
		public override OperateResult<long[]> ReadInt64(string address, ushort length)
		{
			IByteTransform transform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 4)), (byte[] m) => transform.TransInt64(m, 0, length));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.ReadUInt64(System.String,System.UInt16)" />
		[ByoeMqttApi("ReadUInt64Array", "")]
		public override OperateResult<ulong[]> ReadUInt64(string address, ushort length)
		{
			IByteTransform transform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 4)), (byte[] m) => transform.TransUInt64(m, 0, length));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.ReadDouble(System.String,System.UInt16)" />
		[ByoeMqttApi("ReadDoubleArray", "")]
		public override OperateResult<double[]> ReadDouble(string address, ushort length)
		{
			IByteTransform transform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * base.WordLength * 4)), (byte[] m) => transform.TransDouble(m, 0, length));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.Write(System.String,System.Int32[])" />
		[ByoeMqttApi("WriteInt32Array", "")]
		public override OperateResult Write(string address, int[] values)
		{
			IByteTransform byteTransform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return Write(address, byteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.Write(System.String,System.UInt32[])" />
		[ByoeMqttApi("WriteUInt32Array", "")]
		public override OperateResult Write(string address, uint[] values)
		{
			IByteTransform byteTransform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return Write(address, byteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.Write(System.String,System.Single[])" />
		[ByoeMqttApi("WriteFloatArray", "")]
		public override OperateResult Write(string address, float[] values)
		{
			IByteTransform byteTransform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return Write(address, byteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.Write(System.String,System.Int64[])" />
		[ByoeMqttApi("WriteInt64Array", "")]
		public override OperateResult Write(string address, long[] values)
		{
			IByteTransform byteTransform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return Write(address, byteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.Write(System.String,System.UInt64[])" />
		[ByoeMqttApi("WriteUInt64Array", "")]
		public override OperateResult Write(string address, ulong[] values)
		{
			IByteTransform byteTransform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return Write(address, byteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.Write(System.String,System.Double[])" />
		[ByoeMqttApi("WriteDoubleArray", "")]
		public override OperateResult Write(string address, double[] values)
		{
			IByteTransform byteTransform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return Write(address, byteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.ReadInt32Async(System.String,System.UInt16)" />
		public override async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
		{
			IByteTransform transform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 2)), (byte[] m) => transform.TransInt32(m, 0, length));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.ReadUInt32Async(System.String,System.UInt16)" />
		public override async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
		{
			IByteTransform transform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 2)), (byte[] m) => transform.TransUInt32(m, 0, length));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.ReadFloatAsync(System.String,System.UInt16)" />
		public override async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
		{
			IByteTransform transform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 2)), (byte[] m) => transform.TransSingle(m, 0, length));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.ReadInt64Async(System.String,System.UInt16)" />
		public override async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
		{
			IByteTransform transform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 4)), (byte[] m) => transform.TransInt64(m, 0, length));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.ReadUInt64Async(System.String,System.UInt16)" />
		public override async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
		{
			IByteTransform transform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 4)), (byte[] m) => transform.TransUInt64(m, 0, length));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.ReadDoubleAsync(System.String,System.UInt16)" />
		public override async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
		{
			IByteTransform transform = ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform);
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * base.WordLength * 4)), (byte[] m) => transform.TransDouble(m, 0, length));
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.WriteAsync(System.String,System.Int32[])" />
		public override async Task<OperateResult> WriteAsync(string address, int[] values)
		{
			return await WriteAsync(value: ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address);
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.WriteAsync(System.String,System.UInt32[])" />
		public override async Task<OperateResult> WriteAsync(string address, uint[] values)
		{
			return await WriteAsync(value: ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address);
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.WriteAsync(System.String,System.Single[])" />
		public override async Task<OperateResult> WriteAsync(string address, float[] values)
		{
			return await WriteAsync(value: ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address);
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.WriteAsync(System.String,System.Int64[])" />
		public override async Task<OperateResult> WriteAsync(string address, long[] values)
		{
			return await WriteAsync(value: ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address);
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.WriteAsync(System.String,System.UInt64[])" />
		public override async Task<OperateResult> WriteAsync(string address, ulong[] values)
		{
			return await WriteAsync(value: ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address);
		}

		/// <inheritdoc cref="M:Byoe.Core.IReadWriteNet.WriteAsync(System.String,System.Double[])" />
		public override async Task<OperateResult> WriteAsync(string address, double[] values)
		{
			return await WriteAsync(value: ByoeHelper.ExtractTransformParameter(ref address, base.ByteTransform).TransByte(values), address: address);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"ModbusRtu[{base.PortName}:{base.BaudRate}]";
		}
	}
}
