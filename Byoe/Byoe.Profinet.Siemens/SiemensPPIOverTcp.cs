using System.Threading.Tasks;
using Byoe.Core;
using Byoe.Core.Device;
using Byoe.Core.IMessage;
using Byoe.Profinet.Siemens.Helper;
using Byoe.Reflection;

namespace Byoe.Profinet.Siemens
{
	/// <inheritdoc cref="T:Byoe.Profinet.Siemens.SiemensPPI" />
	public class SiemensPPIOverTcp : DeviceTcpNet, ISiemensPPI, IReadWriteNet
	{
		private byte station = 2;

		private object communicationLock;

		/// <inheritdoc cref="P:Byoe.Profinet.Siemens.SiemensPPI.Station" />
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

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.SiemensPPI.#ctor" />
		public SiemensPPIOverTcp()
		{
			base.WordLength = 2;
			base.ByteTransform = new ReverseBytesTransform();
			communicationLock = new object();
		}

		/// <summary>
		/// 使用指定的ip地址和端口号来实例化对象<br />
		/// Instantiate the object with the specified IP address and port number
		/// </summary>
		/// <param name="ipAddress">Ip地址信息</param>
		/// <param name="port">端口号信息</param>
		public SiemensPPIOverTcp(string ipAddress, int port)
			: this()
		{
			IpAddress = ipAddress;
			Port = port;
		}

		/// <inheritdoc />
		protected override INetMessage GetNewNetMessage()
		{
			return new SiemensPPIMessage();
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.SiemensPPI.Read(System.String,System.UInt16)" />
		[ByoeMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			return SiemensPPIHelper.Read(this, address, length, Station, communicationLock);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.Helper.SiemensPPIHelper.ReadBool(Byoe.Core.IReadWriteDevice,System.String,System.Byte,System.Object)" />
		[ByoeMqttApi("ReadBool", "")]
		public override OperateResult<bool> ReadBool(string address)
		{
			return SiemensPPIHelper.ReadBool(this, address, Station, communicationLock);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.Helper.SiemensPPIHelper.ReadBool(Byoe.Core.IReadWriteDevice,System.String,System.Byte,System.Object)" />
		[ByoeMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			return SiemensPPIHelper.ReadBool(this, address, length, Station, communicationLock);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.SiemensPPI.Write(System.String,System.Byte[])" />
		[ByoeMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			return SiemensPPIHelper.Write(this, address, value, Station, communicationLock);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.SiemensPPI.Write(System.String,System.Boolean[])" />
		[ByoeMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] value)
		{
			return SiemensPPIHelper.Write(this, address, value, Station, communicationLock);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.SiemensPPI.ReadByte(System.String)" />
		[ByoeMqttApi("ReadByte", "")]
		public OperateResult<byte> ReadByte(string address)
		{
			return ByteTransformHelper.GetResultFromArray(Read(address, 1));
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.SiemensPPI.Write(System.String,System.Byte)" />
		[ByoeMqttApi("WriteByte", "")]
		public OperateResult Write(string address, byte value)
		{
			return Write(address, new byte[1] { value });
		}

		/// <inheritdoc />
		public override async Task<OperateResult<bool>> ReadBoolAsync(string address)
		{
			return await Task.Run(() => ReadBool(address));
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.SiemensPPIOverTcp.ReadByte(System.String)" />
		public async Task<OperateResult<byte>> ReadByteAsync(string address)
		{
			return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1));
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.SiemensPPIOverTcp.Write(System.String,System.Byte)" />
		public async Task<OperateResult> WriteAsync(string address, byte value)
		{
			return await WriteAsync(address, new byte[1] { value });
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.SiemensPPI.Start(System.String)" />
		[ByoeMqttApi]
		public OperateResult Start(string parameter = "")
		{
			return SiemensPPIHelper.Start(this, parameter, Station, communicationLock);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.SiemensPPI.Stop(System.String)" />
		[ByoeMqttApi]
		public OperateResult Stop(string parameter = "")
		{
			return SiemensPPIHelper.Stop(this, parameter, Station, communicationLock);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.Helper.SiemensPPIHelper.ReadPlcType(Byoe.Core.IReadWriteDevice,System.String,System.Byte,System.Object)" />
		[ByoeMqttApi]
		public OperateResult<string> ReadPlcType(string parameter = "")
		{
			return SiemensPPIHelper.ReadPlcType(this, parameter, Station, communicationLock);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.SiemensPPI.Start(System.String)" />
		public async Task<OperateResult> StartAsync(string parameter = "")
		{
			return await Task.Run(() => Start(parameter));
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.SiemensPPI.Stop(System.String)" />
		public async Task<OperateResult> StopAsync(string parameter = "")
		{
			return await Task.Run(() => Stop(parameter));
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.Helper.SiemensPPIHelper.ReadPlcType(Byoe.Core.IReadWriteDevice,System.String,System.Byte,System.Object)" />
		public async Task<OperateResult<string>> ReadPlcTypeAsync(string parameter = "")
		{
			return await Task.Run(() => SiemensPPIHelper.ReadPlcType(this, parameter, Station, communicationLock));
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"SiemensPPIOverTcp[{IpAddress}:{Port}]";
		}
	}
}
