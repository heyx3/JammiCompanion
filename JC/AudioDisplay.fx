sampler2D audioSamples;// : register(s0);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float audioSample = tex2D(audioSamples, uv).r;
	return float4(audioSample, audioSample, audioSample, 1.0);
}