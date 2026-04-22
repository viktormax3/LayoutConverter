using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200003B RID: 59
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum TextureFlip
	{
		// Token: 0x04000105 RID: 261
		None,
		// Token: 0x04000106 RID: 262
		FlipH,
		// Token: 0x04000107 RID: 263
		FlipV,
		// Token: 0x04000108 RID: 264
		Rotate90,
		// Token: 0x04000109 RID: 265
		Rotate180,
		// Token: 0x0400010A RID: 266
		Rotate270
	}
}
