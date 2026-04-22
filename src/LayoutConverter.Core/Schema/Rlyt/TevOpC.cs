using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000056 RID: 86
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum TevOpC
	{
		// Token: 0x040001BA RID: 442
		Add,
		// Token: 0x040001BB RID: 443
		Sub,
		// Token: 0x040001BC RID: 444
		Comp_r8_gt,
		// Token: 0x040001BD RID: 445
		Comp_r8_eq,
		// Token: 0x040001BE RID: 446
		Comp_gr16_gt,
		// Token: 0x040001BF RID: 447
		Comp_gr16_eq,
		// Token: 0x040001C0 RID: 448
		Comp_bgr24_gt,
		// Token: 0x040001C1 RID: 449
		Comp_bgr24_eq,
		// Token: 0x040001C2 RID: 450
		Comp_rgb8_gt,
		// Token: 0x040001C3 RID: 451
		Comp_rgb8_eq
	}
}
