using UnityEngine;
using System.Collections;

public class SceneTransition : MonoBehaviour
{	
	public string nextSceneName = "";
	public int nextSceneIndex = -1;
	public bool automatic = true;
	public float delay = 1.0f;
	
	void Start ()
	{
		if(automatic)
		{
			Invoke("OnNextScene", delay);
		}
	}
	
	void OnNextScene()
	{
		if(string.IsNullOrEmpty(nextSceneName))
		{
			if(nextSceneIndex == -1)
			{
				// if no scene is specified, go to the next scene in the build settings
				Application.LoadLevel(Application.loadedLevel+1);
			}
			else
			{
				Application.LoadLevel(nextSceneIndex);
			}
		}
		else
		{
			Application.LoadLevel(nextSceneName);
		}
	}
}
