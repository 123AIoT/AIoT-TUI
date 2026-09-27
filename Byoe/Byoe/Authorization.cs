using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Byoe.BasicFramework;
using Byoe.Core;
using Byoe.Core.Security;
using Byoe.Enthernet;
using Byoe.Language;
using Byoe.MQTT;

namespace Byoe
{
	/// <summary>
	/// 系统的基本授权类
	/// </summary>
	public class Authorization
	{
		private static bool nasogaghgaknnc;

		private static readonly string Declaration;

		private static DateTime niahdiahduasdbubfas;

		internal static long naihsdadaasdasdiwasdaid;

		internal static long moashdawidaisaosdas;

		internal static double nuasgdawydbishcgas;

		internal static int nuasgdaaydbishdgas;

		internal static int nuasgdawydbishdgas;

		internal static double nuasgdawydaishdgas;

		internal static int nasidhadguawdbasd;

		internal static int niasdhasdguawdwdad;

		internal static int hidahwdauushduasdhu;

		internal static long iahsduiwikaskfhishfdi;

		internal static int zxnkasdhiashifshfsofh;

		internal static int aosghasdugfavugaosidhaisf;

		static Authorization()
		{
			nasogaghgaknnc = false;
			Declaration = "----";
			niahdiahduasdbubfas = DateTime.Now;
			naihsdadaasdasdiwasdaid = 0L;
			moashdawidaisaosdas = 0L;
			nuasgdawydbishcgas = 8.0;
			nuasgdaaydbishdgas = 0;
			nuasgdawydbishdgas = 8;
			nuasgdawydaishdgas = 24.0;
			nasidhadguawdbasd = 1000;
			niasdhasdguawdwdad = 12345;
			hidahwdauushduasdhu = 23456;
			iahsduiwikaskfhishfdi = 0L;
			zxnkasdhiashifshfsofh = 0;
			aosghasdugfavugaosidhaisf = 0;
			niahdiahduasdbubfas = nashgaosgasaisfasfga();
			if (naihsdadaasdasdiwasdaid != 0)
			{
				naihsdadaasdasdiwasdaid = 0L;
			}
			if (nuasgdawydaishdgas != 24.0)
			{
				nuasgdawydaishdgas = 24.0;
			}
			if (nuasgdaaydbishdgas != 0)
			{
				nuasgdaaydbishdgas = 0;
			}
			if (nuasgdawydbishdgas != 24)
			{
				nuasgdawydbishdgas = 24;
			}
		}

		private static void asidhiahfaoaksdnasoif(object obj)
		{
			//for (int i = 0; i < 3600; i++)
			//{
			//	ByoeHelper.ThreadSleep(1000);
			//	if (naihsdadaasdasdiwasdaid == niasdhasdguawdwdad && nuasgdaaydbishdgas > 0)
			//	{
			//		return;
			//	}
			//}
			//NetSimplifyClient netSimplifyClient = new NetSimplifyClient("118.24.36.220", 18467);
			//netSimplifyClient.ReadCustomerFromServer(500, SoftBasic.FrameworkVersion.ToString());
		}

		private static void noajfojgkansdnfgaggh()
		{
			if (!nasogaghgaknnc)
			{
				if (niahdiahduasdbubfas < new DateTime(1980, 1, 1) && nashgaosgasaisfasfga() >= new DateTime(1980, 1, 1))
				{
					niahdiahduasdbubfas = nashgaosgasaisfasfga();
					nasogaghgaknnc = true;
				}
				else if (niahdiahduasdbubfas >= new DateTime(1980, 1, 1) && nashgaosgasaisfasfga() >= new DateTime(1980, 1, 1))
				{
					nasogaghgaknnc = true;
				}
			}
		}

		/// <summary>
		/// 普通VIP的授权的判定
		/// </summary>
		/// <returns>是否授权成功</returns>
		internal static bool nzugaydgwadawdibbas()
		{
			return true;
			//moashdawidaisaosdas++;
			//if (naihsdadaasdasdiwasdaid == niasdhasdguawdwdad && nuasgdaaydbishdgas > 0)
			//{
			//	return nuasduagsdwydbasudasd();
			//}
			//noajfojgkansdnfgaggh();
			//if ((nashgaosgasaisfasfga() - niahdiahduasdbubfas).TotalHours < nuasgdawydaishdgas)
			//{
			//	return nuasduagsdwydbasudasd();
			//}
			//return asdhuasdgawydaduasdgu();
		}

		/// <summary>
		/// 商业授权则返回true，否则返回false
		/// </summary>
		/// <returns>是否成功进行商业授权</returns>
		internal static bool asdniasnfaksndiqwhawfskhfaiw()
		{
			return true;
			//if (naihsdadaasdasdiwasdaid == niasdhasdguawdwdad && nuasgdaaydbishdgas > 0)
			//{
			//	return nuasduagsdwydbasudasd();
			//}
			//noajfojgkansdnfgaggh();
			//if ((nashgaosgasaisfasfga() - niahdiahduasdbubfas).TotalHours < (double)nuasgdawydbishdgas)
			//{
			//	return nuasduagsdwydbasudasd();
			//}
			//return asdhuasdgawydaduasdgu();
		}

		internal static bool nuasduagsdwydbasudasd()
		{
			return true;
		}

		internal static bool asdhuasdgawydaduasdgu()
		{
			return false;
		}

		internal static bool ashdadgawdaihdadsidas()
		{
			return niasdhasdguawdwdad == 12345;
		}

		internal static DateTime nashgaosgasaisfasfga()
		{
			return DateTime.Now;
		}

		internal static DateTime iashdagsaawbdawda()
		{
			return DateTime.Now.AddDays(1.0);
		}

		internal static DateTime iashdagsaawadawda()
		{
			return DateTime.Now.AddDays(2.0);
		}

		internal static void oasjodaiwfsodopsdjpasjpf()
		{
			Interlocked.Increment(ref iahsduiwikaskfhishfdi);
		}

		internal static string nasduabwduadawdb(string miawdiawduasdhasd)
		{
			StringBuilder stringBuilder = new StringBuilder();
			MD5 mD = MD5.Create();
			byte[] array = mD.ComputeHash(Encoding.Unicode.GetBytes(miawdiawduasdhasd));
			mD.Clear();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append((255 - array[i]).ToString("X2"));
			}
			return stringBuilder.ToString();
		}

		

	
		


	}
}
