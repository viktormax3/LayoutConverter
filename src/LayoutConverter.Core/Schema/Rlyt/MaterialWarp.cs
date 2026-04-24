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
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class MaterialWarp
	{
		
		public MaterialWarp()
		{
			this.scale_s = IndTexScale.V1;
			this.scale_t = IndTexScale.V1;
			this.signedOffsets = false;
			this.replaceMode = false;
		}

[XmlAttribute]
		public byte texMap;

[XmlAttribute]
		public byte texCoordGen;

[DefaultValue(IndTexScale.V1)]
		[XmlAttribute]
		public IndTexScale scale_s;

[XmlAttribute]
		[DefaultValue(IndTexScale.V1)]
		public IndTexScale scale_t;

[DefaultValue(false)]
		[XmlAttribute]
		public bool signedOffsets;

[XmlAttribute]
		[DefaultValue(false)]
		public bool replaceMode;

[XmlAttribute]
		public byte matrix;
	}
}
