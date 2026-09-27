using System;

namespace Byoe.Reflection
{
	/// <summary>
	/// 对应redis的一个键值信息的内容
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class ByoeRedisKeyAttribute : Attribute
	{
		/// <summary>
		/// 键值的名称
		/// </summary>
		public string KeyName { get; set; }

		/// <summary>
		/// 根据键名来读取写入当前的数据信息
		/// </summary>
		/// <param name="key">键名</param>
		public ByoeRedisKeyAttribute(string key)
		{
			KeyName = key;
		}
	}
}
