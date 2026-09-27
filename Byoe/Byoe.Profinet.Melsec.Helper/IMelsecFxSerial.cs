using System.Threading.Tasks;
using Byoe.Core;

namespace Byoe.Profinet.Melsec.Helper
{
	/// <summary>
	/// 三菱的串口的接口类对象
	/// </summary>
	public interface IMelsecFxSerial : IReadWriteNet
	{
		/// <inheritdoc cref="M:Byoe.Profinet.Melsec.Helper.MelsecFxSerialHelper.ActivePlc(Byoe.Core.IReadWriteDevice)" />
		OperateResult ActivePlc();

		/// <inheritdoc cref="M:Byoe.Profinet.Melsec.Helper.MelsecFxSerialHelper.ActivePlc(Byoe.Core.IReadWriteDevice)" />
		Task<OperateResult> ActivePlcAsync();
	}
}
