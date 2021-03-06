﻿using System;

namespace Sandbox
{
    public class Lumberjack
    {
        public static OutputLevel TraceLevel { get; set; } = OutputLevel.Info;

        public static void Debug(string message)
        {
            WriteLine(message, ConsoleColor.Gray, OutputLevel.Debug, "DEBUG");
        }

        public static void Error(string message)
        {
            WriteLine(message, ConsoleColor.Red, OutputLevel.Error, "ERROR");
        }

        public static void Info(string message)
        {
            WriteLine(message, ConsoleColor.Green, OutputLevel.Info, "INFO");
        }

        public static void Log(string message)
        {
            WriteLine(message, ConsoleColor.Gray, OutputLevel.Log, "LOG");
        }

        public static void Warn(string message)
        {
            WriteLine(message, ConsoleColor.Yellow, OutputLevel.Warn, "WARN");
        }

        public static void WriteLine(string message, ConsoleColor color, OutputLevel level, string header)
        {
            if (TraceLevel > level)
                return;

            if (Console.ForegroundColor != color)
                Console.ForegroundColor = color;
            Console.WriteLine(Resources.Log_Format, DateTime.Now, header, message);
        }
    }

    public enum OutputLevel
    {
        Debug,
        Log,
        Info,
        Warn,
        Error,
        Off
    }
}