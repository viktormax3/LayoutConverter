using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200002B RID: 43
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class ScreenSettingBackGround
	{
		// Token: 0x040000D8 RID: 216
		public Color4 color;

		// Token: 0x040000D9 RID: 217
		[XmlAttribute]
		public string imageFile;
	}
}
