using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;


namespace JC
{
	/// <summary>
	/// A shader effect that displays audio samples from a 1D texture.
	/// </summary>
	public class AudioRenderEffect : ShaderEffect
	{
		private static PixelShader shader = new PixelShader()
		{ UriSource = Utils.MakePackUri("AudioDisplay.ps") };
		private static readonly DependencyProperty audioSamplesProperty =
			RegisterPixelShaderSamplerProperty("audioSamples", typeof(AudioRenderEffect), 0);

		private float[] audioSamplesBuffer;
		private ushort[] tempAudioSamplesBuffer;
		private int nSrcSamplesPerBufferSample,
					currentIndex_Buffer = 0,
					currentCountInBufferIndex = 0;
		private WriteableBitmap audioSamplesTexture;
		private int currentIndex_Texture = 0;

		public AudioRenderEffect() { Init(256, 100); }
		public AudioRenderEffect(int nMaxValues, int nSamplesPerValue) { Init(nMaxValues, nSamplesPerValue); }
		private void Init(int nMaxValues, int nSamplesPerValue)
		{
			nSrcSamplesPerBufferSample = nSamplesPerValue;

			PixelShader = shader;
			UpdateShaderValue(audioSamplesProperty);

			//Initialize the pixel buffer.
			audioSamplesBuffer = new float[nMaxValues];
			for (int x = 0; x < nMaxValues; ++x)
				audioSamplesBuffer[x] = 0.0f;

			//Create the texture.
			audioSamplesTexture = new WriteableBitmap(nMaxValues, 1, 96.0, 96.0,
													  PixelFormats.Gray16,
													  BitmapPalettes.Gray256);

			//Create the brush for the shader.
			var audioSamplesBrush = new ImageBrush(audioSamplesTexture);
			SetValue(audioSamplesProperty, audioSamplesBrush);
		}

		public void StartAddSamples(int nSamples)
		{
			//Reset buffer counters.
			currentIndex_Buffer = 0;
			currentCountInBufferIndex = 0;
		}
		public void AddSample(float sample)
		{
			audioSamplesBuffer[currentIndex_Buffer] += sample;
			currentCountInBufferIndex += 1;

			//If we've got all the samples we need,
			//    calculate the average and move on to the next value.
			if (currentCountInBufferIndex >= nSrcSamplesPerBufferSample)
			{
				audioSamplesBuffer[currentIndex_Buffer] /= currentCountInBufferIndex;
				currentIndex_Buffer += 1;
				currentCountInBufferIndex = 0;
			}
		}
		public void EndAddSamples()
		{
			//If we're in the middle of calculating a new sample, finish up.
			if (currentCountInBufferIndex > 0)
			{
				audioSamplesBuffer[currentIndex_Buffer] /= currentCountInBufferIndex;
				currentIndex_Buffer += 1;
				currentCountInBufferIndex = 0;
			}

			//Write the samples to the texture.
			int nBufferSamples = currentIndex_Buffer;
			currentIndex_Buffer = 0;
			while (currentIndex_Buffer < nBufferSamples)
			{
				int nSamplesTillEndOfTex = audioSamplesTexture.PixelWidth - currentIndex_Texture;
				int nSamplesToWrite = Math.Min(nSamplesTillEndOfTex,
											   nBufferSamples - currentIndex_Buffer);

				//Make sure the temp buffer is large enough.
				if (tempAudioSamplesBuffer.Length < nSamplesToWrite)
				{
					tempAudioSamplesBuffer = new ushort[Math.Max(nSamplesToWrite,
																 tempAudioSamplesBuffer.Length * 2)];
				}

				//Fill the temp buffer.
				for (int i = 0; i < nSamplesToWrite; ++i)
				{
					float f = audioSamplesBuffer[i + currentIndex_Buffer];
					f = Math.Min(1.0f - float.Epsilon, Math.Max(0.0f, f));
					tempAudioSamplesBuffer[i] = (ushort)(ushort.MaxValue * f);
				}

				audioSamplesTexture.WritePixels(new Int32Rect(currentIndex_Texture, 0,
															  nSamplesToWrite, 1),
												audioSamplesBuffer,
												sizeof(ushort) * nSamplesToWrite,
												currentIndex_Buffer * sizeof(ushort));//TODO: Remove " * sizeof(ushort)"?

				currentIndex_Texture = 0;
				currentIndex_Buffer += nSamplesToWrite;
			}
		}

		//Source:
		//https://blogs.msdn.microsoft.com/greg_schechter/2008/05/12/writing-custom-gpu-based-effects-for-wpf/
	}
}
