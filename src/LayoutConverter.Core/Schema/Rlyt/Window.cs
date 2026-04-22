using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000072 RID: 114
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class Window
	{
		// Token: 0x04000294 RID: 660
		public WindowContent content;

		// Token: 0x04000295 RID: 661
		[XmlElement("frame")]
		public WindowFrame[] frame;

		// Token: 0x04000296 RID: 662
		public InflationRect contentInflation;
	}
}
