using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000073 RID: 115
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[Serializable]
	public class TextBox
	{
		// Token: 0x0600005D RID: 93 RVA: 0x000029E0 File Offset: 0x000019E0
		public TextBox()
		{
			this.charSpace = 0f;
			this.lineSpace = 0f;
			this.textAlignment = TextAlignment.Synchronous;
		}

		// Token: 0x04000297 RID: 663
		public Vec2 fontSize;

		// Token: 0x04000298 RID: 664
		public string text;

		// Token: 0x04000299 RID: 665
		public Color4 topColor;

		// Token: 0x0400029A RID: 666
		public Color4 bottomColor;

		// Token: 0x0400029B RID: 667
		public Position positionType;

		// Token: 0x0400029C RID: 668
		public Material material;

		// Token: 0x0400029D RID: 669
		public Material_Revo materialRevo;

		// Token: 0x0400029E RID: 670
		[XmlAttribute]
		public string font;

		// Token: 0x0400029F RID: 671
		[XmlAttribute]
		public uint allocateStringLength;

		// Token: 0x040002A0 RID: 672
		[XmlIgnore]
		public bool allocateStringLengthSpecified;

		// Token: 0x040002A1 RID: 673
		[XmlAttribute]
		[DefaultValue(typeof(float), "0")]
		public float charSpace;

		// Token: 0x040002A2 RID: 674
		[XmlAttribute]
		[DefaultValue(typeof(float), "0")]
		public float lineSpace;

		// Token: 0x040002A3 RID: 675
		[DefaultValue(TextAlignment.Synchronous)]
		[XmlAttribute]
		public TextAlignment textAlignment;

		// Token: 0x040002A4 RID: 676
		[XmlIgnore]
		public MaterialInfo MaterialInfo;
	}
}
