using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200003C RID: 60
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[Serializable]
	public class Material
	{
		// Token: 0x0600003E RID: 62 RVA: 0x0000262C File Offset: 0x0000162C
		public Material()
		{
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00002640 File Offset: 0x00001640
		public Material(string name)
		{
			this.blackColor = new BlackColor(0, 0, 0);
			this.whiteColor = new WhiteColor(byte.MaxValue, byte.MaxValue, byte.MaxValue);
			this.name = name;
		}

		// Token: 0x0400010B RID: 267
		public BlackColor blackColor;

		// Token: 0x0400010C RID: 268
		public WhiteColor whiteColor;

		// Token: 0x0400010D RID: 269
		[XmlElement("texMap")]
		public TexMap[] texMap;

		// Token: 0x0400010E RID: 270
		[XmlElement("texMatrix")]
		public TexMatrix[] texMatrix;

		// Token: 0x0400010F RID: 271
		[XmlElement("texCoordGen")]
		public TexCoordGen[] texCoordGen;

		// Token: 0x04000110 RID: 272
		[XmlElement("textureStage")]
		public MaterialTextureStage[] textureStage;

		// Token: 0x04000111 RID: 273
		[XmlElement("texBlendRatio")]
		public TexBlendRatio[] texBlendRatio;

		// Token: 0x04000112 RID: 274
		[XmlArrayItem("warp", IsNullable = false)]
		public MaterialWarp[] indirectStages;

		// Token: 0x04000113 RID: 275
		[XmlAttribute]
		public string name;
	}
}
