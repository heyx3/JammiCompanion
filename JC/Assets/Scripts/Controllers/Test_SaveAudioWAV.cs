﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Test_SaveAudioWAV : MonoBehaviour
{
	public string Directory = "C:\\Users\\Billy\\Desktop\\tempWAVs";
	public int SampleFrequency = 44100;

	private AudioDeviceListener listener;
	private int nextFileI = 1;
	private List<float> currentWAVData = new List<float>();

	private void Start()
	{
		listener = new AudioDeviceListener();
		listener.OnNewSamples += (samplesBuffer, nSamples) =>
		{
			//Copy the samples to the WAV file data.
			for (int i = 0; i < nSamples; ++i)
				currentWAVData.Add(samplesBuffer[i]);
		};
		listener.Start(CSCore.SoundIn.WaveInDevice.EnumerateDevices().First().Name,
					   SampleFrequency);
	}
	private void OnGUI()
	{
		if (GUI.Button(new Rect(50.0f, 0.0f, 100.0f, 100.0f),
					   (listener.IsListening ? "Stop" : "Start")))
		{
			if (listener.IsListening)
			{
				listener.Stop();
				if (currentWAVData.Count > 0)
					FlushToFile();
			}
			else
			{
				listener.Start();
			}
		}
		if (listener.IsListening &&
			GUI.Button(new Rect(0.0f, 150.0f, 200.0f, 100.0f),
					   "Break WAV File Here"))
		{
			FlushToFile();
		}
	}
	private void OnDestroy()
	{
		if (listener.IsListening)
		{
			listener.Stop();
			FlushToFile();
		}

		listener.Dispose();
	}

	private void FlushToFile()
	{
		//Create the WAV folder if it doesn't exist.
		if (!System.IO.Directory.Exists(Directory))
			System.IO.Directory.CreateDirectory(Directory);

		//Save the samples to a file.
		SavWav.Save(System.IO.Path.Combine(Directory, nextFileI + ".wav"),
					currentWAVData.ToArray(),
					listener.SamplingFrequency, 1, currentWAVData.Count);
		currentWAVData.Clear();
		nextFileI += 1;
	}
}