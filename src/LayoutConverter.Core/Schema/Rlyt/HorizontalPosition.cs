using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000075 RID: 117
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum HorizontalPosition
	{
		// Token: 0x040002A8 RID: 680
		Left,
		// Token: 0x040002A9 RID: 681
		Center,
		// Token: 0x040002AA RID: 682
		Right
	}
}
