using Byoe.Core;

namespace Byoe.Profinet.Omron.Helper
{
	/// <summary>
	/// HostLinkCMode协议的接口信息
	/// </summary>
	public interface IHostLinkCMode : IReadWriteNet
	{
		/// <inheritdoc cref="M:Byoe.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcType(Byoe.Core.IReadWriteDevice,System.Byte)" />
		OperateResult<string> ReadPlcType();

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcType(Byoe.Core.IReadWriteDevice,System.Byte)" />
		OperateResult<string> ReadPlcType(byte unitNumber);

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcMode(Byoe.Core.IReadWriteDevice,System.Byte)" />
		OperateResult<int> ReadPlcMode();

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ReadPlcMode(Byoe.Core.IReadWriteDevice,System.Byte)" />
		OperateResult<int> ReadPlcMode(byte unitNumber);

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ChangePlcMode(Byoe.Core.IReadWriteDevice,System.Byte,System.Byte)" />
		OperateResult ChangePlcMode(byte mode);

		/// <inheritdoc cref="M:Byoe.Profinet.Omron.Helper.OmronHostLinkCModeHelper.ChangePlcMode(Byoe.Core.IReadWriteDevice,System.Byte,System.Byte)" />
		OperateResult ChangePlcMode(byte unitNumber, byte mode);
	}
}
