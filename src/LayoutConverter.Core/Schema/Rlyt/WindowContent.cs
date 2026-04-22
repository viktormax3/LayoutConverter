using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200006F RID: 111
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class WindowContent
	{
		// Token: 0x06000057 RID: 87 RVA: 0x0000290C File Offset: 0x0000190C
		public WindowContent()
		{
			this.detailSetting = false;
		}

		// Token: 0x04000285 RID: 645
		public Color4 vtxColLT;

		// Token: 0x04000286 RID: 646
		public Color4 vtxColRT;

		// Token: 0x04000287 RID: 647
		public Color4 vtxColLB;

		// Token: 0x04000288 RID: 648
		public Color4 vtxColRB;

		// Token: 0x04000289 RID: 649
		[XmlElement("texCoord")]
		public TexCoord[] texCoord;

		// Token: 0x0400028A RID: 650
		public Material material;

		// Token: 0x0400028B RID: 651
		public Material_Revo materialRevo;

		// Token: 0x0400028C RID: 652
		[XmlAttribute]
		[DefaultValue(false)]
		public bool detailSetting;

		// Token: 0x0400028D RID: 653
		[XmlIgnore]
		public MaterialInfo MaterialInfo;
	}
}
