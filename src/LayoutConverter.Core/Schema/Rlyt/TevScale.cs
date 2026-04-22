using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000058 RID: 88
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum TevScale
	{
		// Token: 0x040001C9 RID: 457
		V1,
		// Token: 0x040001CA RID: 458
		V2,
		// Token: 0x040001CB RID: 459
		V4,
		// Token: 0x040001CC RID: 460
		V1_2
	}
}
