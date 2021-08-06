#define MODE_DEFAULT 0
#define MODE_DIFFUSE 1
#define MODE_NORMAL 2
#define MODE_WORLDPOS 3

struct PSInput
{
    float4 position : SV_POSITION;
};


PSInput VSMain(uint id : SV_VertexID)
{
    PSInput output;

    float2 uv = float2((id << 1) & 2, id & 2);

    output.position = float4(uv, 0, 1);
    output.position.x = output.position.x * 2 - 1;
    output.position.y = output.position.y * -2 + 1;

    return output;
}

Texture2D positionGB : register(t0);
Texture2D normalGB : register(t1);
Texture2D diffuseGB : register(t2);

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

cbuffer PassData: register(b1)
{
    uint PassOrder;
    uint Mode;
};

float4 PSMain(PSInput input) : SV_TARGET
{
    int3 sampleIndices = int3(input.position.xy, 0);

    float3 normal = normalGB.Load(sampleIndices).xyz;

    float3 position = positionGB.Load(sampleIndices).xyz;

    float4 diffuseR = diffuseGB.Load(sampleIndices);
    float3 diffuse = diffuseR.xyz;

    float3 lightColor = float3(0.5f, 0.5f, 0.5f);
    float3 lightDir = float3(10.0f, -10.0f, 0.0f);

    float3 L = -lightDir;

    float lightAmountDL = saturate(dot(normal, L));
    float3 color = lightColor * lightAmountDL * diffuse;

    switch (Mode) {
    case MODE_DIFFUSE:
        color = diffuse;
        break;
    case MODE_NORMAL:
        color = normal * 0.5f + 0.5f;
        break;
    case MODE_WORLDPOS:
        color = normalize(position);
        break;
    }

    // Skybox
    color = lerp(color, FogColor, diffuseR.w);
    return float4(color, 1.0f);
}