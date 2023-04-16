using UnityEngine;

public static class TextureGenerator
{
	public static Texture2D Generate(Material material, int width = 128, int height = 128)
	{
		RenderTexture renderTexture = new RenderTexture(width, height, 24);
		Graphics.Blit(Texture2D.blackTexture, renderTexture, material);
		RenderTexture.active = renderTexture;
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: true);
		texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
		texture2D.Apply();
		return texture2D;
	}
}
