using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000043 RID: 67
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[Serializable]
	public class TexCoordGen
	{
		// Token: 0x0400012C RID: 300
		[XmlAttribute]
		public TexGenType func;

		// Token: 0x0400012D RID: 301
		[XmlAttribute]
		public TexGenSrc srcParam;

		// Token: 0x0400012E RID: 302
		[XmlAttribute]
		public sbyte matrix;
	}
}
