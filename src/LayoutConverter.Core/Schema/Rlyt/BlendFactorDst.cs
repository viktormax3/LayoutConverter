using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200006B RID: 107
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public enum BlendFactorDst
	{
		// Token: 0x0400025F RID: 607
		V0,
		// Token: 0x04000260 RID: 608
		V1_0,
		// Token: 0x04000261 RID: 609
		SrcClr,
		// Token: 0x04000262 RID: 610
		InvSrcClr,
		// Token: 0x04000263 RID: 611
		SrcAlpha,
		// Token: 0x04000264 RID: 612
		InvSrcAlpha,
		// Token: 0x04000265 RID: 613
		DstAlpha,
		// Token: 0x04000266 RID: 614
		InvDstAlpha
	}
}
