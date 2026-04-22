using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000053 RID: 83
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[Serializable]
	public class Material_RevoTevStageColor
	{
		// Token: 0x04000181 RID: 385
		[XmlAttribute]
		public TevColorArg a;

		// Token: 0x04000182 RID: 386
		[XmlAttribute]
		public TevColorArg b;

		// Token: 0x04000183 RID: 387
		[XmlAttribute]
		public TevColorArg c;

		// Token: 0x04000184 RID: 388
		[XmlAttribute]
		public TevColorArg d;

		// Token: 0x04000185 RID: 389
		[XmlAttribute]
		public TevKColorSel konst;

		// Token: 0x04000186 RID: 390
		[XmlAttribute]
		public TevOpC op;

		// Token: 0x04000187 RID: 391
		[XmlAttribute]
		public TevBias bias;

		// Token: 0x04000188 RID: 392
		[XmlAttribute]
		public TevScale scale;

		// Token: 0x04000189 RID: 393
		[XmlAttribute]
		public bool clamp;

		// Token: 0x0400018A RID: 394
		[XmlAttribute]
		public TevRegID outReg;
	}
}
