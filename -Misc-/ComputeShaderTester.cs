using UnityEngine;

public class ComputeShaderTester : MonoBehaviour
{
	public ComputeShader shader;

	private void Start()
	{
		int num = 3;
		ComputeBuffer computeBuffer = new ComputeBuffer(num, 12);
		computeBuffer.SetData(new Vector3[3]
		{
			Vector3.zero,
			Vector3.one,
			new Vector3(3f, 2f, 1f)
		});
		int kernelIndex = shader.FindKernel("CSMain");
		shader.SetBuffer(kernelIndex, "output", computeBuffer);
		shader.SetFloat("vtest", 4f);
		shader.Dispatch(kernelIndex, num, 1, 1);
		Vector3[] array = new Vector3[num];
		computeBuffer.GetData(array);
		for (int i = 0; i < array.Length; i++)
		{
			MonoBehaviour.print(array[i]);
		}
	}

	private void Update()
	{
	}
}
