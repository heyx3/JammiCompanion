using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using CSCore;
using CSCore.SoundIn;


public class AudioDeviceListener : IDisposable
{
	private WaveIn audioInput = null;
	private float[] samplesBuffer = new float[1024];


	public int SamplingFrequency { get; private set; }
	public string DeviceName { get; private set; }

	public bool IsListening
	{
		get { return audioInput != null; }
	}


	/// <summary>
	/// Raised when this listener has new samples in from the audio device.
	/// The first argument is the sample buffer.
	/// The second argument is the number of available samples in the buffer.
	/// NOTE: this is raised in a different thread;
	///     don't directly call Unity API methods from this event.
	/// </summary>
	public event Action<float[], int> OnNewSamples;


	/// <summary>
	/// Stops this listener if it's currently running.
	/// </summary>
	public void Stop()
	{
		if (IsListening)
		{
			audioInput.Stop();
			audioInput.Dispose();
			audioInput = null;
		}
	}
	/// <summary>
	/// Stops this listener if it's currently running,
	///     then restarts it with the given parameters.
	/// </summary>
	/// <param name="deviceName">
	/// The name of one of the devices from CSCore.SoundIn.WaveInDevice.EnumerateDevices().
	/// Pass null to leave it unchanged.
	/// </param>
	/// <param name="samplingFrequency">
	/// The sampling frequency. Pass a negative value to leave it unchanged.
	/// </param>
	public void Start(string deviceName = null, int samplingFrequency = -1)
	{
		Stop();

		if (deviceName != null)
			DeviceName = deviceName;
		if (samplingFrequency >= 0)
			SamplingFrequency = samplingFrequency;

		audioInput = new WaveIn(new WaveFormat(SamplingFrequency, 16, 1));
		audioInput.Device = WaveInDevice.EnumerateDevices().First(dev => dev.Name == DeviceName);
		audioInput.DataAvailable += Callback_AudioSamplesAvailable;
		audioInput.Latency = 100;

		audioInput.Initialize();
		audioInput.Start();
	}

	public void Dispose() { Stop(); }

	private void Callback_AudioSamplesAvailable(object sender, CSCore.SoundIn.DataAvailableEventArgs e)
	{
		//Make sure the buffer is big enough to hold the samples.
		//Note that there are two bytes to each sample.
		if (samplesBuffer == null || samplesBuffer.Length < (e.ByteCount / 2))
			samplesBuffer = new float[e.ByteCount / 2];

		//Put the samples in the buffer.
		for (int i = 0; i < e.ByteCount; i += 2)
		{
			short sample = (short)(e.Data[i] |
								   (e.Data[i + 1] << 8));
			samplesBuffer[i / 2] = sample / (short.MaxValue + 1.0f); //TODO: I think the values start at short.MinValue, not 0.
		}

		//Raise the event.
		if (OnNewSamples != null)
			OnNewSamples(samplesBuffer, e.ByteCount / 2);
	}
}