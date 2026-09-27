namespace Byoe.Core
{
	/// <summary>
	/// 按照字节错位的数据转换类<br />
	/// Data conversion class according to byte misalignment
	/// </summary>
	public class ReverseWordTransform : RegularByteTransform
	{
		/// <inheritdoc cref="M:Byoe.Core.RegularByteTransform.#ctor" />
		public ReverseWordTransform()
		{
			base.DataFormat = DataFormat.CDAB;
		}

		/// <inheritdoc cref="M:Byoe.Core.RegularByteTransform.#ctor(Byoe.Core.DataFormat)" />
		public ReverseWordTransform(DataFormat dataFormat)
			: base(dataFormat)
		{
		}

		/// <inheritdoc cref="M:Byoe.Core.IByteTransform.CreateByDateFormat(Byoe.Core.DataFormat)" />
		public override IByteTransform CreateByDateFormat(DataFormat dataFormat)
		{
			return new ReverseWordTransform(dataFormat)
			{
				IsStringReverseByteWord = base.IsStringReverseByteWord
			};
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"ReverseWordTransform[{base.DataFormat}]";
		}
	}
}
