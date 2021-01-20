/* 
AutoBuilder.cs
Automatically changes the target platform and creates a build.
 
Installation
Place in an Editor folder.
 
Usage
Go to File > AutoBuilder and select a platform. These methods can also be run from the Unity command line using -executeMethod AutoBuilder.MethodName.
 
License
Copyright (C) 2011 by Thinksquirrel Software, LLC
 
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
 
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
 
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 */
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class AutoBuilder {
	
	static string GetProjectName()
	{
		string[] s = Application.dataPath.Split('/');
		return s[s.Length - 2];
	}
	
	static string[] GetScenePaths()
	{
		string[] scenes = new string[EditorBuildSettings.scenes.Length];
		
		for(int i = 0; i < scenes.Length; i++)
		{
			scenes[i] = EditorBuildSettings.scenes[i].path;
		}
		
		return scenes;
	}

	public const string outputDirectoryName = "SteamWorkshopUploader";
	public static readonly string basePath = Application.dataPath + "/../";
    public static readonly string buildPath = basePath + "Builds/";
	public static readonly string extrasDirectoryName = basePath + "BuildExtras/";

	private static void PreBuild()
	{

	}

    private static void PostBuild()
    {
		CopyFileToBuilds(basePath, "config.json");
		CopyFileToBuilds(basePath, "steam_appid.txt");

        CopyDirectory(extrasDirectoryName, buildPath + "Win/" + outputDirectoryName + "/");
        CopyDirectory(extrasDirectoryName, buildPath + "OSX-Universal/" + outputDirectoryName + ".app/");
        CopyDirectory(extrasDirectoryName, buildPath + "Linux/" + outputDirectoryName + "/");
    }

    static void CopyToSteam(string buildPath, string steamPath)
    {

    }

	[MenuItem("File/AutoBuilder/Build ALL")]
	static void BuildAll()
	{
		PerformWinBuild();
		PerformOSXUniversalBuild();
		PerformLinuxUniversalBuild();
		
        PostBuild();
	}

	public static void CopyFileToBuilds(string directory, string fileName)
	{
		string sourcePath = directory + "/" + fileName;

		File.Copy(sourcePath, buildPath + "Win/" + outputDirectoryName + "/" + fileName, true);
		File.Copy(sourcePath, buildPath + "OSX-Universal/" + outputDirectoryName + ".app/" + fileName, true);
		File.Copy(sourcePath, buildPath + "Linux/" + outputDirectoryName + "/" + fileName, true);
	}

	public static void CopyDirectory(string SourcePath, string DestinationPath)
	{
		//Now Create all of the directories
		foreach (string dirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
		{
			Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));
		}
		
		//Copy all the files & Replaces any files with the same name
		foreach (string newPath in Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories))
		{
			File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
		}
	}
	
    private static void BuildWithLogging(string identifier, System.Action buildFunction)
    {
        Debug.LogFormat("---=== BUILD BEGIN! {0} @ {1}", identifier, System.DateTime.Now.ToShortTimeString());

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        PreBuild();

        buildFunction();

        //PostBuild();

        stopwatch.Stop();
        float seconds = Mathf.FloorToInt((float)stopwatch.Elapsed.TotalSeconds);
        float mins = (float)stopwatch.Elapsed.TotalMinutes;
        Debug.LogFormat("---=== BUILD COMPLETE! {0} (elapsed: {1:0.00}m) @ {2}", identifier, mins, System.DateTime.Now.ToShortTimeString());
    }

	//[MenuItem("File/AutoBuilder/Windows")]
	static void PerformWinBuild ()
	{
		BuildWithLogging("Windows", () => {
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows);
			BuildPipeline.BuildPlayer(GetScenePaths(), "Builds/Win/" + outputDirectoryName + "/" + GetProjectName() + ".exe",BuildTarget.StandaloneWindows,BuildOptions.None);
		});
	}

	//[MenuItem("File/AutoBuilder/Mac OSX")]
	static void PerformOSXUniversalBuild ()
	{
		BuildWithLogging("Mac OSX", () => {
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
			BuildPipeline.BuildPlayer(GetScenePaths(), "Builds/OSX-Universal/" + GetProjectName() + ".app", BuildTarget.StandaloneOSX,BuildOptions.None);
		});
	}

	//[MenuItem("File/AutoBuilder/Linux")]
	static void PerformLinuxUniversalBuild ()
	{
		BuildWithLogging("Linux", () => {
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneLinuxUniversal);
			BuildPipeline.BuildPlayer(GetScenePaths(), "Builds/Linux/" + outputDirectoryName + "/", BuildTarget.StandaloneLinuxUniversal,BuildOptions.None);
		});
	}
}