using Byoe.Core;

namespace Byoe.Profinet.Siemens.Helper
{
	/// <summary>
	/// 西门子PPI的公用接口信息
	/// </summary>
	public interface ISiemensPPI : IReadWriteNet
	{
		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.Helper.SiemensPPIHelper.Start(Byoe.Core.IReadWriteDevice,System.String,System.Byte,System.Object)" />
		OperateResult Start(string parameter = "");

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.Helper.SiemensPPIHelper.Stop(Byoe.Core.IReadWriteDevice,System.String,System.Byte,System.Object)" />
		OperateResult Stop(string parameter = "");

		/// <inheritdoc cref="M:Byoe.Profinet.Siemens.Helper.SiemensPPIHelper.ReadPlcType(Byoe.Core.IReadWriteDevice,System.String,System.Byte,System.Object)" />
		OperateResult<string> ReadPlcType(string parameter = "");
	}
}
