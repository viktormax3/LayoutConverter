using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200004F RID: 79
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[Serializable]
	public class Material_RevoSwapTable
	{
		// Token: 0x0400016C RID: 364
		[XmlAttribute]
		public TevColorChannel r;

		// Token: 0x0400016D RID: 365
		[XmlAttribute]
		public TevColorChannel g;

		// Token: 0x0400016E RID: 366
		[XmlAttribute]
		public TevColorChannel b;

		// Token: 0x0400016F RID: 367
		[XmlAttribute]
		public TevColorChannel a;
	}
}
