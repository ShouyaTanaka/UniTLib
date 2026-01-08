using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace UniTLib.Debug.Editor
{
    public class UTConsoleWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private Vector2 detailScrollPosition;
        private bool showLog = true;
        private bool showWarning = true;
        private bool showError = true;
        private string selectedTag = "All";
        private HashSet<string> availableTags = new HashSet<string> { "All" };
        private LogEntry selectedLogEntry = null;
        private double lastClickTime = 0;
        private LogEntry lastClickedEntry = null;

        // アイコンのテクスチャ
        private Texture2D infoIcon;
        private Texture2D warningIcon;
        private Texture2D errorIcon;

        // カウンター
        private int logCount = 0;
        private int warningCount = 0;
        private int errorCount = 0;

        [MenuItem("Window/UT Console")]
        public static void ShowWindow()
        {
            var window = GetWindow<UTConsoleWindow>("UT Console");
            window.Show();
        }

        private void OnEnable()
        {
            UTLog.OnLogAdded += OnLogAdded;
            LoadIcons();
            UpdateCounts();
            UpdateAvailableTags();
        }

        private void OnDisable()
        {
            UTLog.OnLogAdded -= OnLogAdded;
        }

        private void LoadIcons()
        {
            // Unity標準のアイコンを使用
            infoIcon = EditorGUIUtility.IconContent("console.infoicon").image as Texture2D;
            warningIcon = EditorGUIUtility.IconContent("console.warnicon").image as Texture2D;
            errorIcon = EditorGUIUtility.IconContent("console.erroricon").image as Texture2D;
        }

        private void OnLogAdded(LogEntry entry)
        {
            UpdateCounts();
            UpdateAvailableTags();
            Repaint();
        }

        private void UpdateCounts()
        {
            var logs = UTLog.GetAllLogs();
            logCount = logs.Count(l => l.Type == LogType.Log);
            warningCount = logs.Count(l => l.Type == LogType.Warning);
            errorCount = logs.Count(l => l.Type == LogType.Error);
        }

        private void UpdateAvailableTags()
        {
            availableTags.Clear();
            availableTags.Add("All");
            foreach (var log in UTLog.GetAllLogs())
            {
                if (!string.IsNullOrEmpty(log.Tag))
                {
                    availableTags.Add(log.Tag);
                }
            }
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.BeginHorizontal();
            DrawLogList();
            DrawLogDetail();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Clearボタン
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                UTLog.Clear();
                selectedLogEntry = null;
                UpdateCounts();
                UpdateAvailableTags();
            }

            GUILayout.Space(10);

            // Log/Warning/Errorトグルボタン
            var logStyle = new GUIStyle(EditorStyles.toolbarButton);
            logStyle.fixedWidth = 80;

            GUI.color = showLog ? Color.white : new Color(0.7f, 0.7f, 0.7f);
            if (GUILayout.Button(new GUIContent($" {logCount}", infoIcon), logStyle))
            {
                showLog = !showLog;
            }

            GUI.color = showWarning ? Color.white : new Color(0.7f, 0.7f, 0.7f);
            if (GUILayout.Button(new GUIContent($" {warningCount}", warningIcon), logStyle))
            {
                showWarning = !showWarning;
            }

            GUI.color = showError ? Color.white : new Color(0.7f, 0.7f, 0.7f);
            if (GUILayout.Button(new GUIContent($" {errorCount}", errorIcon), logStyle))
            {
                showError = !showError;
            }

            GUI.color = Color.white;

            GUILayout.Space(10);

            // Tagフィルター
            GUILayout.Label("Tag Filter:", EditorStyles.toolbarButton, GUILayout.Width(70));

            var tagArray = availableTags.ToArray();
            var currentIndex = System.Array.IndexOf(tagArray, selectedTag);
            if (currentIndex == -1) currentIndex = 0;

            var newIndex = EditorGUILayout.Popup(currentIndex, tagArray, EditorStyles.toolbarPopup, GUILayout.Width(100));
            if (newIndex >= 0 && newIndex < tagArray.Length)
            {
                selectedTag = tagArray[newIndex];
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawLogList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.65f));
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            var logs = UTLog.GetAllLogs();
            var filteredLogs = logs.Where(log =>
            {
                bool typeMatch = (log.Type == LogType.Log && showLog) ||
                                (log.Type == LogType.Warning && showWarning) ||
                                (log.Type == LogType.Error && showError);

                bool tagMatch = selectedTag == "All" || log.Tag == selectedTag;

                return typeMatch && tagMatch;
            }).ToList();

            foreach (var log in filteredLogs)
            {
                DrawLogEntry(log);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawLogEntry(LogEntry log)
        {
            var backgroundColor = selectedLogEntry == log ? new Color(0.3f, 0.5f, 0.8f) : Color.clear;

            if (log.Type == LogType.Error)
                backgroundColor = selectedLogEntry == log ? new Color(0.8f, 0.3f, 0.3f) : new Color(0.5f, 0.2f, 0.2f, 0.3f);
            else if (log.Type == LogType.Warning)
                backgroundColor = selectedLogEntry == log ? new Color(0.8f, 0.6f, 0.3f) : new Color(0.5f, 0.4f, 0.2f, 0.3f);

            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;

            EditorGUILayout.BeginHorizontal("box");
            GUI.backgroundColor = originalColor;

            // アイコン
            Texture2D icon = log.Type == LogType.Log ? infoIcon :
                            log.Type == LogType.Warning ? warningIcon : errorIcon;
            GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));

            // 時間
            GUILayout.Label(log.Time, GUILayout.Width(60));

            // Tag
            var tagStyle = new GUIStyle(EditorStyles.label);
            tagStyle.normal.textColor = new Color(0.5f, 0.8f, 1f);
            tagStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label($"[{log.Tag}]", tagStyle, GUILayout.Width(80));

            // メッセージ
            GUILayout.Label(log.Message, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            var rect = GUILayoutUtility.GetLastRect();

            // クリック・ダブルクリック判定
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                double currentTime = EditorApplication.timeSinceStartup;

                // ダブルクリック判定（0.3秒以内）
                if (lastClickedEntry == log && (currentTime - lastClickTime) < 0.3)
                {
                    OpenScriptAtLine(log);
                }
                else
                {
                    // シングルクリック
                    selectedLogEntry = log;
                    lastClickTime = currentTime;
                    lastClickedEntry = log;
                }

                Event.current.Use();
                Repaint();
            }
        }

        private void OpenScriptAtLine(LogEntry log)
        {
            if (string.IsNullOrEmpty(log.FilePath) || log.LineNumber <= 0)
            {
                UnityEngine.Debug.LogWarning("No file path or line number available for this log entry.");
                return;
            }

            string relativePath = log.FilePath;

            // 絶対パスを相対パスに変換
            if (Path.IsPathRooted(relativePath))
            {
                string dataPath = Application.dataPath;

                // Windowsのパス区切り文字を統一
                relativePath = relativePath.Replace("\\", "/");
                dataPath = dataPath.Replace("\\", "/");

                if (relativePath.StartsWith(dataPath))
                {
                    // "Assets"フォルダ以下のパスに変換
                    relativePath = "Assets" + relativePath.Substring(dataPath.Length);
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"File is outside the Assets folder: {relativePath}");
                    return;
                }
            }

            // デバッグ情報を出力
            UnityEngine.Debug.Log($"Attempting to open:  {relativePath} at line {log.LineNumber}");

            // アセットを読み込んで開く
            var script = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(relativePath);
            if (script != null)
            {
                AssetDatabase.OpenAsset(script, log.LineNumber);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Could not load asset at path: {relativePath}");

                // ファイル名だけで検索を試みる
                string fileName = Path.GetFileName(relativePath);
                string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(fileName));

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (assetPath.EndsWith(fileName))
                    {
                        UnityEngine.Debug.Log($"Found file by name: {assetPath}");
                        var foundScript = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                        if (foundScript != null)
                        {
                            AssetDatabase.OpenAsset(foundScript, log.LineNumber);
                            return;
                        }
                    }
                }

                UnityEngine.Debug.LogError($"Could not find or open file: {fileName}");
            }
        }

        private void DrawLogDetail()
        {
            var detailWidth = position.width * 0.35f - 10;
            EditorGUILayout.BeginVertical(GUILayout.Width(detailWidth), GUILayout.MaxWidth(detailWidth));

            if (selectedLogEntry != null)
            {
                // ヘッダー
                var headerStyle = new GUIStyle(EditorStyles.boldLabel);
                headerStyle.fontSize = 14;
                headerStyle.padding = new RectOffset(10, 10, 10, 10);
                EditorGUILayout.LabelField("Details", headerStyle);

                GUILayout.Space(5);

                // 情報セクション（背景付き）
                var infoBoxStyle = new GUIStyle(EditorStyles.helpBox);
                infoBoxStyle.padding = new RectOffset(10, 10, 10, 10);

                EditorGUILayout.BeginVertical(infoBoxStyle, GUILayout.MaxWidth(detailWidth - 5));

                // Type（アイコン付き）
                EditorGUILayout.BeginHorizontal();
                Texture2D icon = selectedLogEntry.Type == LogType.Log ? infoIcon :
                                selectedLogEntry.Type == LogType.Warning ? warningIcon : errorIcon;
                GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));

                var typeStyle = new GUIStyle(EditorStyles.boldLabel);
                typeStyle.fontSize = 12;
                Color typeColor = selectedLogEntry.Type == LogType.Log ? Color.white :
                                 selectedLogEntry.Type == LogType.Warning ? new Color(1f, 0.8f, 0.3f) :
                                 new Color(1f, 0.4f, 0.4f);
                typeStyle.normal.textColor = typeColor;
                EditorGUILayout.LabelField(selectedLogEntry.Type.ToString(), typeStyle);
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(8);

                // Time
                EditorGUILayout.BeginHorizontal();
                var labelStyle = new GUIStyle(EditorStyles.label);
                labelStyle.fontStyle = FontStyle.Bold;
                labelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
                EditorGUILayout.LabelField("Time:", labelStyle, GUILayout.Width(50));
                EditorGUILayout.LabelField(selectedLogEntry.Time);
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);

                // Tag
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Tag:", labelStyle, GUILayout.Width(50));
                var tagValueStyle = new GUIStyle(EditorStyles.label);
                tagValueStyle.normal.textColor = new Color(0.5f, 0.8f, 1f);
                tagValueStyle.fontStyle = FontStyle.Bold;
                EditorGUILayout.LabelField(selectedLogEntry.Tag, tagValueStyle);
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);

                // File and Line（クリック可能にする）
                if (!string.IsNullOrEmpty(selectedLogEntry.FilePath))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("File:", labelStyle, GUILayout.Width(50));

                    var fileName = Path.GetFileName(selectedLogEntry.FilePath);
                    var fileButtonStyle = new GUIStyle(EditorStyles.label);
                    fileButtonStyle.normal.textColor = new Color(0.4f, 0.7f, 1f);
                    fileButtonStyle.hover.textColor = new Color(0.6f, 0.85f, 1f);

                    if (GUILayout.Button($"{fileName}:{selectedLogEntry.LineNumber}", fileButtonStyle))
                    {
                        OpenScriptAtLine(selectedLogEntry);
                    }

                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();

                GUILayout.Space(10);

                // Message (選択可能・コピー可能・編集不可)
                var sectionStyle = new GUIStyle(EditorStyles.boldLabel);
                sectionStyle.fontSize = 11;
                sectionStyle.padding = new RectOffset(5, 5, 5, 5);
                EditorGUILayout.LabelField("Message", sectionStyle);

                var messageStyle = new GUIStyle(EditorStyles.textArea);
                messageStyle.wordWrap = true;
                messageStyle.padding = new RectOffset(5, 5, 5, 5);

                EditorGUILayout.SelectableLabel(selectedLogEntry.Message, messageStyle,
                    GUILayout.Height(80), GUILayout.MaxWidth(detailWidth - 5));

                GUILayout.Space(10);

                // Stack Trace (選択可能・コピー可能・編集不可)
                EditorGUILayout.LabelField("Stack Trace", sectionStyle);

                detailScrollPosition = EditorGUILayout.BeginScrollView(detailScrollPosition,
                    GUILayout.ExpandHeight(true), GUILayout.MaxWidth(detailWidth - 5));
                var stackTraceStyle = new GUIStyle(EditorStyles.textArea);
                stackTraceStyle.wordWrap = true;
                stackTraceStyle.fontSize = 9;
                stackTraceStyle.padding = new RectOffset(5, 5, 5, 5);

                EditorGUILayout.SelectableLabel(selectedLogEntry.StackTrace, stackTraceStyle,
                    GUILayout.ExpandHeight(true), GUILayout.MaxWidth(detailWidth - 10));
                EditorGUILayout.EndScrollView();
            }
            else
            {
                // 何も選択されていない時
                GUILayout.FlexibleSpace();
                var hintStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                hintStyle.fontSize = 12;
                hintStyle.wordWrap = true;
                hintStyle.alignment = TextAnchor.MiddleCenter;
                EditorGUILayout.LabelField("Select a log entry\nto view details\n\nDouble-click to open file", hintStyle);
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.EndVertical();
        }
    }
}