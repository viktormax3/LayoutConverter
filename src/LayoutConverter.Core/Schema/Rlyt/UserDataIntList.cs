using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000034 RID: 52
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class UserDataIntList
	{
		// Token: 0x040000F3 RID: 243
		[XmlAttribute]
		public string name;

		// Token: 0x040000F4 RID: 244
		[XmlText]
		public string Value;
	}
}
