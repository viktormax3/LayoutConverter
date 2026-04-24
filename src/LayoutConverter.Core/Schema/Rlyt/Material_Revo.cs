using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class Material_Revo
	{
		
		public Material_Revo()
		{
			this.tevStageNum = 1;
			this.indirectStageNum = 0;
			this.displayFace = DisplayFace.Both;
		}

[XmlElement("channelControl")]
		public Material_RevoChannelControl[] channelControl;

public Color4 matColReg;

[XmlElement("tevColReg")]
		public ColorS10_4[] tevColReg;

[XmlElement("tevConstReg")]
		public Color4[] tevConstReg;

[XmlElement("texMap")]
		public TexMap[] texMap;

[XmlElement("texMatrix")]
		public TexMatrix[] texMatrix;

[XmlElement("texCoordGen")]
		public TexCoordGen[] texCoordGen;

[XmlElement("swapTable")]
		public Material_RevoSwapTable[] swapTable;

[XmlElement("indirectMatrix")]
		public TexMatrix[] indirectMatrix;

[XmlElement("indirectStage")]
		public Material_RevoIndirectStage[] indirectStage;

[XmlElement("tevStage")]
		public Material_RevoTevStage[] tevStage;

public Material_RevoAlphaCompare alphaCompare;

public Material_RevoBlendMode blendMode;

[XmlAttribute]
		public string name;

[XmlAttribute]
		[DefaultValue(typeof(byte), "1")]
		public byte tevStageNum;

[DefaultValue(typeof(byte), "0")]
		[XmlAttribute]
		public byte indirectStageNum;

[DefaultValue(DisplayFace.Both)]
		[XmlAttribute]
		public DisplayFace displayFace;
	}
}
