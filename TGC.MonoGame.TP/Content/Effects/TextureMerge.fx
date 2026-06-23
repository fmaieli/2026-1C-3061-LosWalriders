#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float time;
float bloodIntensity; // Intensidad de la sangre
float grainIntensity; // Intensidad del noise (granulado filmico)

texture baseTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (baseTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture overlayTexture;
sampler2D overlayTextureSampler = sampler_state
{
	Texture = (overlayTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

    
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
};


VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = input.Position;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

// Calculo 'random' para generar el noise (granulado filmico)
float random(float2 uv, float t)
{
    // Gracias Patricio Gonzalez & Jen Lowe https://thebookofshaders.com/10/
    // frac(x) - fractional part f of x
    return frac(sin(dot(uv, float2(12.9898, 78.233)) + t) * 43758.5453);
}

float4 MergePS(VertexShaderOutput input) : COLOR
{
    float4 baseColor = tex2D(textureSampler, input.TextureCoordinates);
	float4 overlayColor = tex2D(overlayTextureSampler, input.TextureCoordinates);
    
    // Efecto de sangre
    // Para que la sangre 'palpite' con el tiempo
	float timeFactor = sin(time * 5.0) * 0.3 + 0.7; 
	float3 finalColor = lerp(baseColor.rgb, overlayColor.rgb, overlayColor.a * timeFactor * bloodIntensity);
    
    // Efecto de noise (granulado filmico)
    if (grainIntensity > 0.0)
    {
        // Estatica en el tiempo
        float noise = random(input.TextureCoordinates, time);
        
        // Se multiplica por 0.4 para que la pantalla no este totalmente negra
        finalColor.rgb -= noise * grainIntensity * 0.4; 
    }

	return float4(finalColor, 1.0);
}

technique Merge
{
    pass Pass0
    {
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MergePS();
	}
};