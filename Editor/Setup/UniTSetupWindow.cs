using UnityEditor;
using UnityEngine;

namespace UniTLib.Editor.Setup
{
    public class UniTSetupWindow : EditorWindow
    {
        bool _useUniTask;
        bool _useUniRx;
        bool _createProjectFolders = true;

        [MenuItem("Tools/UniTLib/Setup")]
        static void Open()
        {
            GetWindow<UniTSetupWindow>("UniTLib Setup");
        }

        void OnEnable()
        {
            // 初回表示時に既存パッケージのインストール状態を反映
            _useUniTask = !PackageInstaller.IsInstalled(PackageInfoConst.UniTaskName);
            _useUniRx = !PackageInstaller.IsInstalled(PackageInfoConst.UniRxName);
        }

        void OnGUI()
        {
            GUILayout.Label("UniTLib Setup", EditorStyles.boldLabel);
            GUILayout.Space(8);

            DrawPackage(
                "UniTask",
                PackageInfoConst.UniTaskName,
                ref _useUniTask
            );

            DrawPackage(
                "UniRx",
                PackageInfoConst.UniRxName,
                ref _useUniRx
            );

            GUILayout.Space(12);

            DrawFolderCreation();

            GUILayout.Space(12);

            using (new EditorGUI.DisabledScope(PackageInstaller.IsBusy))
            {
                if (GUILayout.Button("Install / Reinstall Selected", GUILayout.Height(28)))
                {
                    ExecuteInstall();
                }
            }

            DrawStatus();
        }

        void DrawPackage(string label, string packageName, ref bool toggle)
        {
            bool installed = PackageInstaller.IsInstalled(packageName);

            using (new EditorGUILayout.HorizontalScope())
            {
                toggle = EditorGUILayout.Toggle(toggle, GUILayout.Width(18));
                GUILayout.Label(label, GUILayout.Width(80));

                GUILayout.FlexibleSpace();
                GUILayout.Label(
                    installed ? "Installed" : "Not Installed",
                    installed ? EditorStyles.miniBoldLabel : EditorStyles.miniLabel
                );
            }
        }

        void ExecuteInstall()
        {
            if (_useUniTask)
                PackageInstaller.Enqueue(PackageInfoConst.UniTaskGit);

            if (_useUniRx)
                PackageInstaller.Enqueue(PackageInfoConst.UniRxGit);

            if (_createProjectFolders)
                PackageInstaller.CreateProjectFolders();
        }

        void DrawFolderCreation()
        {
            GUILayout.Label("Project Structure", EditorStyles.boldLabel);
            GUILayout.Space(4);

            _createProjectFolders = EditorGUILayout.ToggleLeft(
                "Create project folders",
                _createProjectFolders
            );
        }

        void DrawStatus()
        {
            if (!PackageInstaller.IsBusy) return;

            GUILayout.Space(16);
            GUILayout.Label(
                "Installing packages... Please wait.",
                EditorStyles.helpBox
            );

            Repaint(); // 状態更新用
        }
    }
}
