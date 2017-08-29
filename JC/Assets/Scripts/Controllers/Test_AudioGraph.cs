using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Test_AudioGraph : MonoBehaviour
{
	public int SampleFrequency = 44100;

	public float LineThickness = 0.05f,
				 GraphPower = 1.0f;
	public Color LineColor = Color.white,
				 AboveLineColor = Color.red,
				 BelowLineColor = Color.green;

	[SerializeField]
	private AudioGraph audioGrapher = new AudioGraph();


	private AudioDeviceListener listener;


	private void Awake()
	{
		listener = new AudioDeviceListener();
		listener.OnNewSamples += (samples, samplesLen) =>
		{
			audioGrapher.AddSamples(samples, samplesLen);
		};

		listener.Start(CSCore.SoundIn.WaveInDevice.EnumerateDevices().First().Name,
					   SampleFrequency);
		audioGrapher.Init();
	}
	private void Update()
	{
		audioGrapher.Update();
		AudioGraph.SetRenderMatParams(LineThickness, GraphPower,
									  LineColor, BelowLineColor, AboveLineColor);
	}
	private void OnDestroy()
	{
		listener.Dispose();
	}

	private void OnGUI()
	{
		if (audioGrapher.SampleTex == null)
			return;

		Graphics.DrawTexture(new Rect(0.0f, 0.0f, Screen.width, Screen.height),
							 audioGrapher.SampleTex,
							 AudioGraph.RenderMat);
	}
}