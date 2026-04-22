using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x02000006 RID: 6
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum SlopeType
	{
		// Token: 0x0400000E RID: 14
		Fixed,
		// Token: 0x0400000F RID: 15
		Smooth,
		// Token: 0x04000010 RID: 16
		Clamped,
		// Token: 0x04000011 RID: 17
		Flat,
		// Token: 0x04000012 RID: 18
		Linear,
		// Token: 0x04000013 RID: 19
		Step
	}
}

