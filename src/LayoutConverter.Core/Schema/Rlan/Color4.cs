using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x0200001A RID: 26
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class Color4
	{
		// Token: 0x04000092 RID: 146
		[XmlAttribute]
		public byte r;

		// Token: 0x04000093 RID: 147
		[XmlAttribute]
		public byte g;

		// Token: 0x04000094 RID: 148
		[XmlAttribute]
		public byte b;

		// Token: 0x04000095 RID: 149
		[XmlAttribute]
		public byte a;
	}
}

