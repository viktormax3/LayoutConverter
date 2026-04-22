using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200002D RID: 45
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[Serializable]
	public class ScreenSettingGrid
	{
		// Token: 0x040000DE RID: 222
		public Color4 color;

		// Token: 0x040000DF RID: 223
		[XmlAttribute]
		public float thickLineInterval;

		// Token: 0x040000E0 RID: 224
		[XmlAttribute]
		public uint thinDivisionNum;

		// Token: 0x040000E1 RID: 225
		[XmlAttribute]
		public bool visible;

		// Token: 0x040000E2 RID: 226
		[XmlAttribute]
		public GridMoveMethod moveMethod;
	}
}
