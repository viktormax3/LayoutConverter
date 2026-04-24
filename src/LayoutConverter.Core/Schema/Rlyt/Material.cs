using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[Serializable]
	public class Material
	{
		
		public Material()
		{
		}

public Material(string name)
		{
			this.blackColor = new BlackColor(0, 0, 0);
			this.whiteColor = new WhiteColor(byte.MaxValue, byte.MaxValue, byte.MaxValue);
			this.name = name;
		}

public BlackColor blackColor;

public WhiteColor whiteColor;

[XmlElement("texMap")]
		public TexMap[] texMap;

[XmlElement("texMatrix")]
		public TexMatrix[] texMatrix;

[XmlElement("texCoordGen")]
		public TexCoordGen[] texCoordGen;

[XmlElement("textureStage")]
		public MaterialTextureStage[] textureStage;

[XmlElement("texBlendRatio")]
		public TexBlendRatio[] texBlendRatio;

[XmlArrayItem("warp", IsNullable = false)]
		public MaterialWarp[] indirectStages;

[XmlAttribute]
		public string name;
	}
}
