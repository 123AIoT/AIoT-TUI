using Byoe.Core;
using Byoe.Core.Device;
using Byoe.Core.IMessage;
using Byoe.Core.Net;
using Byoe.Profinet.Melsec.Helper;
using Byoe.Reflection;

namespace Byoe.Profinet.Melsec
{
	/// <summary>
	/// 三菱计算机链接协议，适用FX3U系列，FX3G，FX3S等等系列，通常在PLC侧连接的是485的接线口<br />
	/// Mitsubishi Computer Link Protocol, suitable for FX3U series, FX3G, FX3S, etc., usually the 485 connection port is connected on the PLC side
	/// </summary>
	/// <remarks>
	/// 关于在PLC侧的配置信息，协议：专用协议  传送控制步骤：格式一  站号设置：0
	/// </remarks>
	public class MelsecFxLinks : DeviceSerialPort, IReadWriteFxLinks, IReadWriteDevice, IReadWriteNet, IReadWriteDeviceStation
	{
		private byte station = 0;

		private byte watiingTime = 0;

		private bool sumCheck = true;

		/// <inheritdoc cref="P:Byoe.Profinet.Melsec.MelsecFxLinksOverTcp.Station" />
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

		/// <inheritdoc cref="P:Byoe.Profinet.Melsec.MelsecFxLinksOverTcp.WaittingTime" />
		public byte WaittingTime
		{
			get
			{
				return watiingTime;
			}
			set
			{
				if (watiingTime > 15)
				{
					watiingTime = 15;
				}
				else
				{
					watiingTime = value;
				}
			}
		}

		/// <inheritdoc cref="P:Byoe.Profinet.Melsec.MelsecFxLinksOverTcp.SumCheck" />
		public bool SumCheck
		{
			get
			{
				return sumCheck;
			}
			set
			{
				sumCheck = value;
			}
		}

		/// <inheritdoc cref="P:Byoe.Profinet.Melsec.MelsecFxLinksOverTcp.Format" />
		public int Format { get; set; } = 1;


		/// <inheritdoc cref="M:Byoe.Profinet.Melsec.MelsecFxLinksOverTcp.#ctor" />
		public MelsecFxLinks()
		{
			base.ByteTransform = new RegularByteTransform();
			base.WordLength = 1;
			LogMsgFormatBinary = false;
		}

		/// <inheritdoc />
		protected override INetMessage GetNewNetMessage()
		{
			return new MelsecFxLinksMessage(Format, SumCheck);
		}

		/// <inheritdoc />
		public override byte[] PackCommandWithHeader(byte[] command)
		{
			return MelsecFxLinksHelper.PackCommandWithHeader(this, command);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Melsec.MelsecFxLinksOverTcp.Read(System.String,System.UInt16)" />
		[ByoeMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			return MelsecFxLinksHelper.Read(this, address, length);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Melsec.MelsecFxLinksOverTcp.Write(System.String,System.Byte[])" />
		[ByoeMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			return MelsecFxLinksHelper.Write(this, address, value);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Melsec.MelsecFxLinksOverTcp.ReadBool(System.String,System.UInt16)" />
		[ByoeMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			return MelsecFxLinksHelper.ReadBool(this, address, length);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Melsec.MelsecFxLinksOverTcp.Write(System.String,System.Boolean[])" />
		[ByoeMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] value)
		{
			return MelsecFxLinksHelper.Write(this, address, value);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Melsec.MelsecFxLinksOverTcp.StartPLC(System.String)" />
		[ByoeMqttApi(Description = "Start the PLC operation, you can carry additional parameter information and specify the station number. Example: s=2; Note: The semicolon is required.")]
		public OperateResult StartPLC(string parameter = "")
		{
			return MelsecFxLinksHelper.StartPLC(this, parameter);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Melsec.MelsecFxLinksOverTcp.StopPLC(System.String)" />
		[ByoeMqttApi(Description = "Stop PLC operation, you can carry additional parameter information and specify the station number. Example: s=2; Note: The semicolon is required.")]
		public OperateResult StopPLC(string parameter = "")
		{
			return MelsecFxLinksHelper.StopPLC(this, parameter);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Melsec.MelsecFxLinksOverTcp.ReadPlcType(System.String)" />
		[ByoeMqttApi(Description = "Read the PLC model information, you can carry additional parameter information, and specify the station number. Example: s=2; Note: The semicolon is required.")]
		public OperateResult<string> ReadPlcType(string parameter = "")
		{
			return MelsecFxLinksHelper.ReadPlcType(this, parameter);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"MelsecFxLinks[{base.PortName}:{base.BaudRate}]";
		}
	}
}
