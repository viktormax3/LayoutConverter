using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200006E RID: 110
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public enum WindowFrameType
	{
		// Token: 0x0400027D RID: 637
		CornerLT,
		// Token: 0x0400027E RID: 638
		CornerLB,
		// Token: 0x0400027F RID: 639
		CornerRT,
		// Token: 0x04000280 RID: 640
		CornerRB,
		// Token: 0x04000281 RID: 641
		FrameL,
		// Token: 0x04000282 RID: 642
		FrameR,
		// Token: 0x04000283 RID: 643
		FrameT,
		// Token: 0x04000284 RID: 644
		FrameB
	}
}
