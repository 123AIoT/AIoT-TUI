using Byoe.Core;

namespace Byoe.Profinet.FATEK.Helper
{
	/// <summary>
	/// FatekProgram协议的接口
	/// </summary>
	public interface IFatekProgram : IReadWriteNet
	{
		/// <inheritdoc cref="M:Byoe.Profinet.FATEK.Helper.FatekProgramHelper.Run(Byoe.Core.IReadWriteDevice,System.Byte)" />
		OperateResult Run(byte station);

		/// <inheritdoc cref="M:Byoe.Profinet.FATEK.Helper.IFatekProgram.Run(System.Byte)" />
		OperateResult Run();

		/// <inheritdoc cref="M:Byoe.Profinet.FATEK.Helper.FatekProgramHelper.Stop(Byoe.Core.IReadWriteDevice,System.Byte)" />
		OperateResult Stop(byte station);

		/// <inheritdoc cref="M:Byoe.Profinet.FATEK.Helper.IFatekProgram.Stop(System.Byte)" />
		OperateResult Stop();

		/// <inheritdoc cref="M:Byoe.Profinet.FATEK.Helper.FatekProgramHelper.ReadStatus(Byoe.Core.IReadWriteDevice,System.Byte)" />
		OperateResult<bool[]> ReadStatus(byte station);

		/// <inheritdoc cref="M:Byoe.Profinet.FATEK.Helper.IFatekProgram.ReadStatus(System.Byte)" />
		OperateResult<bool[]> ReadStatus();
	}
}
