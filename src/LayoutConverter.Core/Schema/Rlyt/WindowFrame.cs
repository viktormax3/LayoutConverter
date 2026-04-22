using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200003A RID: 58
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class WindowFrame
	{
		// Token: 0x0600003D RID: 61 RVA: 0x00002610 File Offset: 0x00001610
		public WindowFrame()
		{
			this.detailSetting = false;
		}

		// Token: 0x040000FE RID: 254
		[XmlElement("textureFlip")]
		public TextureFlip[] textureFlip;

		// Token: 0x040000FF RID: 255
		public Material material;

		// Token: 0x04000100 RID: 256
		public Material_Revo materialRevo;

		// Token: 0x04000101 RID: 257
		[XmlAttribute]
		public WindowFrameType frameType;

		// Token: 0x04000102 RID: 258
		[XmlAttribute]
		[DefaultValue(false)]
		public bool detailSetting;

		// Token: 0x04000103 RID: 259
		[XmlIgnore]
		public MaterialInfo MaterialInfo;
	}
}
