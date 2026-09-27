using System;
using Byoe.Core;

namespace Byoe.Profinet.Omron.Helper
{
	/// <summary>
	/// Fins协议的接口对象
	/// </summary>
	public interface IOmronFins : IReadWriteDevice, IReadWriteNet
	{
		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronFinsNet.ICF" />
		byte ICF { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronFinsNet.RSV" />
		byte RSV { get; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronFinsNet.GCT" />
		byte GCT { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronFinsNet.DNA" />
		byte DNA { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronFinsNet.DA1" />
		byte DA1 { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronFinsNet.DA2" />
		byte DA2 { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronFinsNet.SNA" />
		byte SNA { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronFinsNet.SA1" />
		byte SA1 { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronFinsNet.SA2" />
		byte SA2 { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronFinsNet.SID" />
		byte SID { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronFinsNet.ReadSplits" />
		int ReadSplits { get; set; }

		/// <summary>
		/// 获取或设置欧姆龙PLC的系列信息<br />
		/// Obtain or set the series information of Omron PLC
		/// </summary>
		OmronPlcType PlcType { get; set; }

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.OmronFinsNetHelper.Run(Byoe.Core.IReadWriteDevice)" />
		OperateResult Run();

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.OmronFinsNetHelper.Stop(Byoe.Core.IReadWriteDevice)" />
		OperateResult Stop();

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.OmronFinsNetHelper.ReadCpuUnitData(Byoe.Core.IReadWriteDevice)" />
		OperateResult<OmronCpuUnitData> ReadCpuUnitData();

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.OmronFinsNetHelper.ReadCpuUnitStatus(Byoe.Core.IReadWriteDevice)" />
		OperateResult<OmronCpuUnitStatus> ReadCpuUnitStatus();

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.OmronFinsNetHelper.ReadCpuTime(Byoe.Core.IReadWriteDevice)" />
		OperateResult<DateTime> ReadCpuTime();
	}
}
