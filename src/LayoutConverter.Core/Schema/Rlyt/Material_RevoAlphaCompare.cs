using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000065 RID: 101
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[Serializable]
	public class Material_RevoAlphaCompare
	{
		// Token: 0x04000239 RID: 569
		[XmlAttribute]
		public Compare comp0;

		// Token: 0x0400023A RID: 570
		[XmlAttribute]
		public byte ref0;

		// Token: 0x0400023B RID: 571
		[XmlAttribute]
		public AlphaOp op;

		// Token: 0x0400023C RID: 572
		[XmlAttribute]
		public Compare comp1;

		// Token: 0x0400023D RID: 573
		[XmlAttribute]
		public byte ref1;
	}
}
