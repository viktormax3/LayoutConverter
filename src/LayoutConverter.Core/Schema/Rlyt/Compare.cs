using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000066 RID: 102
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public enum Compare
	{
		// Token: 0x0400023F RID: 575
		Never,
		// Token: 0x04000240 RID: 576
		Less,
		// Token: 0x04000241 RID: 577
		LEqual,
		// Token: 0x04000242 RID: 578
		Equal,
		// Token: 0x04000243 RID: 579
		NEqual,
		// Token: 0x04000244 RID: 580
		GEqual,
		// Token: 0x04000245 RID: 581
		Greater,
		// Token: 0x04000246 RID: 582
		Always
	}
}
