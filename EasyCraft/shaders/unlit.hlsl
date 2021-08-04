#define FOG_START_Y 140
#define FOG_END_Y 200

Texture2D _MainTex : register(t0);
SamplerState _MainTexSampler : register(s0);

struct VSInput {
    float4 position : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD;
    float3 normal : NORMAL;
};

struct PSInput
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
    float3 worldPosition : WORLD_POSITION;
    float2 uv : TEXCOORD;
};

cbuffer Transformations: register(b0)
{
    float4x4 modelTransform;
    float4x4 viewTransform;
    float4x4 projTransform;
};

PSInput VSMain(VSInput input)
{
    PSInput output;

    input.position.w = 1.0f;

    float4 viewpos = mul(mul(input.position, modelTransform), viewTransform);
    output.position = mul(viewpos, projTransform);
    output.worldPosition = output.position.xyz;

    output.color = input.color;
    output.uv = input.uv;

    return output;
}

cbuffer GlobalData: register(b0)
{
    float4 FogColor;
    float3 CameraPosition;
    float CameraNear;
    float CameraFar;
    float FogStart;
    float FogEnd;
    float Time;
};

float4 PSMain(PSInput input) : SV_TARGET
{
    float distCam = distance(CameraPosition, input.worldPosition);
    float distCamXZ = distance(float3(CameraPosition.x, input.worldPosition.y, CameraPosition.z), input.worldPosition);
    float distCamY = abs(CameraPosition.y - input.worldPosition.y);

    float4 color = _MainTex.Sample(_MainTexSampler, input.uv);

    color *= input.color;
    
    if (FogStart >= 0 && FogEnd > FogStart)
        color = lerp(color, FogColor, (clamp(distCamXZ, FogStart, FogEnd) - FogStart) / (FogEnd - FogStart));

    if (FOG_START_Y >= 0 && FOG_END_Y > FOG_START_Y)
        color = lerp(color, FogColor, (clamp(distCamY, FOG_START_Y, FOG_END_Y) - FOG_START_Y) / (FOG_END_Y - FOG_START_Y));

    return color;
}