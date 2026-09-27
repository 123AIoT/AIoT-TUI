using Byoe.Reflection;
using Byoe.Serial;

namespace Byoe.Profinet.Keyence
{
	/// <inheritdoc cref="T:Byoe.Profinet.Keyence.KeyenceSR2000SeriesTcp" />
	public class KeyenceSR2000Serial : SerialBase, IKeyenceSR2000Series
	{
		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000SeriesTcp.#ctor" />
		public KeyenceSR2000Serial()
		{
			base.ReceiveTimeOut = 10000;
			base.SleepTime = 20;
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.ReadBarcode(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		public OperateResult<string> ReadBarcode()
		{
			return KeyenceSR2000Helper.ReadBarcode(ReadFromCoreServer);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.Reset(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		public OperateResult Reset()
		{
			return KeyenceSR2000Helper.Reset(ReadFromCoreServer);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.OpenIndicator(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		public OperateResult OpenIndicator()
		{
			return KeyenceSR2000Helper.OpenIndicator(ReadFromCoreServer);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.CloseIndicator(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		public OperateResult CloseIndicator()
		{
			return KeyenceSR2000Helper.CloseIndicator(ReadFromCoreServer);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.ReadVersion(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		public OperateResult<string> ReadVersion()
		{
			return KeyenceSR2000Helper.ReadVersion(ReadFromCoreServer);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.ReadCommandState(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		public OperateResult<string> ReadCommandState()
		{
			return KeyenceSR2000Helper.ReadCommandState(ReadFromCoreServer);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.ReadErrorState(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		public OperateResult<string> ReadErrorState()
		{
			return KeyenceSR2000Helper.ReadErrorState(ReadFromCoreServer);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.CheckInput(System.Int32,System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		public OperateResult<bool> CheckInput(int number)
		{
			return KeyenceSR2000Helper.CheckInput(number, ReadFromCoreServer);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.SetOutput(System.Int32,System.Boolean,System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		public OperateResult SetOutput(int number, bool value)
		{
			return KeyenceSR2000Helper.SetOutput(number, value, ReadFromCoreServer);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.ReadRecord(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		public OperateResult<int[]> ReadRecord()
		{
			return KeyenceSR2000Helper.ReadRecord(ReadFromCoreServer);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.Lock(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		public OperateResult Lock()
		{
			return KeyenceSR2000Helper.Lock(ReadFromCoreServer);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.UnLock(System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		public OperateResult UnLock()
		{
			return KeyenceSR2000Helper.UnLock(ReadFromCoreServer);
		}

		/// <inheritdoc cref="M:Byoe.Profinet.Keyence.KeyenceSR2000Helper.ReadCustomer(System.String,System.Func{System.Byte[],Byoe.OperateResult{System.Byte[]}})" />
		[ByoeMqttApi]
		public OperateResult<string> ReadCustomer(string command)
		{
			return KeyenceSR2000Helper.ReadCustomer(command, ReadFromCoreServer);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"KeyenceSR2000Serial[{base.PortName}:{base.BaudRate}]";
		}
	}
}
