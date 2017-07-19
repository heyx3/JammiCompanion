using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Test_SaveAudioWAV : MonoBehaviour
{
	public string Directory = "C:\\Users\\Billy\\Desktop\\tempWAVs";
	public int MicIntervalSeconds = 1;
	public int SampleFrequency = 44100;

	private MicrophoneListener listener;
	private int nextFileI = 1;
	private float[] tempSamples = null;
	private List<float> currentWAVData = new List<float>();

	private void Start()
	{
		listener = new MicrophoneListener(false, null, MicIntervalSeconds, SampleFrequency);
		listener.OnNewAudioFragment += (clip, length) =>
		{
			int nSamples = (int)(clip.frequency * (double)length) * clip.channels;

			//Read the samples into the buffer.
			if (tempSamples == null || tempSamples.Length < nSamples)
				tempSamples = new float[nSamples];
			clip.GetData(tempSamples, 0); //NOTE: Unity will log a warning. Nothing we can do about that.

			//Copy the samples to the WAV file data.
			for (int i = 0; i < nSamples; ++i)
				currentWAVData.Add(tempSamples[i]);
		};
	}
	private void Update()
	{
		listener.Update();
	}
	private void OnGUI()
	{
		if (GUI.Button(new Rect(50.0f, 0.0f, 100.0f, 100.0f),
					   (listener.IsListening ? "Stop" : "Start")))
		{
			if (listener.IsListening)
			{
				int nChannels = listener.NChannels;
				listener.Stop();
				if (currentWAVData.Count > 0)
					FlushToFile(nChannels);
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
			int nChannels = listener.NChannels;
			listener.Stop();
			FlushToFile(nChannels);
		}

		listener.Dispose();
	}

	private void FlushToFile(int nChannels = -1)
	{
		if (nChannels == -1)
			nChannels = listener.NChannels;

		//Create the WAV folder if it doesn't exist.
		if (!System.IO.Directory.Exists(Directory))
			System.IO.Directory.CreateDirectory(Directory);

		//Save the samples to a file.
		SavWav.Save(System.IO.Path.Combine(Directory, nextFileI + ".wav"),
					currentWAVData.ToArray(),
					listener.SamplingFrequency, nChannels, currentWAVData.Count);
		currentWAVData.Clear();
		nextFileI += 1;
	}
}