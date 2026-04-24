using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class Material_RevoIndirectStage
	{
		
		[XmlAttribute]
		public byte texMap;

[XmlAttribute]
		public byte texCoordGen;

[XmlAttribute]
		public IndTexScale scale_s;

[XmlAttribute]
		public IndTexScale scale_t;
	}
}
