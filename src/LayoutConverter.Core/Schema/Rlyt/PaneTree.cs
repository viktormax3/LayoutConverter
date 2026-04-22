using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000037 RID: 55
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[XmlRoot("paneTree", Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor", IsNullable = false)]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[Serializable]
	public class PaneTree
	{
		// Token: 0x040000F8 RID: 248
		[XmlElement("paneTree")]
		public PaneTree[] paneTree;

		// Token: 0x040000F9 RID: 249
		[XmlAttribute]
		public string name;
	}
}
