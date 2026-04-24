using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class WindowContent
	{
		
		public WindowContent()
		{
			this.detailSetting = false;
		}

public Color4 vtxColLT;

public Color4 vtxColRT;

public Color4 vtxColLB;

public Color4 vtxColRB;

[XmlElement("texCoord")]
		public TexCoord[] texCoord;

public Material material;

public Material_Revo materialRevo;

[XmlAttribute]
		[DefaultValue(false)]
		public bool detailSetting;

[XmlIgnore]
		public MaterialInfo MaterialInfo;
	}
}
