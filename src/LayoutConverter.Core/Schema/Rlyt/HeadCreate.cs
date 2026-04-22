using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000025 RID: 37
	[DebuggerStepThrough]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class HeadCreate
	{
		// Token: 0x040000C1 RID: 193
		[XmlAttribute]
		public string user;

		// Token: 0x040000C2 RID: 194
		[XmlAttribute]
		public string host;

		// Token: 0x040000C3 RID: 195
		[XmlAttribute]
		public DateTime date;

		// Token: 0x040000C4 RID: 196
		[XmlAttribute]
		public string source;
	}
}
