#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class WabishWindowsBuild
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string BuildDirectoryName = "DiceKing-Windows-x64-display-modes";
    private const string ExecutableName = "DiceKing.exe";
    private const string StatusFileName = "WabishWindowsBuildDisplayModes.status.txt";

    [MenuItem("Wabish/Build Windows x64 (Display Modes)")]
    public static void BuildWindowsX64()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string outputDirectory = Path.Combine(projectRoot, "Builds", "Windows", BuildDirectoryName);
        string outputPath = Path.Combine(outputDirectory, ExecutableName);
        string statusPath = Path.Combine(projectRoot, "Logs", StatusFileName);

        Directory.CreateDirectory(outputDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(statusPath));

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        string status = string.Join("\n", new[]
        {
            "result=" + summary.result,
            "output=" + outputPath,
            "totalSize=" + summary.totalSize.ToString(CultureInfo.InvariantCulture),
            "errors=" + summary.totalErrors.ToString(CultureInfo.InvariantCulture),
            "warnings=" + summary.totalWarnings.ToString(CultureInfo.InvariantCulture),
            "duration=" + summary.totalTime,
            "finishedUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
        });
        File.WriteAllText(statusPath, status + "\n");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException("Windows x64 build failed. See " + statusPath);
        }

        Debug.Log("Windows x64 build succeeded: " + outputPath);
    }
}
#endif
