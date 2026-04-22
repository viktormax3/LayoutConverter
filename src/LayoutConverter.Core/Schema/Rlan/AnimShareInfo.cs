using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x0200001F RID: 31
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[Serializable]
	public class AnimShareInfo
	{
		// Token: 0x040000B0 RID: 176
		public string comment;

		// Token: 0x040000B1 RID: 177
		[XmlAttribute]
		public string srcPaneName;

		// Token: 0x040000B2 RID: 178
		[XmlAttribute]
		public string targetGroupName;
	}
}

