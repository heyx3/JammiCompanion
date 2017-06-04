using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JC
{
	/// <summary>
	/// Detects when a noticeable sound was made.
	/// </summary>
	public class JammiSoundDetector : IDisposable
	{
		/// <summary>
		/// Raised when an interesting sound is detected.
		/// The given array is the set of samples for the sound.
		/// </summary>
		public event Action<float[]> OnInterestingSound;

		/// <summary>
		/// The number of samples in a single interval.
		/// After an interval ends, if nothing interesting happened inside it, it is discarded.
		/// </summary>
		public int SampleIntervalSize
		{
			get { return sampleBuffer.Length; }
			set
			{
				if (SampleIntervalSize == value)
					return;

				float[] newBuffer = new float[value];
				for (int i = 0; i < sampleBufferNextI; ++i)
					newBuffer[i] = (sampleBuffer == null ? 0.0f : sampleBuffer[i]);
				sampleBuffer = newBuffer;
			}
		}
		/// <summary>
		/// If a sample is above this threshold, it is considered interesting.
		/// </summary>
		public float InterestingSoundThreshold;
		/// <summary>
		/// If an "interesting" sound lasts at least this long, it isn't ignored.
		/// </summary>
		public int InterestingSoundMinSamples;
		/// <summary>
		/// The number of samples to check before accepting the end of an interesting sound.
		/// </summary>
		public int InterestingSoundEndSamples;


		private float[] sampleBuffer = null;
		private int sampleBufferNextI = 0;

		private List<float> importantSamples = new List<float>();


		/// <summary>
		/// Rudimentary state machine for what this detector is currently doing.
		/// </summary>
		private enum States
		{
			/// <summary>
			/// Waiting for something interesting to happen.
			/// </summary>
			WaitingForSound,
			/// <summary>
			/// Confirming that something interesting is happening, not just random noise.
			/// </summary>
			ConfirmingSound,
			/// <summary>
			/// Recording an interesting sound in progress.
			/// </summary>
			RecordingSound,
			/// <summary>
			/// Confirming that the end of the sound is here.
			/// </summary>
			ConfirmingEnd,
		}
		private States currentState;

		private int state_Confirming_SamplesLeft;


		public JammiSoundDetector(int sampleIntervalSize,
								  float interestingSoundThreshold,
								  int interestingSoundMinSamples,
								  int interestingSoundEndSamples)
		{
			SampleIntervalSize = sampleIntervalSize;
			InterestingSoundThreshold = interestingSoundThreshold;
			InterestingSoundMinSamples = interestingSoundMinSamples;
			InterestingSoundEndSamples = interestingSoundEndSamples;

			currentState = States.WaitingForSound;
		}


		/// <summary>
		/// Adds the next audio sample to this detector's current buffer.
		/// </summary>
		public void AddSample(float sample)
		{
			sampleBuffer[sampleBufferNextI] = sample;
			sampleBufferNextI = (sampleBufferNextI + 1) % sampleBuffer.Length;

			switch (currentState)
			{
				case States.WaitingForSound:
					//If a loud noise appears, see if it's the start of something interesting.
					if (sample > InterestingSoundThreshold)
					{
						currentState = States.ConfirmingSound;
						state_Confirming_SamplesLeft = InterestingSoundMinSamples - 1;

						//Edge-case: min samples for an interesting sound is 1.
						if (state_Confirming_SamplesLeft < 1)
						{
							currentState = States.RecordingSound;
							for (int i = 0; i < sampleBufferNextI; ++i)
								importantSamples.Add(sampleBuffer[i]);
						}
					}
					break;

				case States.ConfirmingSound:
					//If the loud noise continues, see if it's considered interesting yet.
					if (sample > InterestingSoundThreshold)
					{
						state_Confirming_SamplesLeft -= 1;
						if (state_Confirming_SamplesLeft < 1)
						{
							currentState = States.RecordingSound;
							for (int i = 0; i < sampleBufferNextI; ++i)
								importantSamples.Add(sampleBuffer[i]);
						}
					}
					//Otherwise, it's not interesting.
					else
					{
						currentState = States.WaitingForSound;
					}
					break;

				case States.RecordingSound:
					importantSamples.Add(sample);

					//If the noise wasn't very loud, see if this is the end of the interesting noise.
					if (sample < InterestingSoundThreshold)
					{
						currentState = States.ConfirmingEnd;
						state_Confirming_SamplesLeft = InterestingSoundEndSamples - 1;
					}
					break;

				case States.ConfirmingEnd:
					importantSamples.Add(sample);

					//If the noise was loud, this isn't the end of the noise.
					if (sample > InterestingSoundThreshold)
					{
						currentState = States.RecordingSound;
					}
					//Otherwise, see if it's considered finished yet.
					else
					{
						state_Confirming_SamplesLeft -= 1;
						if (state_Confirming_SamplesLeft < 1)
							FinishInterestingSound();
					}
					break;

				default: throw new NotImplementedException(currentState.ToString());
			}
		}
		private void FinishInterestingSound()
		{
			var finalData = importantSamples.ToArray();

			importantSamples.Clear();
			currentState = States.WaitingForSound;

			OnInterestingSound(importantSamples.ToArray());
		}

		/// <summary>
		/// Finishes up any sampling currently being done,
		///     then resets this detector to its initial state.
		/// </summary>
		public void Dispose()
		{
			//If we're in the middle of recording an interesting sound, finish up.
			switch (currentState)
			{
				case States.WaitingForSound:
				case States.ConfirmingSound:
					break;

				case States.RecordingSound:
				case States.ConfirmingEnd:
					FinishInterestingSound();
					break;

				default: throw new NotImplementedException(currentState.ToString());
			}

			//Clear all state.
			currentState = States.WaitingForSound;
			sampleBufferNextI = 0;
		}
	}
}
