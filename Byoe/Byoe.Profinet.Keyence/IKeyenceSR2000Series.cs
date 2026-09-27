using Byoe.Reflection;

namespace Byoe.Profinet.Keyence
{
	/// <summary>
	/// 基恩士SR2000系列扫码设备的通用接口
	/// </summary>
	internal interface IKeyenceSR2000Series
	{
		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.ReadBarcode(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		OperateResult<string> ReadBarcode();

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.Reset(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		OperateResult Reset();

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.OpenIndicator(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		OperateResult OpenIndicator();

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.CloseIndicator(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		OperateResult CloseIndicator();

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.ReadVersion(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		OperateResult<string> ReadVersion();

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.ReadCommandState(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		OperateResult<string> ReadCommandState();

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.ReadErrorState(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		OperateResult<string> ReadErrorState();

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.CheckInput(System.Int32,System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		OperateResult<bool> CheckInput(int number);

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.SetOutput(System.Int32,System.Boolean,System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		OperateResult SetOutput(int number, bool value);

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.ReadRecord(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		OperateResult<int[]> ReadRecord();

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.Lock(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		OperateResult Lock();

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.UnLock(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		OperateResult UnLock();

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.ReadCustomer(System.String,System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		OperateResult<string> ReadCustomer(string command);
	}
}
