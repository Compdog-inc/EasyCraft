struct VSInput {
    float4 position : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD;
};

struct PSInput
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD;
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

    float3 diffuse;

    if ((input.uv.x % 1 <= 0.5f && input.uv.y % 1 <= 0.5f) || (input.uv.x % 1 >= 0.5f && input.uv.y % 1 >= 0.5f))
        diffuse = float3(0, 0, 0);
    else
        diffuse = float3(1, 0, 1);

    output.Position = float4(input.worldPosition, 1.0f);
    output.Normal = float4(0, 0, 0, 1.0f);
    output.Diffuse = float4(diffuse, 1.0f);

    return output;
}