using UnityEngine;
using System.Collections;

public class FullscreenTexture : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Setup();
	}
	
	void Setup()
	{
		transform.position = Vector3.zero;
		GetComponent<GUITexture>().pixelInset = new Rect(0,0, Screen.width, Screen.height);
	}
}
