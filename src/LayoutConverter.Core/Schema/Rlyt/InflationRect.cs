using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000039 RID: 57
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class InflationRect
	{
		// Token: 0x040000FA RID: 250
		[XmlAttribute]
		public float l;

		// Token: 0x040000FB RID: 251
		[XmlAttribute]
		public float r;

		// Token: 0x040000FC RID: 252
		[XmlAttribute]
		public float t;

		// Token: 0x040000FD RID: 253
		[XmlAttribute]
		public float b;
	}
}
