using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace UniTLib.Editor.Setup
{
    public static class PackageInstaller
    {
        static readonly Queue<string> _installQueue = new();
        static AddRequest _currentRequest;
        static ListRequest _listRequest;

        public static bool IsBusy =>
            _currentRequest != null || _installQueue.Count > 0;

        // -------- Installed Check --------

        public static bool IsInstalled(string packageName)
        {
            _listRequest ??= Client.List(true);
            if (!_listRequest.IsCompleted) return false;

            return _listRequest.Result.Any(p => p.name == packageName);
        }

        // -------- Queue API --------

        public static void Enqueue(string gitUrl)
        {
            if (_installQueue.Contains(gitUrl))
                return;

            _installQueue.Enqueue(gitUrl);
            TryExecuteNext();
        }

        static void TryExecuteNext()
        {
            if (_currentRequest != null) return;
            if (_installQueue.Count == 0) return;

            var url = _installQueue.Dequeue();
            _currentRequest = Client.Add(url);
            EditorApplication.update += Monitor;
        }

        static void Monitor()
        {
            if (_currentRequest == null || !_currentRequest.IsCompleted)
                return;

            if (_currentRequest.Status == StatusCode.Success)
            {
                UnityEngine.Debug.Log(
                    $"[UniTLib] Installed: {_currentRequest.Result.name}"
                );
            }
            else
            {
                UnityEngine.Debug.LogError(
                    $"[UniTLib] Install failed: {_currentRequest.Error.message}"
                );
            }

            EditorApplication.update -= Monitor;
            _currentRequest = null;
            _listRequest = null; // Installed 再チェック用

            TryExecuteNext(); // 次へ
        }
    }
}
