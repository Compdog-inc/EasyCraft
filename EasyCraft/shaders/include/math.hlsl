float map(float v, float min1, float max1, float min2, float max2) {
	return (v - min1) / (max1 - min1) * (max2 - min2) + min2;
}

float2 map2(float2 v, float2 min1, float2 max1, float2 min2, float2 max2) {
	return float2(map(v.x, min1.x, max1.x, min2.x, max2.x), map(v.y, min1.y, max1.y, min2.y, max2.y));
}

float3 map3(float3 v, float3 min1, float3 max1, float3 min2, float3 max2) {
	return float3(map(v.x, min1.x, max1.x, min2.x, max2.x), map(v.y, min1.y, max1.y, min2.y, max2.y), map(v.z, min1.z, max1.z, min2.z, max2.z));
}

float4 map4(float4 v, float4 min1, float4 max1, float4 min2, float4 max2) {
	return float4(map(v.x, min1.x, max1.x, min2.x, max2.x), map(v.y, min1.y, max1.y, min2.y, max2.y), map(v.z, min1.z, max1.z, min2.z, max2.z), map(v.w, min1.w, max1.w, min2.w, max2.w));
}