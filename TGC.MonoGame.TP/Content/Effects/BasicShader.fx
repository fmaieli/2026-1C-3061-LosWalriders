#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Custom Effects - https://docs.monogame.net/articles/content/custom_effects.html
// High-level shader language (HLSL) - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl
// Programming guide for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-pguide
// Reference for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-reference
// HLSL Semantics - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 DiffuseColor;
float UseVertexColor = 1.0f;

// Variables Iluminacion
float IsLightActive;
float3 LightPosition;
float3 LightDirection;
float3 LightColor;
float LightIntensity;
float LightRadius;
float IsSpotLight;
float SpotAngle;

// Texturas para habitaciones
texture WallTexture;
sampler2D WallSampler = sampler_state { 
    Texture = (WallTexture); 
    AddressU = Wrap; 
    AddressV = Wrap; 
};

texture FloorTexture;
sampler2D FloorSampler = sampler_state { 
    Texture = (FloorTexture);
    AddressU = Wrap; 
    AddressV = Wrap; 
};

// Repeticion de las imagenes para las texturas en suelo y paredes
float2 Tiling = float2(4.0f, 4.0f);

struct VertexShaderInput
{
	float4 Position : POSITION0;    
	float4 Color	: COLOR0;
    float3 Normal   : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color    : COLOR0;
    float3 Normal   : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
    float2 TexCoord : TEXCOORD2;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	// Clear the output
	VertexShaderOutput output = (VertexShaderOutput)0;
    // Model space to World space
    float4 worldPosition = mul(input.Position, World);
    // World space to View space
    float4 viewPosition = mul(worldPosition, View);	
	// View space to Projection space
    output.Position = mul(viewPosition, Projection);
		
	output.Color = (input.Color.a == 0) ? float4(1,1,1,1) : input.Color;
    output.WorldPos = worldPosition.xyz;
    output.TexCoord = input.TexCoord;
    output.Normal = normalize(mul(input.Normal, (float3x3)World));

	return output;
}

float4 MainPS(VertexShaderOutput output) : COLOR0
{
    // Coordenadas de textura del vertice multiplicado por el tiling 
    // Para que no se estiren de mas las imagenes
    float2 tiledUV = output.TexCoord * Tiling;  
    float4 wallColor = tex2D(WallSampler, tiledUV);
    float4 floorColor = tex2D(FloorSampler, tiledUV);

    // Si la normal apunta hacia arriba es el suelo
    float isFloor = step(0.5f, output.Normal.y);
    // Si la normal apunta hacia abajo es el techo
    float isCeiling = step(0.5f, -output.Normal.y);

    // Pinto la pared o suelo teniendo en cuenta las variables anteriores
    float4 surfaceColor = lerp(wallColor, floorColor, isFloor);
    // Pinto la superficie del techo con el valor que tiene el vertice
    surfaceColor = lerp(surfaceColor, output.Color, isCeiling); 
    // Pinto la superficie por DiffuseColor
    float3 surfaceRgb = lerp(DiffuseColor, surfaceColor.rgb * DiffuseColor, UseVertexColor);
    
    // Multiplico por 0.25 el color de la superficie para tener habitaciones oscuras
    float3 finalLighting = surfaceRgb * 0.25f; 

    // Si la linterna o el fosforo esta prendido (1.0f es encendido, 0.0f es apagado)
    if (IsLightActive > 0.5f)
    {
        // Direccion de la luz
        float3 lightDir = LightPosition - output.WorldPos;
        // Calculo la distancia
        float distance = length(lightDir);
        // Normal del vector calculado
        lightDir /= distance;

        // Perdida de luz segun la distancia 
        // saturate - Clamps x to the range [0, 1]
        float attenuation = saturate(1.0f - (distance / LightRadius));
        attenuation *= attenuation;

        // Lambertian reflectance
        // Producto punto entre normal de las superficies y direccion de la luz
        // Superficie perpendicular a la direccion de la luz brilla mas
        float diffuseMatch = max(dot(output.Normal, lightDir), 0.0f);

        // Linterna
        float spotEffect = 1.0f;
        if (IsSpotLight > 0.5f)
        {
            // Si los pixeles se encuentran dentro del cono de la linterna
            float apertureAngle = dot(-lightDir, normalize(LightDirection));
            // https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-smoothstep
            // Se difumina el borde de la luz mezclando los colores
            spotEffect = smoothstep(SpotAngle, SpotAngle + 0.05f, apertureAngle);
        }

        // Calculo la intensidad, orientacion de superficie, distancia y el haz de luz
        float3 appliedLight = LightColor * LightIntensity * diffuseMatch * attenuation * spotEffect;
        finalLighting += surfaceRgb * appliedLight;
    }

    return float4(finalLighting, 1.0f);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};