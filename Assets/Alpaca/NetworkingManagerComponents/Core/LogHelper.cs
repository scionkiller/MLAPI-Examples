using UnityEngine;

namespace Alpaca
{
    public enum LogLevel : int
    {
          Info
		, Warning
		, Error
		, Nothing
    }

    public static class Log
    {
		static readonly string ALPACA_PREFIX = "[Alpaca] ";

		public static LogLevel s_logLevel = LogLevel.Warning;

        public static void LogInfo( string message )
		{
			if( s_logLevel <= LogLevel.Info ) { Debug.Log( ALPACA_PREFIX + message); }
		}

        public static void LogWarning( string message )
		{
			if( s_logLevel <= LogLevel.Warning ) { Debug.LogWarning( ALPACA_PREFIX + message); }
		}

        public static void LogError(string message)
		{
			if( s_logLevel <= LogLevel.Error ) { Debug.LogError( ALPACA_PREFIX + message ); }
		}
    }
}