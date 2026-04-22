using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x0200001B RID: 27
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class UserDataFloatList
	{
		// Token: 0x04000096 RID: 150
		[XmlAttribute]
		public string name;

		// Token: 0x04000097 RID: 151
		[XmlText]
		public string Value;
	}
}

