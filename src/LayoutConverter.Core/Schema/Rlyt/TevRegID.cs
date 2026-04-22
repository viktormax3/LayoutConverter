using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000059 RID: 89
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum TevRegID
	{
		// Token: 0x040001CE RID: 462
		Reg0,
		// Token: 0x040001CF RID: 463
		Reg1,
		// Token: 0x040001D0 RID: 464
		Reg2,
		// Token: 0x040001D1 RID: 465
		Prev
	}
}
