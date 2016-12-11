using UnityEngine;

namespace UniHttp
{
	public class Logger
	{
		public enum Level : byte { Debug, Info, Warning, Error, Exception, None }

		public static Level LogLevel = Level.Info;
		public static ILogger logger = UnityEngine.Debug.logger;
		public static string kTAG = "";

		#region Debug
		public static void Debug(object message)
		{
			if(LogLevel <= Level.Debug) logger.Log(kTAG, message);
		}

		public static void Debug(object message, Object context)
		{
			if(LogLevel <= Level.Debug) logger.Log(kTAG, message, context);
		}

		public static void DebugFormat(string format, params object[] objects)
		{
			if(LogLevel <= Level.Debug) logger.Log(kTAG, string.Format(format, objects));
		}

		public static void DebugFormat(Object context, string format, params object[] objects)
		{
			if(LogLevel <= Level.Debug) logger.Log(kTAG, string.Format(format, objects), context);
		}
		#endregion

		#region Info
		public static void Info(object message)
		{
			if(LogLevel <= Level.Info) logger.Log(kTAG, message);
		}

		public static void Info(object message, Object context)
		{
			if(LogLevel <= Level.Info) logger.Log(kTAG, message, context);
		}

		public static void InfoFormat(string format, params object[] objects)
		{
			if(LogLevel <= Level.Info) logger.Log(kTAG, string.Format(format, objects));
		}

		public static void InfoFormat(Object context, string format, params object[] objects)
		{
			if(LogLevel <= Level.Info) logger.Log(kTAG, string.Format(format, objects), context);
		}
		#endregion

		#region Warning
		public static void Warning(object message)
		{
			if(LogLevel <= Level.Warning) logger.LogWarning(kTAG, message);
		}

		public static void Warning(object message, Object context)
		{
			if(LogLevel <= Level.Warning) logger.LogWarning(kTAG, message, context);
		}

		public static void WarningFormat(string format, params object[] objects)
		{
			if(LogLevel <= Level.Warning) logger.LogWarning(kTAG, string.Format(format, objects));
		}

		public static void WarningFormat(Object context, string format, params object[] objects)
		{
			if(LogLevel <= Level.Warning) logger.LogWarning(kTAG, string.Format(format, objects), context);
		}
		#endregion

		#region Error
		public static void Error(object message)
		{
			if(LogLevel <= Level.Error) logger.LogError(kTAG, message);
		}

		public static void Error(object message, Object context)
		{
			if(LogLevel <= Level.Error) logger.LogError(kTAG, message, context);
		}

		public static void ErrorFormat(string format, params object[] objects)
		{
			if(LogLevel <= Level.Error) logger.LogError(kTAG, string.Format(format, objects));
		}

		public static void ErrorFormat(Object context, string format, params object[] objects)
		{
			if(LogLevel <= Level.Error) logger.LogError(kTAG, string.Format(format, objects), context);
		}
		#endregion

		#region Exception
		public static void Exception(System.Exception exception)
		{
			if(LogLevel <= Level.Exception) logger.LogException(exception);
		}

		public static void Exception(System.Exception exception, Object context)
		{
			if(LogLevel <= Level.Exception) logger.LogException(exception, context);
		}
		#endregion
	}
}
