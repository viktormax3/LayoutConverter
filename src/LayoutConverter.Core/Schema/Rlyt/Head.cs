using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000024 RID: 36
	[XmlRoot("head", Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor", IsNullable = false)]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class Head
	{
		// Token: 0x040000BD RID: 189
		public HeadCreate create;

		// Token: 0x040000BE RID: 190
		public string title;

		// Token: 0x040000BF RID: 191
		public string comment;

		// Token: 0x040000C0 RID: 192
		public HeadGenerator generator;
	}
}
