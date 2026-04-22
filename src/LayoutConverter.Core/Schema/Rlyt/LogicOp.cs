using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200006C RID: 108
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public enum LogicOp
	{
		// Token: 0x04000268 RID: 616
		Clear,
		// Token: 0x04000269 RID: 617
		Set,
		// Token: 0x0400026A RID: 618
		Copy,
		// Token: 0x0400026B RID: 619
		InvCopy,
		// Token: 0x0400026C RID: 620
		NoOp,
		// Token: 0x0400026D RID: 621
		Inv,
		// Token: 0x0400026E RID: 622
		And,
		// Token: 0x0400026F RID: 623
		Nand,
		// Token: 0x04000270 RID: 624
		Or,
		// Token: 0x04000271 RID: 625
		Nor,
		// Token: 0x04000272 RID: 626
		Xor,
		// Token: 0x04000273 RID: 627
		Equiv,
		// Token: 0x04000274 RID: 628
		RevAnd,
		// Token: 0x04000275 RID: 629
		InvAnd,
		// Token: 0x04000276 RID: 630
		RevOr,
		// Token: 0x04000277 RID: 631
		InvOr
	}
}
