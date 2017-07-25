using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Samples audio and turns it into a renderable 1D texture.
/// </summary>
[Serializable]
public class AudioGraph
{
	public static Material RenderMat
	{
		get
		{
			if (renderMat == null)
				renderMat = Resources.Load<Material>("AudioGraphMat");

			return renderMat;
		}
	}
	private static Material renderMat = null;

	public static void SetRenderMatParams(float lineThickness, Color lineColor,
										  Color belowLineColor, Color aboveLineColor,
										  Material _renderMat = null)
	{
		if (_renderMat == null)
			_renderMat = RenderMat;

		_renderMat.SetFloat("_LineThickness", lineThickness);
		_renderMat.SetColor("_LineColor", lineColor);
		_renderMat.SetColor("_AboveColor", aboveLineColor);
		_renderMat.SetColor("_BelowColor", belowLineColor);
	}


	public int NSamplesPerAverage = 26,
			   NAverages = 4096;


	[NonSerialized]
	public Texture2D SampleTex;
	[NonSerialized]
	public int CurrentSample = 0,
			   CurrentPixel = 0;


	/// <summary>
	/// The sum is stored in the Green channel.
	/// The average is stored in the Red channel, which is what the shader samples.
	/// </summary>
	private Color[] pixels;


	public void Init()
	{
		//Set up buffer.
		if (pixels == null || pixels.Length != NAverages)
			pixels = new Color[NAverages];
		for (int i = 0; i < pixels.Length; ++i)
			pixels[i] = new Color(0.0f, 0.0f, 0.0f, 1.0f);

		//Set up texture.
		if (SampleTex == null)
		{
			SampleTex = new Texture2D(NAverages, 1, TextureFormat.RGBAFloat, false, true);
			SampleTex.filterMode = FilterMode.Bilinear;
			SampleTex.wrapMode = TextureWrapMode.Clamp;
		}
		else if (SampleTex.width != NAverages)
		{
			SampleTex.Resize(NAverages, 1);
		}

		//Set up counters.
		CurrentSample = 0;
		CurrentPixel = 0;
	}

	/// <summary>
	/// Adds the given samples to the audio graph.
	/// </summary>
	/// <param name="buffer">The array the samples reside in.</param>
	/// <param name="nSamples">
	/// The number of samples to get from "buffer".
	/// </param>
	/// <param name="startOffset">
	/// The index in "buffer" of the first sample to use.
	/// </param>
	public void AddSamples(float[] buffer, int nSamples, int startOffset = 0)
	{
		//If there are more samples than pixels coming in, some of our math will break.
		UnityEngine.Assertions.Assert.IsTrue(nSamples < (pixels.Length * NSamplesPerAverage));

		bool pixelChanged = false;
		for (int _i = 0; _i < nSamples; ++_i)
		{
			//Get the sample.
			int i = _i + startOffset;
			float sample = buffer[i];

			//Add it to the current sum.
			var color = pixels[i];
			color.g += sample;
			pixels[i] = color;
			CurrentSample += 1;

			//If we've got enough samples, find the average and move on.
			if (CurrentSample >= NSamplesPerAverage)
			{
				pixelChanged = true;

				color.r = color.g / CurrentSample;
				pixels[i] = color;

				CurrentPixel = (CurrentPixel + 1) % pixels.Length;
				pixels[CurrentPixel] = new Color(0.0f, 0.0f, 0.0f, 1.0f);

				CurrentSample = 0;
			}
		}

		//Update the texture if anything changed.
		if (pixelChanged)
		{
			SampleTex.SetPixels(pixels);
			SampleTex.Apply(false, false);
		}
	}
}