using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public static class Noise
{
	private static void FAST32_hash_3D(float3 gridcell, float3 v1_mask, float3 v2_mask, out float4 hash_0, out float4 hash_1, out float4 hash_2)
	{
		float2 @float = new float2(50f, 161f);
		float3 float2 = new float3(635.2987f, 682.3575f, 668.9265f);
		float3 float3 = new float3(48.50039f, 65.29412f, 63.9346f);
		gridcell.xyz -= math.floor(gridcell.xyz * (1f / 69f)) * 69f;
		float3 float4 = math.step(gridcell, new float3(67.5f, 67.5f, 67.5f)) * (gridcell + 1f);
		float4 float5 = new float4(gridcell.xy, float4.xy) + @float.xyxy;
		float5 *= float5;
		float4 float6 = math.lerp(float5.xyxy, float5.zwzw, new float4(v1_mask.xy, v2_mask.xy));
		float5 = new float4(float5.x, float6.xz, float5.z) * new float4(float5.y, float6.yw, float5.w);
		float3 float7 = 1f / (float2.xyz + gridcell.zzz * float3.xyz);
		float3 float8 = 1f / (float2.xyz + float4.zzz * float3.xyz);
		v1_mask = ((v1_mask.z < 0.5f) ? float7 : float8);
		v2_mask = ((v2_mask.z < 0.5f) ? float7 : float8);
		hash_0 = math.frac(float5 * new float4(float7.x, v1_mask.x, v2_mask.x, float8.x));
		hash_1 = math.frac(float5 * new float4(float7.y, v1_mask.y, v2_mask.y, float8.y));
		hash_2 = math.frac(float5 * new float4(float7.z, v1_mask.z, v2_mask.z, float8.z));
	}

	private static void Simplex3D_GetCornerVectors(float3 P, out float3 Pi, out float3 Pi_1, out float3 Pi_2, out float4 v1234_x, out float4 v1234_y, out float4 v1234_z)
	{
		P *= 0.70710677f;
		Pi = math.floor(P + math.dot(P, new float3(1f / 3f, 1f / 3f, 1f / 3f)));
		float3 @float = P - Pi + math.dot(Pi, new float3(1f / 6f, 1f / 6f, 1f / 6f));
		float3 float2 = math.step(@float.yzx, @float.xyz);
		float3 float3 = 1f - float2;
		Pi_1 = math.min(float2.xyz, float3.zxy);
		Pi_2 = math.max(float2.xyz, float3.zxy);
		float3 float4 = @float - Pi_1 + 1f / 6f;
		float3 float5 = @float - Pi_2 + 1f / 3f;
		float3 float6 = @float - 0.5f;
		v1234_x = new float4(@float.x, float4.x, float5.x, float6.x);
		v1234_y = new float4(@float.y, float4.y, float5.y, float6.y);
		v1234_z = new float4(@float.z, float4.z, float5.z, float6.z);
	}

	private static float4 Simplex3D_GetSurfletWeights(float4 v1234_x, float4 v1234_y, float4 v1234_z)
	{
		float4 @float = v1234_x * v1234_x + v1234_y * v1234_y + v1234_z * v1234_z;
		@float = math.max(0.5f - @float, 0f);
		return @float * @float * @float;
	}

	private static float SimplexPerlin3D(float3 P)
	{
		Simplex3D_GetCornerVectors(P, out var Pi, out var Pi_, out var Pi_2, out var v1234_x, out var v1234_y, out var v1234_z);
		FAST32_hash_3D(Pi, Pi_, Pi_2, out var hash_, out var hash_2, out var hash_3);
		hash_ -= 0.49999f;
		hash_2 -= 0.49999f;
		hash_3 -= 0.49999f;
		float4 y = math.rsqrt(hash_ * hash_ + hash_2 * hash_2 + hash_3 * hash_3) * (hash_ * v1234_x + hash_2 * v1234_y + hash_3 * v1234_z);
		return math.dot(Simplex3D_GetSurfletWeights(v1234_x, v1234_y, v1234_z), y) * 37.837227f;
	}

	public static float SimplexNormal(float3 p, int octaves, float3 offset, float frequency, float amplitude, float lacunarity, float persistence)
	{
		float num = 0f;
		for (int i = 0; i < octaves; i++)
		{
			float num2 = 0f;
			num2 = SimplexPerlin3D((p + offset) * frequency);
			num += num2 * amplitude;
			frequency *= lacunarity;
			amplitude *= persistence;
		}
		return num;
	}
}
