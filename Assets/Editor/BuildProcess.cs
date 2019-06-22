using System;
using UnityEditor;

public static class BuildProcess
{
    [MenuItem("SSM/Project/Build All")]
    public static void Build()
    {
        var date     = DateTime.Now.ToString("yyyy-mm-dd-hh-mm-tt");
        var opts     = new BuildPlayerOptions();
        opts.options = BuildOptions.None;
        opts.scenes  = new string[] { "Assets/Scenes/MainScene.unity" };

        opts.targetGroup      = BuildTargetGroup.Standalone;
        opts.target           = BuildTarget.StandaloneWindows;
        opts.locationPathName = $"builds/win/{date}/mgpso/mgpso.exe";
        BuildPipeline.BuildPlayer(opts);

        opts.targetGroup      = BuildTargetGroup.Standalone;
        opts.target           = BuildTarget.StandaloneLinux64;
        opts.locationPathName = $"builds/linux/{date}/mgpso";
        BuildPipeline.BuildPlayer(opts);

        opts.targetGroup      = BuildTargetGroup.Standalone;
        opts.target           = BuildTarget.StandaloneOSX;
        opts.locationPathName = $"builds/osx/{date}/mgpso";
        BuildPipeline.BuildPlayer(opts);

        opts.targetGroup      = BuildTargetGroup.WebGL;
        opts.target           = BuildTarget.WebGL;
        opts.locationPathName = $"builds/webgl/{date}/mgpso";
        BuildPipeline.BuildPlayer(opts);
    }
}