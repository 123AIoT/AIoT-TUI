using System;
using System.Collections.Generic;
using Byoe.Core.Device;
using Byoe.Core.IMessage;
using Byoe.Core.Net;
using Byoe.Instrument.IEC.Helper;

namespace Byoe.Instrument.IEC
{
	/// <summary>
	/// IEC104的虚拟服务器，支持总召唤操作
	/// </summary>
	public class IEC104Server : DeviceServer
	{
		private IecValueServerObject<byte> list_单点遥信 = new IecValueServerObject<byte>(0, 100, 1, (IecValueObject<byte> m) => new byte[1] { (byte)(m.Quality | m.Value) });

		private IecValueServerObject<byte> list_双点遥信 = new IecValueServerObject<byte>(0, 100, 3, (IecValueObject<byte> m) => new byte[1] { (byte)(m.Quality | m.Value) });

		private IecValueServerObject<short> list_归一化遥测值 = new IecValueServerObject<short>(0, 100, 9, (IecValueObject<short> m) => BitConverter.GetBytes(m.Value));

		private IecValueServerObject<short> list_标度化遥测值 = new IecValueServerObject<short>(0, 100, 11, (IecValueObject<short> m) => BitConverter.GetBytes(m.Value));

		private IecValueServerObject<float> list_短浮点遥测值 = new IecValueServerObject<float>(0, 100, 13, (IecValueObject<float> m) => BitConverter.GetBytes(m.Value));

		private IecValueServerObject<uint> list_比特串 = new IecValueServerObject<uint>(0, 100, 7, (IecValueObject<uint> m) => BitConverter.GetBytes(m.Value));

		/// <summary>
		/// 获取或设置默认的数据单元公共地址，默认值为 1
		/// </summary>
		public byte PubAddress { get; set; } = 1;


		/// <summary>
		/// 单点遥信，类型ID为: M_SP_NA_1
		/// </summary>
		public IecValueServerObject<byte> SingleYaoXin => list_单点遥信;

		/// <summary>
		/// 双点遥信，类型ID为: M_DP_NA_1
		/// </summary>
		public IecValueServerObject<byte> DoubleYaoXin => list_双点遥信;

		/// <summary>
		/// 归一化遥测值，类型ID: M_ME_NA_1
		/// </summary>
		public IecValueServerObject<short> YaoCeA => list_归一化遥测值;

		/// <summary>
		/// 标度化遥测值，类型ID: M_ME_NB_1
		/// </summary>
		public IecValueServerObject<short> YaoCeB => list_标度化遥测值;

		/// <summary>
		/// 短浮点遥测值，类型ID: M_ME_NC_1
		/// </summary>
		public IecValueServerObject<float> YaoCeC => list_短浮点遥测值;

		/// <summary>
		/// 32位比特串，类型ID: M_BO_NA_1
		/// </summary>
		public IecValueServerObject<uint> BitArray => list_比特串;

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public IEC104Server()
		{
			base.Port = 2404;
			list_单点遥信.OnIecValueChanged = IecValueChangedHelper;
			list_双点遥信.OnIecValueChanged = IecValueChangedHelper;
			list_归一化遥测值.OnIecValueChanged = IecValueChangedHelper;
			list_标度化遥测值.OnIecValueChanged = IecValueChangedHelper;
			list_短浮点遥测值.OnIecValueChanged = IecValueChangedHelper;
			list_比特串.OnIecValueChanged = IecValueChangedHelper;
		}

		/// <inheritdoc />
		protected override INetMessage GetNewNetMessage()
		{
			return new IEC104Message();
		}

		private void IecValueChangedHelper<T>(IecValueServerObject<T> iecValueServer, IecValueObject<T> iecValue)
		{
			PipeSession[] pipeSessions = GetCommunicationServer().GetPipeSessions();
			foreach (PipeSession pipeSession in pipeSessions)
			{
				IECSessionInfo sessionInfo = pipeSession.Tag as IECSessionInfo;
				SendAsdu(pipeSession, sessionInfo, iecValueServer.GetAsduBreakOut(iecValue, PubAddress));
			}
		}

		/// <inheritdoc />
		protected override OperateResult<byte[]> ReadFromCoreServer(PipeSession session, byte[] receive)
		{
			if (receive == null || receive.Length < 6)
			{
				return OperateResult.CreateSuccessResult(new byte[0]);
			}
			if (receive[0] != 104)
			{
				return OperateResult.CreateSuccessResult(new byte[0]);
			}
			if (session.Tag == null)
			{
				session.Tag = new IECSessionInfo();
			}
			if (!receive[2].GetBoolByIndex(0) && !receive[4].GetBoolByIndex(0))
			{
				IECSessionInfo iECSessionInfo = session.Tag as IECSessionInfo;
				iECSessionInfo.IncrRecvMessageID();
				if (receive[6] == 100 && receive[8] == 6)
				{
					SendAsdu(session, iECSessionInfo, "64 01 07 00 01 00 00 00 00 14");
					SendAdsu(session, iECSessionInfo, list_单点遥信, 20, PubAddress);
					SendAdsu(session, iECSessionInfo, list_双点遥信, 20, PubAddress);
					SendAdsu(session, iECSessionInfo, list_归一化遥测值, 20, PubAddress);
					SendAdsu(session, iECSessionInfo, list_标度化遥测值, 20, PubAddress);
					SendAdsu(session, iECSessionInfo, list_短浮点遥测值, 20, PubAddress);
					SendAdsu(session, iECSessionInfo, list_比特串, 20, PubAddress);
					SendAsdu(session, iECSessionInfo, "64 01 0A 00 01 00 00 00 00 14");
				}
			}
			else if (receive[2].GetBoolByIndex(0) && !receive[4].GetBoolByIndex(0))
			{
				if (!receive[2].GetBoolByIndex(1))
				{
					return OperateResult.CreateSuccessResult(new byte[0]);
				}
				if (receive[2] == 7)
				{
					return OperateResult.CreateSuccessResult(IECHelper.PackIEC104Message(11, 0, 0, 0, null));
				}
				if (receive[2] == 19)
				{
					return OperateResult.CreateSuccessResult(IECHelper.PackIEC104Message(35, 0, 0, 0, null));
				}
				if (receive[2] == 67)
				{
					return OperateResult.CreateSuccessResult(IECHelper.PackIEC104Message(131, 0, 0, 0, null));
				}
			}
			return base.ReadFromCoreServer(session, receive);
		}

		private void SendAdsu<T>(PipeSession session, IECSessionInfo sessionInfo, IecValueServerObject<T> value, byte reason, byte station)
		{
			List<byte[]> asduCommand = value.GetAsduCommand(reason, station);
			for (int i = 0; i < asduCommand.Count; i++)
			{
				SendAsdu(session, sessionInfo, asduCommand[i]);
			}
		}

		private void SendAsdu(PipeSession session, IECSessionInfo sessionInfo, string asdu)
		{
			SendAsdu(session, sessionInfo, asdu.ToHexBytes());
		}

		private void SendAsdu(PipeSession session, IECSessionInfo sessionInfo, byte[] asdu)
		{
			byte[] array = IECHelper.PackIEC104Message((ushort)(sessionInfo.GetSendMessageID() * 2), (ushort)(sessionInfo.RecvMessageID * 2), asdu);
			session.Communication.Send(array);
			LogSendMessage(array, session);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"IEC104Server[{base.Port}]";
		}
	}
}
