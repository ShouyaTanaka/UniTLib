using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UniTLib.Debug
{
    public enum LogType
    {
        Log,
        Warning,
        Error
    }

    public class LogEntry
    {
        public string Message { get; set; }
        public LogType Type { get; set; }
        public string Tag { get; set; }
        public string Time { get; set; }
        public string StackTrace { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }

        public LogEntry(string message, LogType type, string tag, string filePath, int lineNumber)
        {
            Message = message;
            Type = type;
            Tag = tag;
            Time = DateTime.Now.ToString("HH:mm:ss");
            FilePath = filePath;
            LineNumber = lineNumber;
            StackTrace = Environment.StackTrace;
        }
    }

    public class UTLog
    {
        private static List<LogEntry> logEntries = new List<LogEntry>();
        private static Action<LogEntry> onLogAdded;

        public static event Action<LogEntry> OnLogAdded
        {
            add => onLogAdded += value;
            remove => onLogAdded -= value;
        }

        public static List<LogEntry> GetAllLogs() => logEntries;

        public static void Clear()
        {
            logEntries.Clear();
        }

        public static LogBuilder Log(string message,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            return new LogBuilder(message, LogType.Log, filePath, lineNumber);
        }

        public static LogBuilder Warning(string message,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            return new LogBuilder(message, LogType.Warning, filePath, lineNumber);
        }

        public static LogBuilder Error(string message,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            return new LogBuilder(message, LogType.Error, filePath, lineNumber);
        }

        internal static void AddLog(LogEntry entry)
        {
            logEntries.Add(entry);
            onLogAdded?.Invoke(entry);

            // Unity標準コンソールにも出力
            switch (entry.Type)
            {
                case LogType.Log:
                    UnityEngine.Debug.Log($"[{entry.Tag}] {entry.Message}");
                    break;
                case LogType.Warning:
                    UnityEngine.Debug.LogWarning($"[{entry.Tag}] {entry.Message}");
                    break;
                case LogType.Error:
                    UnityEngine.Debug.LogError($"[{entry.Tag}] {entry.Message}");
                    break;
            }
        }

        internal static void UpdateLastLogTag(string tag)
        {
            if (logEntries.Count > 0)
            {
                logEntries[logEntries.Count - 1].Tag = tag;
            }
        }
    }

    public class LogBuilder
    {
        private string message;
        private LogType type;
        private string tag = "Default";
        private string filePath;
        private int lineNumber;
        private LogEntry createdEntry;

        public LogBuilder(string message, LogType type, string filePath, int lineNumber)
        {
            this.message = message;
            this.type = type;
            this.filePath = filePath;
            this.lineNumber = lineNumber;

            // すぐにログを記録
            createdEntry = new LogEntry(message, type, tag, filePath, lineNumber);
            UTLog.AddLog(createdEntry);
        }

        public LogBuilder Tag(string tag)
        {
            this.tag = tag;
            // 既に記録されたログのTagを更新
            if (createdEntry != null)
            {
                createdEntry.Tag = tag;
            }
            return this;
        }
    }
}