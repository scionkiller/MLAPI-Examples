﻿using UnityEngine;

namespace MLAPI.Logging
{
    public enum LogLevel
    {
        Developer,
        Normal,
        Error,
        Nothing
    }

    public static class LogHelper
    {
        public static LogLevel CurrentLogLevel
        {
            get
            {
                if (NetworkingManager.GetSingleton() == null)
                    return LogLevel.Normal;
                else
                    return NetworkingManager.GetSingleton().LogLevel;
            }
        }

        public static void LogInfo(string message) => Debug.Log("[MLAPI] " + message);
        public static void LogWarning(string message) => Debug.LogWarning("[MLAPI] " + message);
        public static void LogError(string message) => Debug.LogError("[MLAPI] " + message);
    }
}
