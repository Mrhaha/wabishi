using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using DiagnosticsProcess = System.Diagnostics.Process;
using DiagnosticsProcessPriorityClass = System.Diagnostics.ProcessPriorityClass;
using DiagnosticsProcessStartInfo = System.Diagnostics.ProcessStartInfo;
using DiagnosticsProcessWindowStyle = System.Diagnostics.ProcessWindowStyle;

public sealed class WabishPlaytestAnalysisWindow : EditorWindow
{
    private const string SeedCountEditorKey = "Wabish.PlaytestAnalysis.SeedCount";
    private const string BaseSeedEditorKey = "Wabish.PlaytestAnalysis.BaseSeed";
    private const string CloseAfterStartEditorKey = "Wabish.PlaytestAnalysis.CloseAfterStart";

    private int seedCount;
    private int baseSeed;
    private bool closeAfterStart;

    [MenuItem("Tools/Dice King/后台试玩分析")]
    public static void ShowWindow()
    {
        WabishPlaytestAnalysisWindow window = GetWindow<WabishPlaytestAnalysisWindow>(true, "后台试玩分析", true);
        window.minSize = new Vector2(500f, 310f);
        window.Show();
    }

    [MenuItem("Tools/Dice King/打开最近一次试玩分析日志")]
    public static void OpenLatestFromMenu()
    {
        WabishPlaytestBackgroundLauncher.OpenLatestSummary();
    }

    private void OnEnable()
    {
        seedCount = Mathf.Clamp(EditorPrefs.GetInt(SeedCountEditorKey, 50), 1, 200);
        int defaultSeed;
        if (!int.TryParse(DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture), out defaultSeed))
        {
            defaultSeed = 20260828;
        }
        baseSeed = EditorPrefs.GetInt(BaseSeedEditorKey, defaultSeed);
        closeAfterStart = EditorPrefs.GetBool(CloseAfterStartEditorKey, true);
        EditorApplication.update -= RepaintWhileRunning;
        EditorApplication.update += RepaintWhileRunning;
    }

    private void OnDisable()
    {
        EditorPrefs.SetInt(SeedCountEditorKey, seedCount);
        EditorPrefs.SetInt(BaseSeedEditorKey, baseSeed);
        EditorPrefs.SetBool(CloseAfterStartEditorKey, closeAfterStart);
        EditorApplication.update -= RepaintWhileRunning;
    }

    private void RepaintWhileRunning()
    {
        if (WabishPlaytestBackgroundLauncher.IsRunning)
        {
            Repaint();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("骰子王后台试玩分析", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "测试在系统临时目录的隔离 Unity 工程中以 batchmode + nographics 运行。不会切换当前场景、进入当前编辑器的 Play Mode，也不会弹出游戏或控制台窗口。系统任务管理器仍可看到 Unity 进程。",
            MessageType.Info);

        using (new EditorGUI.DisabledScope(WabishPlaytestBackgroundLauncher.IsRunning))
        {
            seedCount = EditorGUILayout.IntSlider("固定种子数量", seedCount, 1, 200);
            baseSeed = EditorGUILayout.IntField("起始种子", baseSeed);
            closeAfterStart = EditorGUILayout.ToggleLeft("启动后关闭本窗口", closeAfterStart);

            EditorGUILayout.Space(8f);
            if (GUILayout.Button("启动后台试玩（5 种策略）", GUILayout.Height(38f)))
            {
                string outputPrefix;
                if (WabishPlaytestBackgroundLauncher.Start(seedCount, baseSeed, out outputPrefix) && closeAfterStart)
                {
                    Close();
                }
            }
        }

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("状态", EditorStyles.boldLabel);
        EditorGUILayout.SelectableLabel(
            WabishPlaytestBackgroundLauncher.StatusText,
            EditorStyles.textArea,
            GUILayout.Height(46f));

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(!WabishPlaytestBackgroundLauncher.HasLatestSummary))
            {
                if (GUILayout.Button("打开本次 / 最近分析日志", GUILayout.Height(30f)))
                {
                    WabishPlaytestBackgroundLauncher.OpenLatestSummary();
                }
            }

            if (GUILayout.Button("打开日志目录", GUILayout.Height(30f)))
            {
                WabishPlaytestBackgroundLauncher.RevealOutputDirectory();
            }
        }

        using (new EditorGUI.DisabledScope(!WabishPlaytestBackgroundLauncher.HasLatestUnityLog))
        {
            if (GUILayout.Button("打开最近一次后台 Unity 日志", GUILayout.Height(25f)))
            {
                WabishPlaytestBackgroundLauncher.OpenLatestUnityLog();
            }
        }
    }
}

