using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000035 RID: 53
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class UserDataString
	{
		// Token: 0x040000F5 RID: 245
		[XmlAttribute]
		public string name;

		// Token: 0x040000F6 RID: 246
		[XmlText]
		public string Value;
	}
}
