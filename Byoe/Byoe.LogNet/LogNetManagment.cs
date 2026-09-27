using System;
using System.Text;

namespace Byoe.LogNet
{
	/// <summary>
	/// 日志类的管理器，提供了基本的功能代码。<br />
	/// The manager of the log class provides the basic function code.
	/// </summary>
	public class LogNetManagment
	{
		/// <summary>
		/// 日志文件的头标志
		/// </summary>
		internal const string LogFileHeadString = "Logs_";

		/// <summary>
		/// 公开的一个静态变量，允许随意的设置<br />
		/// Public static variable, allowing arbitrary setting
		/// </summary>
		public static ILogNet LogNet { get; set; }

		internal static string GetDegreeDescription(ByoeMessageDegree degree)
		{
			return degree switch
			{
				ByoeMessageDegree.DEBUG => StringResources.Language.LogNetDebug, 
				ByoeMessageDegree.INFO => StringResources.Language.LogNetInfo, 
				ByoeMessageDegree.WARN => StringResources.Language.LogNetWarn, 
				ByoeMessageDegree.ERROR => StringResources.Language.LogNetError, 
				ByoeMessageDegree.FATAL => StringResources.Language.LogNetFatal, 
				ByoeMessageDegree.None => StringResources.Language.LogNetAbandon, 
				_ => StringResources.Language.LogNetAbandon, 
			};
		}

		/// <summary>
		/// 通过异常文本格式化成字符串用于保存或发送<br />
		/// Formatted as a string with exception text for saving or sending
		/// </summary>
		/// <param name="text">文本消息</param>
		/// <param name="ex">异常</param>
		/// <returns>异常最终信息</returns>
		public static string GetSaveStringFromException(string text, Exception ex)
		{
			StringBuilder stringBuilder = new StringBuilder(text);
			if (ex != null)
			{
				if (!string.IsNullOrEmpty(text))
				{
					stringBuilder.Append(" : ");
				}
				try
				{
					stringBuilder.Append(StringResources.Language.ExceptionMessage);
					stringBuilder.Append(ex.Message);
					stringBuilder.Append(Environment.NewLine);
					stringBuilder.Append(StringResources.Language.ExceptionSource);
					stringBuilder.Append(ex.Source);
					stringBuilder.Append(Environment.NewLine);
					stringBuilder.Append(StringResources.Language.ExceptionStackTrace);
					stringBuilder.Append(ex.StackTrace);
					stringBuilder.Append(Environment.NewLine);
					stringBuilder.Append(StringResources.Language.ExceptionType);
					stringBuilder.Append(ex.GetType().ToString());
					stringBuilder.Append(Environment.NewLine);
					stringBuilder.Append(StringResources.Language.ExceptionTargetSite);
					stringBuilder.Append(ex.TargetSite?.ToString());
				}
				catch
				{
				}
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.Append("\u0002/=================================================[    Exception    ]================================================/");
			}
			try
			{
				return stringBuilder.ToString();
			}
			catch
			{
				return string.IsNullOrEmpty(text) ? ex.Message : (text + ":" + ex.Message);
			}
		}
	}
}
