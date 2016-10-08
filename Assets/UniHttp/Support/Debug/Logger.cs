using UnityEngine;

namespace UniHttp
{
	public class Logger
	{
		public enum Level { Debug, Info, Warning, Error, Exception }

		public static Level LogLevel = LogLevel.Info;
		public static UnityEngine.Logger logger = UnityEngine.Debug.logger;
		static string kTAG = "UniHttp";

		#region Debug
		public static void Debug(object message)
		{
			logger.Log(kTAG, message);
		}

		public static void Debug(object message, Object context)
		{
			logger.Log(kTAG, message, context);
		}

		public static void DebugFormat(string format, params object objects)
		{
			logger.Log(kTAG, string.Format(format, objects));
		}

		public static void DebugFormat(Object context, string format, params object objects)
		{
			logger.Log(kTAG, string.Format(format, objects), context);
		}
		#endregion

		#region Info
		public static void Info(object message)
		{
			logger.Log(kTAG, message);
		}

		public static void Info(object message, Object context)
		{
			logger.Log(kTAG, message, context);
		}

		public static void InfoFormat(string format, params object objects)
		{
			logger.Log(kTAG, string.Format(format, objects));
		}

		public static void InfoFormat(Object context, string format, params object objects)
		{
			logger.Log(kTAG, string.Format(format, objects), context);
		}
		#endregion

		#region Warning
		public static void Warning(object message)
		{
			logger.LogWarning(kTAG, message);
		}

		public static void Warning(object message, Object context)
		{
			logger.LogWarning(kTAG, message, context);
		}

		public static void WarningFormat(string format, params object objects)
		{
			logger.LogWarning(kTAG, string.Format(format, objects));
		}

		public static void WarningFormat(Object context, string format, params object objects)
		{
			logger.LogWarning(kTAG, string.Format(format, objects), context);
		}
		#endregion

		#region Error
		public static void Error(object message)
		{
			logger.LogError(kTAG, message);
		}

		public static void Error(object message, Object context)
		{
			logger.LogError(kTAG, message, context);
		}

		public static void ErrorFormat(string format, params object objects)
		{
			logger.LogError(kTAG, string.Format(format, objects));
		}

		public static void ErrorFormat(Object context, string format, params object objects)
		{
			logger.LogError(kTAG, string.Format(format, objects), context);
		}
		#endregion

		#region Exception
		public static void Exception(object message)
		{
			logger.LogException(kTAG, message);
		}

		public static void Exception(object message, Object context)
		{
			logger.LogException(kTAG, message, context);
		}

		public static void ExceptionFormat(string format, params object objects)
		{
			logger.LogException(kTAG, string.Format(format, objects));
		}

		public static void ExceptionFormat(Object context, string format, params object objects)
		{
			logger.LogException(kTAG, string.Format(format, objects), context);
		}
		#endregion
	}
}
