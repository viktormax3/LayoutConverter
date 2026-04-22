using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000050 RID: 80
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum TevColorChannel
	{
		// Token: 0x04000171 RID: 369
		Red,
		// Token: 0x04000172 RID: 370
		Green,
		// Token: 0x04000173 RID: 371
		Blue,
		// Token: 0x04000174 RID: 372
		Alpha
	}
}