[InitializeOnLoad]
internal static class WabishPlaytestBackgroundLauncher
{
    private const string LastSummaryEditorKey = "Wabish.PlaytestAnalysis.LastSummary";
    private const string LastUnityLogEditorKey = "Wabish.PlaytestAnalysis.LastUnityLog";
    private const string ActivePrefixEditorKey = "Wabish.PlaytestAnalysis.ActivePrefix";
    private const string ActiveProcessIdEditorKey = "Wabish.PlaytestAnalysis.ActiveProcessId";
    private const string OutputFolderRelativePath = "Docs/QA/Playtests";
    private const string WorkerFolderName = "WabishDiceKingPlaytestWorker";

    private static DiagnosticsProcess workerProcess;
    private static string activeOutputPrefix;
    private static string statusText = "尚未启动后台试玩。";

    static WabishPlaytestBackgroundLauncher()
    {
        RecoverBackgroundProcess();
    }

    public static bool IsRunning
    {
        get
        {
            if (workerProcess == null)
            {
                return false;
            }

            try
            {
                return !workerProcess.HasExited;
            }
            catch
            {
                return false;
            }
        }
    }

    public static string StatusText
    {
        get
        {
            RefreshStatusFromDisk();
            return statusText;
        }
    }

    public static bool HasLatestSummary
    {
        get { return File.Exists(LatestSummaryPath()); }
    }

    public static bool HasLatestUnityLog
    {
        get { return File.Exists(EditorPrefs.GetString(LastUnityLogEditorKey, string.Empty)); }
    }

    public static bool Start(int seedCount, int baseSeed, out string outputPrefix)
    {
        outputPrefix = string.Empty;
        if (IsRunning)
        {
            statusText = "已有后台试玩正在运行。";
            return false;
        }

        try
        {
            string projectRoot = ProjectRoot();
            string workerRoot = WorkerRoot();
            PrepareWorkerProject(projectRoot, workerRoot);

            outputPrefix = ReserveOutputPrefix(projectRoot);
            string unityLogPath = outputPrefix + "_unity.log";
            string launcherStatusPath = outputPrefix + "_launcher_status.txt";
            File.WriteAllText(
                launcherStatusPath,
                "状态：正在准备后台 Unity。\n启动时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "\n",
                new System.Text.UTF8Encoding(false));

            string arguments =
                "-batchmode -nographics" +
                " -projectPath " + Quote(workerRoot) +
                " -executeMethod WabishHeadlessPlaytestRunner.RunFromCommandLine" +
                " -logFile " + Quote(unityLogPath) +
                " -wabishPlaytestOutputPrefix " + Quote(outputPrefix) +
                " -wabishPlaytestSeedCount " + Mathf.Clamp(seedCount, 1, 200).ToString(CultureInfo.InvariantCulture) +
                " -wabishPlaytestBaseSeed " + baseSeed.ToString(CultureInfo.InvariantCulture);

            DiagnosticsProcessStartInfo startInfo = new DiagnosticsProcessStartInfo
            {
                FileName = EditorApplication.applicationPath,
                Arguments = arguments,
                WorkingDirectory = workerRoot,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = DiagnosticsProcessWindowStyle.Hidden
            };

            workerProcess = DiagnosticsProcess.Start(startInfo);
            if (workerProcess == null)
            {
                throw new InvalidOperationException("无法启动后台 Unity 进程。");
            }

            try
            {
                workerProcess.PriorityClass = DiagnosticsProcessPriorityClass.BelowNormal;
            }
            catch
            {
                // Some Windows policies do not allow changing another process priority.
            }

            activeOutputPrefix = outputPrefix;
            EditorPrefs.SetString(ActivePrefixEditorKey, activeOutputPrefix);
            EditorPrefs.SetInt(ActiveProcessIdEditorKey, workerProcess.Id);
            EditorPrefs.SetString(LastUnityLogEditorKey, unityLogPath);
            statusText = "后台试玩已启动。固定种子 " + seedCount + " 个 × 5 种策略。";
            RegisterPolling();
            return true;
        }
        catch (Exception exception)
        {
            statusText = "启动失败：" + exception.Message;
            Debug.LogError("Wabish 后台试玩启动失败：" + exception);
            return false;
        }
    }

