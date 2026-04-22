using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x0200001C RID: 28
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[Serializable]
	public class UserDataIntList
	{
		// Token: 0x04000098 RID: 152
		[XmlAttribute]
		public string name;

		// Token: 0x04000099 RID: 153
		[XmlText]
		public string Value;
	}
}

