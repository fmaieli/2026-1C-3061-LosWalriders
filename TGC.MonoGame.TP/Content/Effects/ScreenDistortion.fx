#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float Time;
float BlurFactor; // 1.0f ceguera - 0.0f vision normal
float2 ScreenResolution;

texture ScreenTexture;
sampler2D ScreenSampler = sampler_state
{
    Texture = (ScreenTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = input.Position;
    output.TexCoord = input.TexCoord;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float4 finalColor;

    if (BlurFactor <= 0.001f)
    {
        finalColor = tex2D(ScreenSampler, input.TexCoord);
    }
    else
    {
        float2 uv = input.TexCoord;

        // Ondas de distorcion en X e Y
        // Referencia https://www.shadertoy.com/view/lfKSzz - https://www.shadertoy.com/view/7ltBWl
        float wobbleX = sin(uv.y * 15.0f + Time * 5.0f) * 0.015f * BlurFactor;
        float wobbleY = cos(uv.x * 15.0f + Time * 5.0f) * 0.015f * BlurFactor;
        uv += float2(wobbleX, wobbleY);

        // https://grokipedia.com/page/Kawase_Blur#implementation
        // https://learnopengl.com/In-Practice/2D-Game/Postprocessing
        // Utilizo ScreenResolution para calcular
        float2 texel = float2(1.0f / ScreenResolution.x, 1.0f / ScreenResolution.y) * 12.0f * BlurFactor;
        
        // Gaussian Blur, tomo los 8 vecinos del pixel seleccionado
        float4 color = tex2D(ScreenSampler, uv);
        color += tex2D(ScreenSampler, uv + float2(-1, -1) * texel);
        color += tex2D(ScreenSampler, uv + float2( 0, -1) * texel);
        color += tex2D(ScreenSampler, uv + float2( 1, -1) * texel);
        color += tex2D(ScreenSampler, uv + float2(-1,  0) * texel);
        color += tex2D(ScreenSampler, uv + float2( 1,  0) * texel);
        color += tex2D(ScreenSampler, uv + float2(-1,  1) * texel);
        color += tex2D(ScreenSampler, uv + float2( 0,  1) * texel);
        color += tex2D(ScreenSampler, uv + float2( 1,  1) * texel);

        finalColor = color / 9.0f;
        
        // Oscurecimiento de pantalla
        finalColor.rgb *= (1.0f - BlurFactor * 0.25f);
    }

    return finalColor;
}

technique PanicDistortion
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}