using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class MaterialTextureStage
	{
		
		public MaterialTextureStage()
		{
			this.indirectStage = -1;
		}

[XmlAttribute]
		public sbyte texMap;

[XmlAttribute]
		public sbyte texCoordGen;

[XmlAttribute]
		[DefaultValue(typeof(sbyte), "-1")]
		public sbyte indirectStage;
	}
}
