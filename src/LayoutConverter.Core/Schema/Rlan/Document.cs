using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x02000022 RID: 34
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[XmlRoot("nw4r_layout", Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor", IsNullable = false)]
	[DebuggerStepThrough]
	[Serializable]
	public class Document
	{
		// Token: 0x06000022 RID: 34 RVA: 0x000023A0 File Offset: 0x000013A0
		public Document()
		{
			this.version = "1.2.0";
		}

		// Token: 0x040000B7 RID: 183
		public Head head;

		// Token: 0x040000B8 RID: 184
		public DocumentBody body;

		// Token: 0x040000B9 RID: 185
		[XmlAttribute]
		public string version;
	}
}

