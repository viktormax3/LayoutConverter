using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000077 RID: 119
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum TextAlignment
	{
		// Token: 0x040002B0 RID: 688
		Synchronous,
		// Token: 0x040002B1 RID: 689
		Left,
		// Token: 0x040002B2 RID: 690
		Center,
		// Token: 0x040002B3 RID: 691
		Right
	}
}
