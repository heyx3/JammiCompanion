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

using NAudio;
using NAudio.Wave;


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
					audioInput = new WaveIn();
					audioInput.DeviceNumber = deviceNumber;
					audioInput.WaveFormat = new WaveFormat(44100, 1);
					audioInput.DataAvailable += AudioInputDataReceiver;
					audioInput.RecordingStopped += AudioInputDataStopped;
					audioInput.StartRecording();
				}
				else
				{
					button_ToggleListening.Content = "Start Listening";

					//Stop listening.
					audioInput.StopRecording();
					audioInput = null;
				}
			}
		}

		private WaveIn audioInput = null;
		private List<short> currentAudioData = new List<short>();


		public MainWindow()
		{
			InitializeComponent();

			dropdown_AudioSource.GotFocus += dropdown_AudioSource_GotFocus;
			dropdown_AudioSource_GotFocus(null, null);
		}


		protected override void OnClosed(EventArgs e)
		{
			//Clean up the audio input.
			IsListening = false;

			base.OnClosed(e);
		}

		private void AudioInputDataReceiver(object sender, WaveInEventArgs e)
		{
			//Every 2 bytes in the buffer represents one sample.
			currentAudioData.Capacity += e.BytesRecorded / 2;
			for (int i = 0; i < e.BytesRecorded; i += 2)
			{
				short sample = (short)(e.Buffer[i] |
									   (e.Buffer[i + 1] << 8));
				currentAudioData.Add(sample);
			}
		}
		private void AudioInputDataStopped(object sender, StoppedEventArgs e)
		{
			IsListening = false;

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

			for (int i = 0; i < WaveIn.DeviceCount; ++i)
			{
				var deviceInfo = WaveIn.GetCapabilities(i);
				dropdown_AudioSource.Items.Add(deviceInfo.ProductName);

				if (deviceInfo.ProductName == deviceName)
					selectedIndex = i;
			}

			dropdown_AudioSource.SelectedIndex = selectedIndex;

			updatingAudioSources = false;
		}

		private void button_ToggleListening_Click(object sender, RoutedEventArgs e)
		{
			IsListening = !IsListening;
		}
	}
}
