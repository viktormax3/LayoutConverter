using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x02000003 RID: 3
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[XmlRoot("head", Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor", IsNullable = false)]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class Head
	{
		// Token: 0x04000001 RID: 1
		public HeadCreate create;

		// Token: 0x04000002 RID: 2
		public string title;

		// Token: 0x04000003 RID: 3
		public string comment;

		// Token: 0x04000004 RID: 4
		public HeadGenerator generator;
	}
}

