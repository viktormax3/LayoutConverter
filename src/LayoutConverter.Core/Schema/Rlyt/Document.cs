using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200007E RID: 126
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[XmlRoot("nw4r_layout", Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor", IsNullable = false)]
	[Serializable]
	public class Document
	{
		// Token: 0x06000067 RID: 103 RVA: 0x00002C14 File Offset: 0x00001C14
		public Document()
		{
			this.version = "1.2.0";
		}

		// Token: 0x040002DF RID: 735
		public Head head;

		// Token: 0x040002E0 RID: 736
		public DocumentBody body;

		// Token: 0x040002E1 RID: 737
		[XmlAttribute]
		public string version;
	}
}
