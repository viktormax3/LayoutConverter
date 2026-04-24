using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class Picture
	{
		
		public Picture()
		{
			this.detailSetting = false;
		}

public Picture(Material material)
			: this()
		{
			this.vtxColLT = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			this.vtxColRT = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			this.vtxColLB = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			this.vtxColRB = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			this.material = material;
		}

public Color4 vtxColLT;

public Color4 vtxColRT;

public Color4 vtxColLB;

public Color4 vtxColRB;

[XmlElement("texCoord")]
		public TexCoord[] texCoord;

public Material material;

public Material_Revo materialRevo;

[DefaultValue(false)]
		[XmlAttribute]
		public bool detailSetting;

[XmlIgnore]
		public MaterialInfo MaterialInfo;
	}
}
