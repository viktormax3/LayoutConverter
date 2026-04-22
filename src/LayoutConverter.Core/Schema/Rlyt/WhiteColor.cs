using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200003E RID: 62
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[Serializable]
	public class WhiteColor
	{
		// Token: 0x06000042 RID: 66 RVA: 0x000026C8 File Offset: 0x000016C8
		public WhiteColor()
		{
			this.a = byte.MaxValue;
		}

		// Token: 0x06000043 RID: 67 RVA: 0x000026E8 File Offset: 0x000016E8
		public WhiteColor(byte r, byte g, byte b)
			: this()
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00002710 File Offset: 0x00001710
		public WhiteColor(byte r, byte g, byte b, byte a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		// Token: 0x04000118 RID: 280
		[XmlAttribute]
		public byte r;

		// Token: 0x04000119 RID: 281
		[XmlAttribute]
		public byte g;

		// Token: 0x0400011A RID: 282
		[XmlAttribute]
		public byte b;

		// Token: 0x0400011B RID: 283
		[DefaultValue(typeof(byte), "255")]
		[XmlAttribute]
		public byte a;
	}
}
