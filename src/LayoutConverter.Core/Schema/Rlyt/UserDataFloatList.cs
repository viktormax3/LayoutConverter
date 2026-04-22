using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000033 RID: 51
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[Serializable]
	public class UserDataFloatList
	{
		// Token: 0x040000F1 RID: 241
		[XmlAttribute]
		public string name;

		// Token: 0x040000F2 RID: 242
		[XmlText]
		public string Value;
	}
}
