using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x0200001D RID: 29
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class UserDataString
	{
		// Token: 0x0400009A RID: 154
		[XmlAttribute]
		public string name;

		// Token: 0x0400009B RID: 155
		[XmlText]
		public string Value;
	}
}

