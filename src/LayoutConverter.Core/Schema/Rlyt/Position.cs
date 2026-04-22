using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000074 RID: 116
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class Position
	{
		// Token: 0x040002A5 RID: 677
		[XmlAttribute]
		public HorizontalPosition x;

		// Token: 0x040002A6 RID: 678
		[XmlAttribute]
		public VerticalPosition y;
	}
}
