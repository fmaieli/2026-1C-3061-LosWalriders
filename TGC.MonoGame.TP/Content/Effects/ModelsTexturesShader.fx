#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 DiffuseColor;
Texture2D MainTexture;

// Variables Iluminacion
float IsLightActive;
float3 LightPosition;
float3 LightDirection;
float3 LightColor;
float LightIntensity;
float LightRadius;
float IsSpotLight;
float SpotAngle;

sampler2D TextureSampler = sampler_state
{
    Texture = (MainTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal   : TEXCOORD1;
    float3 WorldPos : TEXCOORD2;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    float4 worldPos = mul(input.Position, World);
    float4 viewPos = mul(worldPos, View);    
    output.Position = mul(viewPos, Projection);

    output.WorldPos = worldPos.xyz;
    output.TexCoord = input.TexCoord;
    // Roto la normal para que coincida con el mundo
    output.Normal = normalize(mul(input.Normal, (float3x3)World));
    
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float4 texColor = tex2D(TextureSampler, input.TexCoord);
    float3 baseColor = texColor.rgb * DiffuseColor;
    
    // Luz ambiente base
    float3 finalLighting = baseColor * 0.1f; 

    // Si la linterna o el fosforo esta prendido (1.0f encendido - 0.0f apagado)
    if (IsLightActive > 0.5f)
    {
        // Direccion de la luz y distancia
        float3 lightDir = LightPosition - input.WorldPos;
        float distance = length(lightDir);
        // Normal
        lightDir /= distance; 

        // Perdida de luz segun la distancia
        float attenuation = saturate(1.0f - (distance / LightRadius));
        attenuation *= attenuation;

        // Lambertian reflectance - caras que miran a la luz brillan mas
        float diffuseMatch = max(dot(input.Normal, lightDir), 0.0f);

        // Linterna
        float spotEffect = 1.0f;
        if (IsSpotLight > 0.5f)
        {
            //  Pixeles dentro del cono de la linterna
            float apertureAngle = dot(-lightDir, normalize(LightDirection));            
            // smoothstep suaviza los bordes de la luz de la linterna
            spotEffect = smoothstep(SpotAngle, SpotAngle + 0.05f, apertureAngle);
        }

        // Calculo con todo
        float3 appliedLight = LightColor * LightIntensity * diffuseMatch * attenuation * spotEffect;        
        finalLighting += baseColor * appliedLight;
    }

    return float4(finalLighting, texColor.a);
}

technique TexturedDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}