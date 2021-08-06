Texture2D MainTexture : register(t0);
SamplerState MainTextureSampler : register(s0);

struct VSInput {
    float4 position : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
};

struct PSInput
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD;
    float3 normal : NORMALWS;
    float3 tangent : TANGENTWS;
    float3 worldPosition : POSITIONWS;
};

struct PSOutput {
    float4 Position : SV_Target0;
    float4 Normal : SV_Target1;
    float4 Diffuse : SV_Target2;
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

    float4x4 worldViewProj = mul(mul(modelTransform, viewTransform), projTransform);
    output.position = mul(input.position, worldViewProj);

    output.worldPosition = mul(input.position, modelTransform).xyz;

    output.normal = normalize(mul(input.normal, (float3x3)modelTransform));

    output.tangent = normalize(mul(input.tangent, (float3x3)modelTransform));

    output.uv = input.uv;
    output.color = input.color;

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

PSOutput PSMain(PSInput input)
{
    PSOutput output;

    float3 N = input.normal;
    float3 T = normalize(input.tangent - N * dot(input.tangent, N));
    float3 bitangent = cross(T, N);

    float3x3 tangentFrame = float3x3(normalize(input.tangent), normalize(bitangent), normalize(input.normal));

    float3 normal = float3(0.5f, 0.5f, 1.0f);
    normal = normalize(normal * 2.0f - 1.0f);

    float3 normalWS = mul(normal, tangentFrame);

    float3 diffuse = MainTexture.Sample(MainTextureSampler, input.uv).rgb;
    diffuse *= input.color.rbg;

    output.Position = float4(input.worldPosition, 1.0f);
    output.Normal = float4(normalWS, 1.0f);
    output.Diffuse = float4(diffuse, 1.0f);

    return output;
}