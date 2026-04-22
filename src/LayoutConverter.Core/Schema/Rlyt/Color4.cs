using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200002C RID: 44
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class Color4
	{
		// Token: 0x06000030 RID: 48 RVA: 0x000024F0 File Offset: 0x000014F0
		public Color4()
		{
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00002504 File Offset: 0x00001504
		public Color4(byte r, byte g, byte b, byte a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		// Token: 0x040000DA RID: 218
		[XmlAttribute]
		public byte r;

		// Token: 0x040000DB RID: 219
		[XmlAttribute]
		public byte g;

		// Token: 0x040000DC RID: 220
		[XmlAttribute]
		public byte b;

		// Token: 0x040000DD RID: 221
		[XmlAttribute]
		public byte a;
	}
}
