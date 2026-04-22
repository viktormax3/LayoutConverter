using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000028 RID: 40
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum TexelFormat
	{
		// Token: 0x040000C9 RID: 201
		I4,
		// Token: 0x040000CA RID: 202
		IA4,
		// Token: 0x040000CB RID: 203
		I8,
		// Token: 0x040000CC RID: 204
		IA8,
		// Token: 0x040000CD RID: 205
		RGB565,
		// Token: 0x040000CE RID: 206
		RGB5A3,
		// Token: 0x040000CF RID: 207
		RGBA8,
		// Token: 0x040000D0 RID: 208
		CMPR,
		// Token: 0x040000D1 RID: 209
		NW4R_TGA
	}
}
