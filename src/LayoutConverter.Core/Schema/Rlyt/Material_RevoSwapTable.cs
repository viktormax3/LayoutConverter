using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[Serializable]
	public class Material_RevoSwapTable
	{
		
		[XmlAttribute]
		public TevColorChannel r;

[XmlAttribute]
		public TevColorChannel g;

[XmlAttribute]
		public TevColorChannel b;

[XmlAttribute]
		public TevColorChannel a;
	}
}
