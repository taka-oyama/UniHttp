using UnityEngine;

namespace UniHttp
{
	public class Logger
	{
		public enum Level : byte { Debug, Info, Warning, Error, Exception, None }

		public static Level LogLevel = Level.Info;
		public static ILogger logger = UnityEngine.Debug.logger;

		#region Debug
		public static void Debug(object message)
		{
			if(LogLevel <= Level.Debug) logger.Log(message);
		}

		public static void Debug(object message, Object context)
		{
			if(LogLevel <= Level.Debug) logger.Log(LogType.Log, message, context);
		}

		public static void DebugFormat(string format, params object[] objects)
		{
			if(LogLevel <= Level.Debug) logger.Log(LogType.Log, string.Format(format, objects));
		}

		public static void DebugFormat(Object context, string format, params object[] objects)
		{
			if(LogLevel <= Level.Debug) logger.Log(LogType.Log, (object)string.Format(format, objects), context);
		}
		#endregion

		#region Info
		public static void Info(object message)
		{
			if(LogLevel <= Level.Info) logger.Log(LogType.Log, message);
		}

		public static void Info(object message, Object context)
		{
			if(LogLevel <= Level.Info) logger.Log(LogType.Log, message, context);
		}

		public static void InfoFormat(string format, params object[] objects)
		{
			if(LogLevel <= Level.Info) logger.Log(LogType.Log, string.Format(format, objects));
		}

		public static void InfoFormat(Object context, string format, params object[] objects)
		{
			if(LogLevel <= Level.Info) logger.Log(LogType.Log, (object)string.Format(format, objects), context);
		}
		#endregion

		#region Warning
		public static void Warning(object message)
		{
			if(LogLevel <= Level.Warning) logger.Log(LogType.Warning, message);
		}

		public static void Warning(object message, Object context)
		{
			if(LogLevel <= Level.Warning) logger.Log(LogType.Warning, message, context);
		}

		public static void WarningFormat(string format, params object[] objects)
		{
			if(LogLevel <= Level.Warning) logger.Log(LogType.Warning, string.Format(format, objects));
		}

		public static void WarningFormat(Object context, string format, params object[] objects)
		{
			if(LogLevel <= Level.Warning) logger.Log(LogType.Warning, (object)string.Format(format, objects), context);
		}
		#endregion

		#region Error
		public static void Error(object message)
		{
			if(LogLevel <= Level.Error) logger.Log(LogType.Error, message);
		}

		public static void Error(object message, Object context)
		{
			if(LogLevel <= Level.Error) logger.Log(LogType.Error, message, context);
		}

		public static void ErrorFormat(string format, params object[] objects)
		{
			if(LogLevel <= Level.Error) logger.Log(LogType.Error, string.Format(format, objects));
		}

		public static void ErrorFormat(Object context, string format, params object[] objects)
		{
			if(LogLevel <= Level.Error) logger.Log(LogType.Error, (object)string.Format(format, objects), context);
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
