using UnityEngine;

public class ColorEncoding
{
	public static byte ReverseBits(byte value)
	{
		return (byte)((value * 0x0202020202 & 0x010884422010) % 1023); 
	}

	public static int SparsifyBits(byte value, int sparse)
	{
		int retVal = 0;
		for (int bits = 0; bits < 8; bits++, value >>= 1)
		{
			retVal |= (value & 1);
			retVal <<= sparse;
		}
		return retVal >> sparse;
	}

	public static Color EncodeIDAsColor(int instanceId)
	{
		var uid = instanceId * 2;
		if (uid < 0)
			uid = -uid + 1;

		var sid =
			(SparsifyBits((byte)(uid >> 16), 3) << 2) |
			(SparsifyBits((byte)(uid >>  8), 3) << 1) |
			 SparsifyBits((byte)(uid      ), 3);
		//Debug.Log(uid + " >>> " + System.Convert.ToString(sid, 2).PadLeft(24, '0'));
	
		var r = (byte)(sid >> 8);
		var g = (byte)(sid >> 16);
		var b = (byte)(sid);
		
		//Debug.Log(r + " " + g + " " + b);
		return new Color32 (r, g, b, 255);
	}

	public static Color EncodeTagAsColor(string tag)
	{
		var hash = tag.GetHashCode();
		var a = (byte)(hash >> 24);
		var r = (byte)(hash >> 16);
		var g = (byte)(hash >> 8);
		var b = (byte)(hash);
		return new Color32 (r, g, b, a);
	}

	public static Color EncodeLayerAsColor(int layer)
	{
		var z = .7f;
		var uniqueColors = new Color[] {
			new Color(1,0,0,1), new Color(0,1,0,1), new Color(0,0,1,1), // 3
			new Color(1,1,0,1), new Color(1,0,1,1), new Color(0,1,1,1), // 6
			new Color(1,z,z,1), new Color(z,1,z,1), new Color(z,z,1,1), // 9
			new Color(1,1,z,1), new Color(1,z,1,1), new Color(z,1,1,1), // 12
			new Color(1,z,0,1), new Color(z,0,1,1), new Color(0,1,z,1), // 15
			new Color(1,1,1,1)											// 16
		};

		var color = uniqueColors[layer % uniqueColors.Length];
		var divider = 1.0f + Mathf.Floor(layer / uniqueColors.Length);
		color /= divider;

		return color;
	}
}
