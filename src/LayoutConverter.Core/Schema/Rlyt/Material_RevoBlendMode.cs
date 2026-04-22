using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000068 RID: 104
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[Serializable]
	public class Material_RevoBlendMode
	{
		// Token: 0x0400024C RID: 588
		[XmlAttribute]
		public BlendMode type;

		// Token: 0x0400024D RID: 589
		[XmlAttribute]
		public BlendFactorSrc srcFactor;

		// Token: 0x0400024E RID: 590
		[XmlAttribute]
		public BlendFactorDst dstFactor;

		// Token: 0x0400024F RID: 591
		[XmlAttribute]
		public LogicOp op;
	}
}
