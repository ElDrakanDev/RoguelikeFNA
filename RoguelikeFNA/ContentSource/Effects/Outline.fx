sampler s0;

float2 _texelSize;
float4 _outlineColor;

float4 mainPixel(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
	//float
	float2 offsetX = float2(_texelSize.x, 0);
	float2 offsetY = { 0, _texelSize.y };
	float alpha = tex2D(s0, coords).a;
	alpha = max(alpha, tex2D(s0, coords + offsetX).a);
	alpha = max(alpha, tex2D(s0, coords - offsetX).a);
	alpha = max(alpha, tex2D(s0, coords + offsetY).a);
	alpha = max(alpha, tex2D(s0, coords - offsetY).a);

	float4 color = tex2D(s0, coords) * color1;
	if (alpha > color.a)
	{
		color.rgb = _outlineColor.rgb;
	}
	color.a = alpha;
	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 mainPixel();
	}
}