using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x0200000D RID: 13
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public enum InfinityType
	{
		// Token: 0x04000078 RID: 120
		Constant,
		// Token: 0x04000079 RID: 121
		Cycle,
		// Token: 0x0400007A RID: 122
		Linear,
		// Token: 0x0400007B RID: 123
		CycleOffset,
		// Token: 0x0400007C RID: 124
		Oscillate
	}
}

