using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000048 RID: 72
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class MaterialWarp
	{
		// Token: 0x0600004A RID: 74 RVA: 0x000027BC File Offset: 0x000017BC
		public MaterialWarp()
		{
			this.scale_s = IndTexScale.V1;
			this.scale_t = IndTexScale.V1;
			this.signedOffsets = false;
			this.replaceMode = false;
		}

		// Token: 0x0400013E RID: 318
		[XmlAttribute]
		public byte texMap;

		// Token: 0x0400013F RID: 319
		[XmlAttribute]
		public byte texCoordGen;

		// Token: 0x04000140 RID: 320
		[DefaultValue(IndTexScale.V1)]
		[XmlAttribute]
		public IndTexScale scale_s;

		// Token: 0x04000141 RID: 321
		[XmlAttribute]
		[DefaultValue(IndTexScale.V1)]
		public IndTexScale scale_t;

		// Token: 0x04000142 RID: 322
		[DefaultValue(false)]
		[XmlAttribute]
		public bool signedOffsets;

		// Token: 0x04000143 RID: 323
		[XmlAttribute]
		[DefaultValue(false)]
		public bool replaceMode;

		// Token: 0x04000144 RID: 324
		[XmlAttribute]
		public byte matrix;
	}
}
