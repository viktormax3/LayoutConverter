using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000062 RID: 98
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public enum IndTexWrap
	{
		// Token: 0x0400022C RID: 556
		Off,
		// Token: 0x0400022D RID: 557
		V256,
		// Token: 0x0400022E RID: 558
		V128,
		// Token: 0x0400022F RID: 559
		V64,
		// Token: 0x04000230 RID: 560
		V32,
		// Token: 0x04000231 RID: 561
		V16,
		// Token: 0x04000232 RID: 562
		V0
	}
}
