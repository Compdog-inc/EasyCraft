Texture2DArray ChunkTextures : register(t0);
SamplerState ChunkTexturesSampler : register(s0);

struct VSInput {
    float4 position : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float id : ID;
};

struct PSInput
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD;
    float3 normal : NORMALWS;
    float3 tangent : TANGENTWS;
    float3 worldPosition : POSITIONWS;
    float id : ID;
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
    output.id = input.id;

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

float3 Blockframe(float3 color, float2 uv, float width) {
    if (uv.x % 1 <= width || uv.x % 1 >= 1 - width || uv.y % 1 <= width || uv.y % 1 >= 1 - width) {
        return float3(0,0,0);
    }
    return color;
}

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

    float3 diffuse;

    if (input.id < 0) {
        if ((input.uv.x % 1 <= 0.5f && input.uv.y % 1 <= 0.5f) || (input.uv.x % 1 >= 0.5f && input.uv.y % 1 >= 0.5f))
            diffuse = float3(0, 0, 0);
        else
            diffuse = float3(1, 0, 1);
    }
    else {
        diffuse = ChunkTextures.Sample(ChunkTexturesSampler, float3(input.uv, input.id)).rgb;
        diffuse *= input.color.rbg;

        //diffuse = Blockframe(diffuse, input.uv, 0.05);
    }

    output.Position = float4(input.worldPosition, 1.0f);
    output.Normal = float4(normalWS, 1.0f);
    output.Diffuse = float4(diffuse, 1.0f);

    return output;
}