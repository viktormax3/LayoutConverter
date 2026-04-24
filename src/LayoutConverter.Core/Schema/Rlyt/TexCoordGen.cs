using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[Serializable]
	public class TexCoordGen
	{
		
		[XmlAttribute]
		public TexGenType func;

[XmlAttribute]
		public TexGenSrc srcParam;

[XmlAttribute]
		public sbyte matrix;
	}
}
