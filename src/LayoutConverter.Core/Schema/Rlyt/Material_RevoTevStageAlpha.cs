using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200005A RID: 90
	[DebuggerStepThrough]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class Material_RevoTevStageAlpha
	{
		// Token: 0x040001D2 RID: 466
		[XmlAttribute]
		public TevAlphaArg a;

		// Token: 0x040001D3 RID: 467
		[XmlAttribute]
		public TevAlphaArg b;

		// Token: 0x040001D4 RID: 468
		[XmlAttribute]
		public TevAlphaArg c;

		// Token: 0x040001D5 RID: 469
		[XmlAttribute]
		public TevAlphaArg d;

		// Token: 0x040001D6 RID: 470
		[XmlAttribute]
		public TevKAlphaSel konst;

		// Token: 0x040001D7 RID: 471
		[XmlAttribute]
		public TevOpA op;

		// Token: 0x040001D8 RID: 472
		[XmlAttribute]
		public TevBias bias;

		// Token: 0x040001D9 RID: 473
		[XmlAttribute]
		public TevScale scale;

		// Token: 0x040001DA RID: 474
		[XmlAttribute]
		public bool clamp;

		// Token: 0x040001DB RID: 475
		[XmlAttribute]
		public TevRegID outReg;
	}
}
