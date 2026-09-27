using Byoe.Core;

namespace Byoe.Profinet.Omron.Helper
{
	/// <summary>
	/// HostLink的接口实现
	/// </summary>
	public interface IHostLink : IReadWriteDevice, IReadWriteNet
	{
		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronHostLinkOverTcp.ICF" />
		byte ICF { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronHostLinkOverTcp.DA2" />
		byte DA2 { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronHostLinkOverTcp.SA2" />
		byte SA2 { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronHostLinkOverTcp.SID" />
		byte SID { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronHostLinkOverTcp.ResponseWaitTime" />
		byte ResponseWaitTime { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronHostLinkOverTcp.UnitNumber" />
		byte UnitNumber { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.OmronHostLinkOverTcp.ReadSplits" />
		int ReadSplits { get; set; }

		/// <inheritdoc cref="P:Byoe.Profinet.Omron.Helper.IOmronFins.PlcType" />
		OmronPlcType PlcType { get; set; }
	}
}
