using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;


/// <summary>
/// Records input from an AudioMixer (expected to be the microphone)
///     and provides it to whoever wants it.
/// </summary>
public class MicrophoneListener : IDisposable
{
	//Based on benjaminoutram's answer on this page:
	//    http://answers.unity3d.com/questions/1113690/microphone-input-in-unity-5x.html


	/// <summary>
	/// Raised when a new piece of audio from the microphone comes in.
	/// The second argument is the actual length of the recording,
	///     which is a bit shorter than the clip itself.
	/// </summary>
	public event Action<AudioClip, float> OnNewAudioFragment;

	/// <summary>
	/// The length of time between captured audio clips.
	/// </summary>
	public int IntervalSeconds { get; private set; }
	/// <summary>
	/// The microphone device to use, from "Microphone.devices".
	/// </summary>
	public string Device { get; private set; }
	/// <summary>
	/// Whether this instance is currently listening to the microphone device.
	/// </summary>
	public bool IsListening { get; private set; }
	/// <summary>
	/// The number of channels.
	/// This property only exists when this instance is currently listening.
	/// </summary>
	public int NChannels { get; private set; }
	/// <summary>
	/// The sampling frequency of the device.
	/// </summary>
	public int SamplingFrequency { get; private set; }

	private AudioClip currentClip = null;
	private float timeSinceRestart = 0.0f;


	public MicrophoneListener(bool startMicNow, string device = null,
							  int intervalSeconds = 1, int samplingFrequency = 44100)
	{
		Device = device;
		IntervalSeconds = intervalSeconds;
		SamplingFrequency = samplingFrequency;
		NChannels = -1;
		IsListening = false;

		if (startMicNow)
		{
			timeSinceRestart = Time.time;
			Start();
		}
	}

	public void Update()
	{
		//Wait for the next microphone clip.
		//Must be done during an Update() to work, for unknown reasons.
		if (IsListening)
		{
			if (Time.time - timeSinceRestart >= IntervalSeconds)// && !Microphone.IsRecording(Device))
			{
				//Wait until microphone position is found (?), then finish the audio source.
				//do { } while (Microphone.GetPosition(Device) <= 0);

				if (OnNewAudioFragment != null)
					OnNewAudioFragment(currentClip, Time.time - timeSinceRestart);

				currentClip = Microphone.Start(Device, false, IntervalSeconds, SamplingFrequency);
				NChannels = currentClip.channels;
				timeSinceRestart = Time.time;
			}
		}
	}

	/// <summary>
	/// Starts listening to the microphone.
	/// </summary>
	public void Start()
	{
		IsListening = true;
		timeSinceRestart = Time.time;

		currentClip = Microphone.Start(Device, false, IntervalSeconds, SamplingFrequency);
		NChannels = currentClip.channels;
	}
	/// <summary>
	/// Stops listening to the microphone.
	/// </summary>
	public void Stop()
	{
		UnityEngine.Assertions.Assert.IsTrue(IsListening,
											 "Called MicrophoneListener.Stop() before Start()");

		//Put out the last clip.
		if (OnNewAudioFragment != null)
			OnNewAudioFragment(currentClip, Time.time - timeSinceRestart);

		IsListening = false;
		currentClip = null;
		NChannels = -1;
		Microphone.End(null);
	}

	public void Dispose()
	{
		if (IsListening)
			Stop();
	}
}