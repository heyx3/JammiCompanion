using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Test_AudioGraph : MonoBehaviour
{
	public int MicIntervalSeconds = 1;
	public int SampleFrequency = 44100;
	public int NSamplesPerAverage = 20,
			   NAverages = 4096;

	[SerializeField]
	private AudioGraph audioGrapher = new AudioGraph();


	private MicrophoneListener listener;


	private void Awake()
	{
		listener = new MicrophoneListener(true, null, MicIntervalSeconds, SampleFrequency);
		listener.OnNewAudioFragment += (clip, duration, nSamples, samplesBuffer) =>
		{
			for (int i = 0; i < nSamples; ++i)
				audioGrapher.AddSamples(samplesBuffer, nSamples);
		};
	}
	private void OnDestroy()
	{
		listener.Dispose();
	}

	private void OnGUI()
	{
		const float scale = 4.0f;
		GUI.DrawTexture(new Rect(0.0f, 0.0f,
								 audioGrapher.SampleTex.width * scale,
								 audioGrapher.SampleTex.height * scale),
						audioGrapher.SampleTex);
	}
}