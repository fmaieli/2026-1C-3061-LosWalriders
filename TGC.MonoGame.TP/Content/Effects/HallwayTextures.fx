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

// Repeticion de la textura
float2 Tiling = float2(0.01, 0.01);

// Textura para paredes
texture WallTexture;
sampler2D WallSampler = sampler_state
{
    Texture = (WallTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

// Textura para pisos
texture FloorTexture;
sampler2D FloorSampler = sampler_state
{
    Texture = (FloorTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

// Textura para techos
texture CeilingTexture;
sampler2D CeilingSampler = sampler_state
{
    Texture = (CeilingTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct WorldVertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct WorldVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 WorldPosition : TEXCOORD0;
    float4 Color : COLOR0;
};

WorldVertexShaderOutput WorldTilingVS(in WorldVertexShaderInput input)
{
    WorldVertexShaderOutput output;
    
    // Calculo WorldViewProjection
    float4x4 worldViewProjection = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, worldViewProjection);

    // Propagate the World Position with the Tiling applied (scaling them)
    output.WorldPosition = mul(input.Position, World);

    output.Color = input.Color;

    return output;
}

float4 WorldTilingPS(WorldVertexShaderOutput input) : COLOR
{
    // Genero la normal
    float3 dX = ddx(input.WorldPosition.xyz);
    float3 dY = ddy(input.WorldPosition.xyz);
    float3 normal = normalize(cross(dX, dY));

    // Get how parallel the normal of this point is to the X plane
    float xAlignment = abs(dot(normal, float3(1, 0, 0)));
    // Same for the Y plane
    float yAlignment = abs(dot(normal, float3(0, 1, 0)));

    // Use the world position as texture coordinates 
    // Choose which coordinates we will use based on our normal
    float2 yPlane = lerp(input.WorldPosition.xy, input.WorldPosition.xz, yAlignment);
    float2 resultPlane = lerp(yPlane, input.WorldPosition.yz, xAlignment);

    float2 textureCoordinates = resultPlane * Tiling;

    // Sample the textures using our scaled World Texture Coordinates
    // Se aplica para cada textura
    float4 wallColor = tex2D(WallSampler, textureCoordinates);
    float4 floorColor = tex2D(FloorSampler, textureCoordinates);
    float4 ceilingColor = tex2D(CeilingSampler, textureCoordinates);

    // Diferenciacion entre piso y techo
    float isFloor = step(0.5, dot(normal, float3(0, 1, 0)));
    float isCeiling = step(0.5, dot(normal, float3(0, -1, 0)));

    // Aplico color
    float4 finalColor = lerp(wallColor, floorColor, isFloor);
    finalColor = lerp(finalColor, ceilingColor, isCeiling);

    return finalColor;
}

technique WorldTiling
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL WorldTilingVS();
        PixelShader = compile PS_SHADERMODEL WorldTilingPS();
    }
};