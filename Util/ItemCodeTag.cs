using System.Runtime.InteropServices;
using QFSW.QC;

namespace Util;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ItemCodeTag : IQcSuggestorTag
{
}
