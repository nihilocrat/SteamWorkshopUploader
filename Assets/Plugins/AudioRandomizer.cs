using UnityEngine;
using System.Collections;

[AddComponentMenu("Audio/Audio Randomizer")]
public class AudioRandomizer : MonoBehaviour
{	
	public bool playOnAwake = true;
	public float minVolume = 1.0f;
	public float maxVolume = 1.0f;
	public float minPitch = 1.0f;
	public float maxPitch = 1.0f;
	public AudioClip[] audioClips;
	
	// Use this for initialization
	void Start ()
	{
		Randomize();
		if(playOnAwake)
		{
			GetComponent<AudioSource>().Play();
		}
	}

	public void Randomize()
	{
		var originalPitch = GetComponent<AudioSource>().pitch;
		var pitch = (Random.value * (maxPitch - minPitch)) + minPitch;
		GetComponent<AudioSource>().pitch = pitch;
		
		var originalVolume = GetComponent<AudioSource>().volume;
		var volume = (Random.value * (maxVolume - minVolume)) + minVolume;
		GetComponent<AudioSource>().volume = volume;
		
		if(audioClips.Length > 0)
		{
			var index = Random.Range(0, audioClips.Length);
			GetComponent<AudioSource>().clip = audioClips[index];
		}
	}
	
}
