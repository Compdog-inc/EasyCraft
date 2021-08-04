struct VSInput {
    float4 position : POSITION;
    float2 uv : TEXCOORD;
};

struct PSInput
{
    float4 position : SV_POSITION;
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
    output.uv = input.uv;

    return output;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    if ((input.uv.x % 1 <= 0.5f && input.uv.y % 1 <= 0.5f) || (input.uv.x % 1 >= 0.5f && input.uv.y % 1 >= 0.5f))
        return float4(0, 0, 0, 1);
    else
        return float4(1, 0, 1, 1);
}