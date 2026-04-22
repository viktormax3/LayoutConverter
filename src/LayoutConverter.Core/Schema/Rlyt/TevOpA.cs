using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200005D RID: 93
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public enum TevOpA
	{
		// Token: 0x040001FF RID: 511
		Add,
		// Token: 0x04000200 RID: 512
		Sub,
		// Token: 0x04000201 RID: 513
		Comp_r8_gt,
		// Token: 0x04000202 RID: 514
		Comp_r8_eq,
		// Token: 0x04000203 RID: 515
		Comp_gr16_gt,
		// Token: 0x04000204 RID: 516
		Comp_gr16_eq,
		// Token: 0x04000205 RID: 517
		Comp_bgr24_gt,
		// Token: 0x04000206 RID: 518
		Comp_bgr24_eq,
		// Token: 0x04000207 RID: 519
		Comp_a8_gt,
		// Token: 0x04000208 RID: 520
		Comp_a8_eq
	}
}
