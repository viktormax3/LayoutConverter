using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class WindowFrame
	{
		
		public WindowFrame()
		{
			this.detailSetting = false;
		}

[XmlElement("textureFlip")]
		public TextureFlip[] textureFlip;

public Material material;

public Material_Revo materialRevo;

[XmlAttribute]
		public WindowFrameType frameType;

[XmlAttribute]
		[DefaultValue(false)]
		public bool detailSetting;

[XmlIgnore]
		public MaterialInfo MaterialInfo;
	}
}
