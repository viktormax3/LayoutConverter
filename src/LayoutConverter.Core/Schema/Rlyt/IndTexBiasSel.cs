using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000060 RID: 96
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum IndTexBiasSel
	{
		// Token: 0x04000218 RID: 536
		None,
		// Token: 0x04000219 RID: 537
		S,
		// Token: 0x0400021A RID: 538
		T,
		// Token: 0x0400021B RID: 539
		U,
		// Token: 0x0400021C RID: 540
		ST,
		// Token: 0x0400021D RID: 541
		SU,
		// Token: 0x0400021E RID: 542
		TU,
		// Token: 0x0400021F RID: 543
		STU
	}
}
