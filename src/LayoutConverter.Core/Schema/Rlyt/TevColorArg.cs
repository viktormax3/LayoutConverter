using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000054 RID: 84
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum TevColorArg
	{
		// Token: 0x0400018C RID: 396
		C0,
		// Token: 0x0400018D RID: 397
		C1,
		// Token: 0x0400018E RID: 398
		C2,
		// Token: 0x0400018F RID: 399
		CPrev,
		// Token: 0x04000190 RID: 400
		A0,
		// Token: 0x04000191 RID: 401
		A1,
		// Token: 0x04000192 RID: 402
		A2,
		// Token: 0x04000193 RID: 403
		APrev,
		// Token: 0x04000194 RID: 404
		TexC,
		// Token: 0x04000195 RID: 405
		TexA,
		// Token: 0x04000196 RID: 406
		RasC,
		// Token: 0x04000197 RID: 407
		RasA,
		// Token: 0x04000198 RID: 408
		V1_0,
		// Token: 0x04000199 RID: 409
		V0_5,
		// Token: 0x0400019A RID: 410
		V0,
		// Token: 0x0400019B RID: 411
		Konst
	}
}
