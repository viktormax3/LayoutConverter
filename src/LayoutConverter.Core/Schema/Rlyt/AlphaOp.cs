using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000067 RID: 103
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public enum AlphaOp
	{
		// Token: 0x04000248 RID: 584
		And,
		// Token: 0x04000249 RID: 585
		Or,
		// Token: 0x0400024A RID: 586
		Xor,
		// Token: 0x0400024B RID: 587
		Xnor
	}
}
