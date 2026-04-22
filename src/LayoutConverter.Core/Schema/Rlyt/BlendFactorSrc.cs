using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200006A RID: 106
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public enum BlendFactorSrc
	{
		// Token: 0x04000256 RID: 598
		V0,
		// Token: 0x04000257 RID: 599
		V1_0,
		// Token: 0x04000258 RID: 600
		DstClr,
		// Token: 0x04000259 RID: 601
		InvDstClr,
		// Token: 0x0400025A RID: 602
		SrcAlpha,
		// Token: 0x0400025B RID: 603
		InvSrcAlpha,
		// Token: 0x0400025C RID: 604
		DstAlpha,
		// Token: 0x0400025D RID: 605
		InvDstAlpha
	}
}
