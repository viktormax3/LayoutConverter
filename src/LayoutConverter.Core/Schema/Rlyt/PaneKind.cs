using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200007B RID: 123
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum PaneKind
	{
		// Token: 0x040002D2 RID: 722
		Null,
		// Token: 0x040002D3 RID: 723
		Picture,
		// Token: 0x040002D4 RID: 724
		TextBox,
		// Token: 0x040002D5 RID: 725
		Window,
		// Token: 0x040002D6 RID: 726
		Bounding
	}
}
