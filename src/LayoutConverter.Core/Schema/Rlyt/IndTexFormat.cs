using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200005F RID: 95
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum IndTexFormat
	{
		// Token: 0x04000213 RID: 531
		V8,
		// Token: 0x04000214 RID: 532
		V5,
		// Token: 0x04000215 RID: 533
		V4,
		// Token: 0x04000216 RID: 534
		V3
	}
}
