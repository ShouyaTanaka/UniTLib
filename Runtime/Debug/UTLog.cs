using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;

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
        private static bool isInitialized = false;
        private static bool captureUnityLogs = false;

        public static event Action<LogEntry> OnLogAdded
        {
            add => onLogAdded += value;
            remove => onLogAdded -= value;
        }

        /// <summary>
        /// Unity標準ログのキャプチャを有効にする
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            if (!isInitialized)
            {
                captureUnityLogs = true;
                Application.logMessageReceived += OnUnityLogMessageReceived;
                isInitialized = true;
            }
        }

        /// <summary>
        /// Unity標準ログのキャプチャを無効にする
        /// </summary>
        public static void DisableUnityLogCapture()
        {
            captureUnityLogs = false;
        }

        /// <summary>
        /// Unity標準ログのキャプチャを有効にする
        /// </summary>
        public static void EnableUnityLogCapture()
        {
            captureUnityLogs = true;
        }

        public static List<LogEntry> GetAllLogs() => logEntries;

        public static void Clear()
        {
            logEntries.Clear();
        }

        private static void OnUnityLogMessageReceived(string message, string stackTrace, UnityEngine.LogType type)
        {
            if (!captureUnityLogs) return;

            // UTLogから出力されたログは二重登録を避ける
            if (message.StartsWith("[") && message.Contains("]"))
            {
                return;
            }

            // Unity LogTypeをUTLog LogTypeに変換
            LogType utLogType;
            switch (type)
            {
                case UnityEngine.LogType.Error:
                case UnityEngine.LogType.Exception:
                case UnityEngine.LogType.Assert:
                    utLogType = LogType.Error;
                    break;
                case UnityEngine.LogType.Warning:
                    utLogType = LogType.Warning;
                    break;
                default:
                    utLogType = LogType.Log;
                    break;
            }

            // スタックトレースからファイルパス、行番号、クラス名を抽出
            string filePath = "";
            int lineNumber = 0;
            string className = "";
            ParseStackTrace(stackTrace, out filePath, out lineNumber, out className);

            // クラス名が取得できればタグに使用、なければ"Unity"
            string tag = !string.IsNullOrEmpty(className) ? className : "Unity";

            var entry = new LogEntry(message, utLogType, tag, filePath, lineNumber);
            entry.StackTrace = stackTrace;

            logEntries.Add(entry);
            onLogAdded?.Invoke(entry);
        }

        private static void ParseStackTrace(string stackTrace, out string filePath, out int lineNumber, out string className)
        {
            filePath = "";
            lineNumber = 0;
            className = "";

            if (string.IsNullOrEmpty(stackTrace)) return;

            // スタックトレースの最初の行を取得
            var lines = stackTrace.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return;

            string firstLine = lines[0];

            // クラス名を抽出（例: "at ClassName.MethodName ()" または "ClassName.MethodName ()"）
            var classMatch = Regex.Match(firstLine, @"(?:at\s+)?([\w.]+\.\w+)\s*\(");
            if (classMatch.Success)
            {
                string fullName = classMatch.Groups[1].Value;
                // 最後のドット前がクラス名（MethodNameを除く）
                int lastDotIndex = fullName.LastIndexOf('.');
                if (lastDotIndex > 0)
                {
                    className = fullName.Substring(0, lastDotIndex);
                    // ネームスペースがある場合は最後のクラス名だけを取得
                    int classNameDotIndex = className.LastIndexOf('.');
                    if (classNameDotIndex > 0)
                    {
                        className = className.Substring(classNameDotIndex + 1);
                    }
                }
            }

            // スタックトレースから最初のファイルパスと行番号を抽出
            // 例: "at ClassName.MethodName () [0x00001] in C:/Path/To/File.cs:123"
            var match = Regex.Match(stackTrace, @"\(at (.+?):(\d+)\)");
            if (!match.Success)
            {
                // 別のフォーマットを試す
                match = Regex.Match(stackTrace, @"in (.+?):(\d+)");
            }

            if (match.Success && match.Groups.Count >= 3)
            {
                filePath = match.Groups[1].Value;
                int.TryParse(match.Groups[2].Value, out lineNumber);
            }
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