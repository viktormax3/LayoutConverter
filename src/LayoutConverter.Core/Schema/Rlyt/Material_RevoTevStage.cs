using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class Material_RevoTevStage
	{
		
		public Material_RevoTevStageColor color;

public Material_RevoTevStageAlpha alpha;

public Material_RevoTevStageIndirect indirect;

[XmlAttribute]
		public TevChannelID colorChannel;

[XmlAttribute]
		public sbyte texMap;

[XmlAttribute]
		public sbyte texCoordGen;

[XmlAttribute]
		public sbyte rasColSwap;

[XmlAttribute]
		public sbyte texColSwap;
	}
}
