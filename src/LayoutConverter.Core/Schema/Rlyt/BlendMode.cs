using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000069 RID: 105
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public enum BlendMode
	{
		// Token: 0x04000251 RID: 593
		None,
		// Token: 0x04000252 RID: 594
		Blend,
		// Token: 0x04000253 RID: 595
		Logic,
		// Token: 0x04000254 RID: 596
		Subtract
	}
}
