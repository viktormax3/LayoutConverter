using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x02000004 RID: 4
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[Serializable]
	public class HeadCreate
	{
		// Token: 0x04000005 RID: 5
		[XmlAttribute]
		public string user;

		// Token: 0x04000006 RID: 6
		[XmlAttribute]
		public string host;

		// Token: 0x04000007 RID: 7
		[XmlAttribute]
		public DateTime date;

		// Token: 0x04000008 RID: 8
		[XmlAttribute]
		public string source;
	}
}

