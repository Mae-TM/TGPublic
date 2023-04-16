using System.Runtime.InteropServices;

namespace Lidgren.Network;

[StructLayout(LayoutKind.Explicit)]
public struct SingleUIntUnion
{
	[FieldOffset(0)]
	public float SingleValue;

	[FieldOffset(0)]
	public uint UIntValue;
}
