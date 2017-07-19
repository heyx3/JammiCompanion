//This file is provided as an example of how to use CSCore,
//    in case I decide later to go back to that instead of Unity's Microphone class.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace JC
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public bool IsListening
		{
			get { return audioInput != null; }
			set
			{
				if (IsListening == value)
					return;

				if (value)
				{
					button_ToggleListening.Content = "Stop Listening";

					//Start listening.
					int deviceNumber = dropdown_AudioSource.SelectedIndex;
					audioInput = new CSCore.SoundIn.WaveIn(new CSCore.WaveFormat(44100, 16, 1));
					audioInput.Device = CSCore.SoundIn.WaveInDevice.EnumerateDevices().Skip(deviceNumber).First();
					audioInput.DataAvailable += Callback_AudioInput;
					audioInput.Latency = 100;
					audioInput.Initialize();
					audioInput.Start();
				}
				else
				{
					button_ToggleListening.Content = "Start Listening";

					//Stop listening.
					detector.Dispose();
					audioInput.Stop();
					audioInput.Dispose();
					audioInput = null;
				}
			}
		}

		private CSCore.SoundIn.WaveIn audioInput = null;
		private JammiSoundDetector detector = null;
		private AudioRenderEffect audioRenderEffect;


		public MainWindow()
		{
			InitializeComponent();

			dropdown_AudioSource.GotFocus += dropdown_AudioSource_GotFocus;
			dropdown_AudioSource_GotFocus(null, null);

			detector = new JammiSoundDetector(int.Parse(text_SampleIntervalSize.Text),
											  float.Parse(text_InterestingSoundThreshold.Text),
											  int.Parse(text_InterestingSoundMinSamples.Text),
											  int.Parse(text_InterestingSoundEndSamples.Text));
			detector.OnInterestingSound += Callback_InterestingSound;
		}


		protected override void OnClosed(EventArgs e)
		{
			//Clean up the audio input.
			IsListening = false;

			base.OnClosed(e);
		}


		private void Callback_InterestingSound(float[] samples)
		{
			//DEBUG: Write it to a file.
			string prefix = System.IO.Path.Combine(Environment.CurrentDirectory, "Jammi");
			int i = 1;
			while (System.IO.File.Exists(prefix + i + ".wav"))
				i += 1;
			using (var writer = new CSCore.Codecs.WAV.WaveWriter(prefix + i + ".wav", audioInput.WaveFormat))
				writer.WriteSamples(samples, 0, samples.Length);
		}
		private void Callback_AudioInput(object sender, CSCore.SoundIn.DataAvailableEventArgs e)
		{
			audioRenderEffect.StartAddSamples(e.ByteCount / 2);

			//Every 2 bytes in the buffer represents one sample.
			for (int i = 0; i < e.ByteCount; i += 2)
			{
				short sample = (short)(e.Data[i] |
									   (e.Data[i + 1] << 8));
				float sampleF = sample / (short.MaxValue + 1.0f); //TODO: I think the values start at short.MinValue, not 0.

				detector.AddSample(sampleF);
				audioRenderEffect.AddSample(sampleF);
			}

			audioRenderEffect.EndAddSamples();
		}

		private bool updatingAudioSources = false;
		private void dropdown_AudioSource_GotFocus(object sender, RoutedEventArgs e)
		{
			if (updatingAudioSources)
				return;
			updatingAudioSources = true;

			//Update the dropdown's contents with the currently-available devices.
			//Try to preserve the currently-selected device.

			string deviceName = (dropdown_AudioSource.SelectedItem == null ?
									"" :
									dropdown_AudioSource.SelectedItem.ToString());

			int selectedIndex = 0;
			dropdown_AudioSource.Items.Clear();

			int i = 0;
			foreach (var device in CSCore.SoundIn.WaveInDevice.EnumerateDevices())
			{
				dropdown_AudioSource.Items.Add(device.Name);
				if (device.Name == deviceName)
					selectedIndex = i;

				i += 1;
			}

			dropdown_AudioSource.SelectedIndex = selectedIndex;

			updatingAudioSources = false;
		}

		private void button_ToggleListening_Click(object sender, RoutedEventArgs e)
		{
			IsListening = !IsListening;
		}

		private void text_SampleIntervalSize_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (detector == null)
				return;

			int i;
			if (int.TryParse(text_SampleIntervalSize.Text, out i))
				detector.SampleIntervalSize = i;
		}
		private void text_InterestingSoundThreshold_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (detector == null)
				return;

			float f;
			if (float.TryParse(text_InterestingSoundThreshold.Text, out f))
				detector.InterestingSoundThreshold = f;
		}
		private void text_InterestingSoundMinSamples_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (detector == null)
				return;

			int i;
			if (int.TryParse(text_InterestingSoundMinSamples.Text, out i))
				detector.InterestingSoundMinSamples = i;
		}
		private void text_InterestingSoundEndSamples_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (detector == null)
				return;

			int i;
			if (int.TryParse(text_InterestingSoundEndSamples.Text, out i))
				detector.InterestingSoundEndSamples = i;
		}
	}
}
