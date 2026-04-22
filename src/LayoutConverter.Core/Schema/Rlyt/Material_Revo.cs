using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200004A RID: 74
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class Material_Revo
	{
		// Token: 0x0600004B RID: 75 RVA: 0x000027EC File Offset: 0x000017EC
		public Material_Revo()
		{
			this.tevStageNum = 1;
			this.indirectStageNum = 0;
			this.displayFace = DisplayFace.Both;
		}

		// Token: 0x0400014F RID: 335
		[XmlElement("channelControl")]
		public Material_RevoChannelControl[] channelControl;

		// Token: 0x04000150 RID: 336
		public Color4 matColReg;

		// Token: 0x04000151 RID: 337
		[XmlElement("tevColReg")]
		public ColorS10_4[] tevColReg;

		// Token: 0x04000152 RID: 338
		[XmlElement("tevConstReg")]
		public Color4[] tevConstReg;

		// Token: 0x04000153 RID: 339
		[XmlElement("texMap")]
		public TexMap[] texMap;

		// Token: 0x04000154 RID: 340
		[XmlElement("texMatrix")]
		public TexMatrix[] texMatrix;

		// Token: 0x04000155 RID: 341
		[XmlElement("texCoordGen")]
		public TexCoordGen[] texCoordGen;

		// Token: 0x04000156 RID: 342
		[XmlElement("swapTable")]
		public Material_RevoSwapTable[] swapTable;

		// Token: 0x04000157 RID: 343
		[XmlElement("indirectMatrix")]
		public TexMatrix[] indirectMatrix;

		// Token: 0x04000158 RID: 344
		[XmlElement("indirectStage")]
		public Material_RevoIndirectStage[] indirectStage;

		// Token: 0x04000159 RID: 345
		[XmlElement("tevStage")]
		public Material_RevoTevStage[] tevStage;

		// Token: 0x0400015A RID: 346
		public Material_RevoAlphaCompare alphaCompare;

		// Token: 0x0400015B RID: 347
		public Material_RevoBlendMode blendMode;

		// Token: 0x0400015C RID: 348
		[XmlAttribute]
		public string name;

		// Token: 0x0400015D RID: 349
		[XmlAttribute]
		[DefaultValue(typeof(byte), "1")]
		public byte tevStageNum;

		// Token: 0x0400015E RID: 350
		[DefaultValue(typeof(byte), "0")]
		[XmlAttribute]
		public byte indirectStageNum;

		// Token: 0x0400015F RID: 351
		[DefaultValue(DisplayFace.Both)]
		[XmlAttribute]
		public DisplayFace displayFace;
	}
}
