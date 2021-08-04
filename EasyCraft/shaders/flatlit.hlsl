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
    float4 viewPosition : VIEWPOS;
    float3 viewNormal : VIEWNORMAL;
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

    output.viewPosition = mul(mul(input.position, modelTransform), viewTransform);
    output.position = mul(output.viewPosition, projTransform);
    output.worldPosition = output.position.xyz;

    float3x3 normalMatrix = (float3x3)mul(modelTransform, viewTransform);
    output.viewNormal = mul(input.normal.xyz, normalMatrix);

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

cbuffer LightSource: register(b1)
{
    float4 lightViewPosition;
    float4 lightColor;
};

float4 BlinnPhong(PSInput input, float4 color, float4 specularShine, float4 ambient) {
    float3 l = lightViewPosition.xyz - input.viewPosition.xyz;
    float d_sq = dot(l, l);
    l = normalize(l);

    float3 n = normalize(input.viewNormal);
    float3 v = normalize(-input.viewPosition.xyz);
    float nDotL = dot(n, l);

    float lightPower = lightColor.w / d_sq;
    float lambertian = max(nDotL, 0.0);
    float3 diffuseColor = color.rgb * lightColor.rgb;

    float3 h = normalize(l + v);
    float specular = nDotL > 0.0 ? dot(h, n) : 0.0;
    if (specular > 0.0)
    {
        specular = pow(specular, specularShine.w);
    }
    float3 specularColor = specularShine.rgb * lightColor.rgb;

    return float4(ambient.rgb
        + diffuseColor * lambertian * lightPower
        + specularColor * specular * lightPower, color.w);
}

float4 PSMain(PSInput input) : SV_TARGET
{
    float distCam = distance(CameraPosition, input.worldPosition);
    float distCamXZ = distance(float3(CameraPosition.x, input.worldPosition.y, CameraPosition.z), input.worldPosition);
    float distCamY = abs(CameraPosition.y - input.worldPosition.y);

    float4 color = _MainTex.Sample(_MainTexSampler, input.uv);

    color *= input.color;

    float4 materialAmbientColor = float4(0.2, 0.1, 0.5, 1);
    float4 materialSpecular = float4(1, 1, 1, 0.5);

    color = BlinnPhong(input, color, materialSpecular, materialAmbientColor);

    if (FogStart >= 0 && FogEnd > FogStart)
        color = lerp(color, FogColor, (clamp(distCamXZ, FogStart, FogEnd) - FogStart) / (FogEnd - FogStart));

    if (FOG_START_Y >= 0 && FOG_END_Y > FOG_START_Y)
        color = lerp(color, FogColor, (clamp(distCamY, FOG_START_Y, FOG_END_Y) - FOG_START_Y) / (FOG_END_Y - FOG_START_Y));

    return color;
}