using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000042 RID: 66
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[Serializable]
	public class TexMatrix
	{
		// Token: 0x04000129 RID: 297
		public Vec2 scale;

		// Token: 0x0400012A RID: 298
		public Vec2 translate;

		// Token: 0x0400012B RID: 299
		[XmlAttribute]
		public float rotate;
	}
}
