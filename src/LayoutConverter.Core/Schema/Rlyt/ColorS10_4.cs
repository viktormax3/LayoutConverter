using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200004E RID: 78
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[Serializable]
	public class ColorS10_4
	{
		// Token: 0x0600004D RID: 77 RVA: 0x00002828 File Offset: 0x00001828
		public ColorS10_4()
		{
		}

		// Token: 0x0600004E RID: 78 RVA: 0x0000283C File Offset: 0x0000183C
		public ColorS10_4(short r, short g, short b, short a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		// Token: 0x04000168 RID: 360
		[XmlAttribute]
		public short r;

		// Token: 0x04000169 RID: 361
		[XmlAttribute]
		public short g;

		// Token: 0x0400016A RID: 362
		[XmlAttribute]
		public short b;

		// Token: 0x0400016B RID: 363
		[XmlAttribute]
		public short a;
	}
}
