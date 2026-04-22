using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200005E RID: 94
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[Serializable]
	public class Material_RevoTevStageIndirect
	{
		// Token: 0x04000209 RID: 521
		[XmlAttribute]
		public byte indStage;

		// Token: 0x0400020A RID: 522
		[XmlAttribute]
		public IndTexFormat format;

		// Token: 0x0400020B RID: 523
		[XmlAttribute]
		public IndTexBiasSel bias;

		// Token: 0x0400020C RID: 524
		[XmlAttribute]
		public IndTexMtxID matrix;

		// Token: 0x0400020D RID: 525
		[XmlAttribute]
		public IndTexWrap wrap_s;

		// Token: 0x0400020E RID: 526
		[XmlAttribute]
		public IndTexWrap wrap_t;

		// Token: 0x0400020F RID: 527
		[XmlAttribute]
		public bool addPrev;

		// Token: 0x04000210 RID: 528
		[XmlAttribute]
		public bool utcLod;

		// Token: 0x04000211 RID: 529
		[XmlAttribute]
		public IndTexAlphaSel alpha;
	}
}
