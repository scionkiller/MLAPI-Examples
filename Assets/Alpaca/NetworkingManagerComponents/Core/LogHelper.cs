using System.Runtime.CompilerServices;

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

        public static void Info( string message )
		{
			if( s_logLevel <= LogLevel.Info ) { Debug.Log( PrefixMessage( message ) ); }
		}

        public static void Warn( string message )
		{
			if( s_logLevel <= LogLevel.Warning ) { Debug.LogWarning( PrefixMessage( message ) ); }
		}

        public static void Error( string message )
		{
			if( s_logLevel <= LogLevel.Error ) { Debug.LogError( PrefixMessage( message ) ); }
		}

		// TODO: Make this more performant with a string builder? Not sure if worth the bother
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string PrefixMessage( string message ) { return ALPACA_PREFIX + message; }
    }
}