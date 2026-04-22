using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x02000021 RID: 33
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[Serializable]
	public class HeadGenerator
	{
		// Token: 0x040000B5 RID: 181
		[XmlAttribute]
		public string name;

		// Token: 0x040000B6 RID: 182
		[XmlAttribute]
		public string version;
	}
}

