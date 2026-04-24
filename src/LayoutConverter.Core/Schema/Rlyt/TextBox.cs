using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[Serializable]
	public class TextBox
	{
		
		public TextBox()
		{
			this.charSpace = 0f;
			this.lineSpace = 0f;
			this.textAlignment = TextAlignment.Synchronous;
			this.binaryMaterialIndex = -1;
			this.binaryWrittenBytes = -1;
			this.binaryStoredBytes = -1;
		}

public Vec2 fontSize;

public string text;

public Color4 topColor;

public Color4 bottomColor;

public Position positionType;

public Material material;

public Material_Revo materialRevo;

[XmlAttribute]
		public string font;

[XmlAttribute]
		public uint allocateStringLength;

[XmlIgnore]
		public bool allocateStringLengthSpecified;

[XmlAttribute]
		[DefaultValue(typeof(float), "0")]
		public float charSpace;

[XmlAttribute]
		[DefaultValue(typeof(float), "0")]
		public float lineSpace;

[DefaultValue(TextAlignment.Synchronous)]
		[XmlAttribute]
		public TextAlignment textAlignment;

		[XmlAttribute]
		public int binaryMaterialIndex;

		[XmlIgnore]
		public bool binaryMaterialIndexSpecified;

		[XmlAttribute]
		public int binaryWrittenBytes;

		[XmlIgnore]
		public bool binaryWrittenBytesSpecified;

		[XmlAttribute]
		public int binaryStoredBytes;

		[XmlIgnore]
		public bool binaryStoredBytesSpecified;

[XmlIgnore]
		public MaterialInfo MaterialInfo;
	}
}
