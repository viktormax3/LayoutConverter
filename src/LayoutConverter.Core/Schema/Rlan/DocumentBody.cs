using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x02000023 RID: 35
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[Serializable]
	public class DocumentBody
	{
		// Token: 0x040000BA RID: 186
		[XmlElement("animShare")]
		public AnimShare[] animShare;

		// Token: 0x040000BB RID: 187
		[XmlElement("animTag")]
		public AnimTag[] animTag;

		// Token: 0x040000BC RID: 188
		[XmlElement("rlan")]
		public RLAN[] rlan;
	}
}

