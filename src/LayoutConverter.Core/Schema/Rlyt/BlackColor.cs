using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200003D RID: 61
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class BlackColor
	{
		// Token: 0x06000040 RID: 64 RVA: 0x00002684 File Offset: 0x00001684
		public BlackColor()
		{
			this.a = 0;
		}

		// Token: 0x06000041 RID: 65 RVA: 0x000026A0 File Offset: 0x000016A0
		public BlackColor(byte r, byte g, byte b)
			: this()
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}

		// Token: 0x04000114 RID: 276
		[XmlAttribute]
		public byte r;

		// Token: 0x04000115 RID: 277
		[XmlAttribute]
		public byte g;

		// Token: 0x04000116 RID: 278
		[XmlAttribute]
		public byte b;

		// Token: 0x04000117 RID: 279
		[XmlAttribute]
		[DefaultValue(typeof(byte), "0")]
		public byte a;
	}
}
