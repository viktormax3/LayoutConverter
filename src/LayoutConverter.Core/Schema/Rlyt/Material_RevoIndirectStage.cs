using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000051 RID: 81
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class Material_RevoIndirectStage
	{
		// Token: 0x04000175 RID: 373
		[XmlAttribute]
		public byte texMap;

		// Token: 0x04000176 RID: 374
		[XmlAttribute]
		public byte texCoordGen;

		// Token: 0x04000177 RID: 375
		[XmlAttribute]
		public IndTexScale scale_s;

		// Token: 0x04000178 RID: 376
		[XmlAttribute]
		public IndTexScale scale_t;
	}
}
