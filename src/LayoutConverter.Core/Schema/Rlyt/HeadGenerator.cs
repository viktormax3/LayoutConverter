using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200007D RID: 125
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class HeadGenerator
	{
		// Token: 0x040002DD RID: 733
		[XmlAttribute]
		public string name;

		// Token: 0x040002DE RID: 734
		[XmlAttribute]
		public string version;
	}
}
