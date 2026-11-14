using Godot;
using System;
using System.Collections.Generic;

// Builds a blacked-out sprite from a creature's actual artwork. All collection
// illustrations have a light backdrop (including two opaque JPGs), so a normal
// color-modulate would turn their *background* into a solid rectangle too.
// Flood-filling from the image edges removes that backdrop first.
public static class SilhouetteFactory
{
	private static readonly Dictionary<ulong, Texture2D> Cache = new();

	public static Texture2D Get(Texture2D original)
	{
		ulong key = original.GetInstanceId();
		if (Cache.TryGetValue(key, out Texture2D result))
			return result;

		Image image = original.GetImage();
		if (image.IsCompressed() && image.Decompress() != Error.Ok)
		{
			GD.PushWarning("Could not decompress a collection portrait for its silhouette.");
			Image hidden = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
			hidden.Fill(new Color("122031"));
			result = ImageTexture.CreateFromImage(hidden);
			Cache[key] = result;
			return result;
		}
		image.Convert(Image.Format.Rgba8);
		int width = image.GetWidth();
		int height = image.GetHeight();
		byte[] pixels = image.GetData();
		int count = width * height;
		bool[] outside = new bool[count];
		int[] queue = new int[count];
		int head = 0;
		int tail = 0;

		int[] corners = { 0, width - 1, count - width, count - 1 };
		int backgroundR = 0, backgroundG = 0, backgroundB = 0;
		foreach (int corner in corners)
		{
			int n = corner * 4;
			backgroundR += pixels[n];
			backgroundG += pixels[n + 1];
			backgroundB += pixels[n + 2];
		}
		backgroundR /= 4;
		backgroundG /= 4;
		backgroundB /= 4;

		bool IsBackdrop(int index)
		{
			int n = index * 4;
			if (pixels[n + 3] < 20)
				return true;
			int dr = pixels[n] - backgroundR;
			int dg = pixels[n + 1] - backgroundG;
			int db = pixels[n + 2] - backgroundB;
			return dr * dr + dg * dg + db * db < 3 * 27 * 27;
		}

		void Visit(int index)
		{
			if (outside[index] || !IsBackdrop(index))
				return;
			outside[index] = true;
			queue[tail++] = index;
		}

		for (int x = 0; x < width; x++)
		{
			Visit(x);
			Visit(count - width + x);
		}
		for (int y = 0; y < height; y++)
		{
			Visit(y * width);
			Visit(y * width + width - 1);
		}

		while (head < tail)
		{
			int p = queue[head++];
			int x = p % width;
			if (x > 0) Visit(p - 1);
			if (x < width - 1) Visit(p + 1);
			if (p >= width) Visit(p - width);
			if (p < count - width) Visit(p + width);
		}

		for (int i = 0; i < count; i++)
		{
			int n = i * 4;
			if (outside[i] || pixels[n + 3] < 20)
			{
				pixels[n + 3] = 0;
				continue;
			}
			pixels[n] = 18;
			pixels[n + 1] = 32;
			pixels[n + 2] = 49;
			pixels[n + 3] = 255;
		}

		Image silhouette = Image.CreateFromData(width, height, false, Image.Format.Rgba8, pixels);
		result = ImageTexture.CreateFromImage(silhouette);
		Cache[key] = result;
		return result;
	}
}
