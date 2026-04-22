using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x02000020 RID: 32
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class AnimShare
	{
		// Token: 0x040000B3 RID: 179
		[XmlElement("targetTagName")]
		public string[] targetTagName;

		// Token: 0x040000B4 RID: 180
		[XmlElement("animShareInfo")]
		public AnimShareInfo[] animShareInfo;
	}
}