    public static void OpenLatestSummary()
    {
        string path = LatestSummaryPath();
        if (!File.Exists(path))
        {
            path = FindLatestFile("*_playtest_summary.md");
        }

        if (File.Exists(path))
        {
            EditorPrefs.SetString(LastSummaryEditorKey, path);
            InternalEditorUtility.OpenFileAtLineExternal(path, 1);
        }
    }

    public static void OpenLatestUnityLog()
    {
        string path = EditorPrefs.GetString(LastUnityLogEditorKey, string.Empty);
        if (File.Exists(path))
        {
            InternalEditorUtility.OpenFileAtLineExternal(path, 1);
        }
    }

    public static void RevealOutputDirectory()
    {
        string root = Path.Combine(ProjectRoot(), OutputFolderRelativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(root);
        EditorUtility.RevealInFinder(root);
    }

    private static void RecoverBackgroundProcess()
    {
        activeOutputPrefix = EditorPrefs.GetString(ActivePrefixEditorKey, string.Empty);
        int processId = EditorPrefs.GetInt(ActiveProcessIdEditorKey, -1);
        if (processId <= 0)
        {
            return;
        }

        try
        {
            workerProcess = DiagnosticsProcess.GetProcessById(processId);
            if (!workerProcess.HasExited)
            {
                statusText = "已恢复对后台试玩进程的监视。";
                RegisterPolling();
                return;
            }
        }
        catch
        {
            workerProcess = null;
        }

        CompletePollingState();
    }

    private static void RegisterPolling()
    {
        EditorApplication.update -= PollBackgroundProcess;
        EditorApplication.update += PollBackgroundProcess;
    }

    private static void PollBackgroundProcess()
    {
        if (IsRunning)
        {
            RefreshStatusFromDisk();
            return;
        }

        EditorApplication.update -= PollBackgroundProcess;
        CompletePollingState();
    }

    private static void CompletePollingState()
    {
        int exitCode = int.MinValue;
        if (workerProcess != null)
        {
            try
            {
                exitCode = workerProcess.ExitCode;
                workerProcess.Dispose();
            }
            catch
            {
                // The process can disappear between the status check and disposal.
            }
        }

        workerProcess = null;
        EditorPrefs.DeleteKey(ActiveProcessIdEditorKey);
        EditorPrefs.DeleteKey(ActivePrefixEditorKey);

        if (!string.IsNullOrEmpty(activeOutputPrefix))
        {
            string summaryPath = activeOutputPrefix + "_playtest_summary.md";
            if (File.Exists(summaryPath))
            {
                EditorPrefs.SetString(LastSummaryEditorKey, summaryPath);
                statusText = "后台试玩完成。可点击“打开本次 / 最近分析日志”。";
            }
            else
            {
                statusText = exitCode == int.MinValue
                    ? "后台试玩已结束，但未找到分析报告；请查看后台 Unity 日志。"
                    : "后台试玩退出码 " + exitCode + "，未找到分析报告；请查看后台 Unity 日志。";
            }
        }

        activeOutputPrefix = string.Empty;
    }

    private static void RefreshStatusFromDisk()
    {
        string prefix = !string.IsNullOrEmpty(activeOutputPrefix)
            ? activeOutputPrefix
            : EditorPrefs.GetString(ActivePrefixEditorKey, string.Empty);
        if (string.IsNullOrEmpty(prefix))
        {
            return;
        }

        string statusPath = prefix + "_status.txt";
        if (!File.Exists(statusPath))
        {
            statusPath = prefix + "_launcher_status.txt";
        }

        if (!File.Exists(statusPath))
        {
            return;
        }

        try
        {
            string text = File.ReadAllText(statusPath).Trim();
            if (!string.IsNullOrEmpty(text))
            {
                statusText = text;
            }
        }
        catch
        {
            // A background writer may hold the file for a fraction of a frame.
        }
    }

    private static string LatestSummaryPath()
    {
        string current = EditorPrefs.GetString(LastSummaryEditorKey, string.Empty);
        if (File.Exists(current))
        {
            return current;
        }

        current = FindLatestFile("*_playtest_summary.md");
        if (File.Exists(current))
        {
            EditorPrefs.SetString(LastSummaryEditorKey, current);
        }
        return current;
    }

    private static string FindLatestFile(string pattern)
    {
        string root = Path.Combine(ProjectRoot(), OutputFolderRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!Directory.Exists(root))
        {
            return string.Empty;
        }

        string[] files = Directory.GetFiles(root, pattern, SearchOption.AllDirectories);
        string latest = string.Empty;
        DateTime latestTime = DateTime.MinValue;
        for (int i = 0; i < files.Length; i++)
        {
            DateTime time = File.GetLastWriteTimeUtc(files[i]);
            if (time > latestTime)
            {
                latest = files[i];
                latestTime = time;
            }
        }
        return latest;
    }

    private static string ReserveOutputPrefix(string projectRoot)
    {
        DateTime now = DateTime.Now;
        string dayFolder = Path.Combine(
            projectRoot,
            OutputFolderRelativePath.Replace('/', Path.DirectorySeparatorChar),
            now.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
        Directory.CreateDirectory(dayFolder);

        string baseName = now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        string candidate = Path.Combine(dayFolder, baseName);
        int suffix = 2;
        while (HasOutputWithPrefix(candidate))
        {
            candidate = Path.Combine(dayFolder, baseName + "_" + suffix.ToString(CultureInfo.InvariantCulture));
            suffix++;
        }
        return candidate;
    }

    private static bool HasOutputWithPrefix(string prefix)
    {
        string directory = Path.GetDirectoryName(prefix);
        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
        {
            return false;
        }

        return Directory.GetFiles(directory, Path.GetFileName(prefix) + "_*", SearchOption.TopDirectoryOnly).Length > 0;
    }

    private static void PrepareWorkerProject(string projectRoot, string workerRoot)
    {
        Directory.CreateDirectory(workerRoot);
        SyncDirectory(Path.Combine(projectRoot, "Assets", "Scripts"), Path.Combine(workerRoot, "Assets", "Scripts"));
        SyncDirectory(Path.Combine(projectRoot, "Assets", "Resources"), Path.Combine(workerRoot, "Assets", "Resources"));
        SyncDirectory(Path.Combine(projectRoot, "Assets", "Scenes"), Path.Combine(workerRoot, "Assets", "Scenes"));
        SyncDirectory(Path.Combine(projectRoot, "Packages"), Path.Combine(workerRoot, "Packages"));
        SyncDirectory(Path.Combine(projectRoot, "ProjectSettings"), Path.Combine(workerRoot, "ProjectSettings"));

        string workerEditor = Path.Combine(workerRoot, "Assets", "Editor");
        SafeRecreateDirectory(workerRoot, workerEditor);
        CopyFileIfChanged(
            Path.Combine(projectRoot, "Assets", "Editor", "WabishHeadlessPlaytestRunner.cs"),
            Path.Combine(workerEditor, "WabishHeadlessPlaytestRunner.cs"));
        string runnerMeta = Path.Combine(projectRoot, "Assets", "Editor", "WabishHeadlessPlaytestRunner.cs.meta");
        if (File.Exists(runnerMeta))
        {
            CopyFileIfChanged(runnerMeta, Path.Combine(workerEditor, "WabishHeadlessPlaytestRunner.cs.meta"));
        }

        string projectVersionPath = Path.Combine(workerRoot, "ProjectSettings", "ProjectVersion.txt");
        if (!File.Exists(projectVersionPath))
        {
            File.WriteAllText(
                projectVersionPath,
                "m_EditorVersion: " + Application.unityVersion + Environment.NewLine,
                new System.Text.UTF8Encoding(false));
        }
    }

    private static void SyncDirectory(string sourceRoot, string targetRoot)
    {
        if (!Directory.Exists(sourceRoot))
        {
            throw new DirectoryNotFoundException("缺少后台试玩所需目录：" + sourceRoot);
        }

        Directory.CreateDirectory(targetRoot);
        HashSet<string> sourceRelativeFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string[] sourceFiles = Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories);
        for (int i = 0; i < sourceFiles.Length; i++)
        {
            string relative = RelativePath(sourceRoot, sourceFiles[i]);
            sourceRelativeFiles.Add(relative);
            CopyFileIfChanged(sourceFiles[i], Path.Combine(targetRoot, relative));
        }

        string[] targetFiles = Directory.GetFiles(targetRoot, "*", SearchOption.AllDirectories);
        for (int i = 0; i < targetFiles.Length; i++)
        {
            string relative = RelativePath(targetRoot, targetFiles[i]);
            if (!sourceRelativeFiles.Contains(relative))
            {
                File.Delete(targetFiles[i]);
            }
        }
    }

    private static void CopyFileIfChanged(string sourcePath, string targetPath)
    {
        string targetDirectory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        FileInfo source = new FileInfo(sourcePath);
        FileInfo target = new FileInfo(targetPath);
        if (target.Exists && target.Length == source.Length && target.LastWriteTimeUtc == source.LastWriteTimeUtc)
        {
            return;
        }

        File.Copy(sourcePath, targetPath, true);
        File.SetLastWriteTimeUtc(targetPath, source.LastWriteTimeUtc);
    }

    private static void SafeRecreateDirectory(string workerRoot, string targetPath)
    {
        string fullWorkerRoot = EnsureTrailingSeparator(Path.GetFullPath(workerRoot));
        string fullTarget = Path.GetFullPath(targetPath);
        if (!fullTarget.StartsWith(fullWorkerRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("拒绝清理临时工程范围外的目录：" + fullTarget);
        }

        if (Directory.Exists(fullTarget))
        {
            Directory.Delete(fullTarget, true);
        }
        Directory.CreateDirectory(fullTarget);
    }

    private static string RelativePath(string root, string path)
    {
        string normalizedRoot = EnsureTrailingSeparator(Path.GetFullPath(root));
        string normalizedPath = Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("文件不在预期目录内：" + normalizedPath);
        }
        return normalizedPath.Substring(normalizedRoot.Length);
    }

    private static string EnsureTrailingSeparator(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            ? path
            : path + Path.DirectorySeparatorChar;
    }

    private static string ProjectRoot()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    }

    private static string WorkerRoot()
    {
        return Path.Combine(Path.GetTempPath(), WorkerFolderName);
    }

    private static string Quote(string value)
    {
        return "\"" + value.Replace("\"", "\\\"") + "\"";
    }
}
