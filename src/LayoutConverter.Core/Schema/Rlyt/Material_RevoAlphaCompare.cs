using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[Serializable]
	public class Material_RevoAlphaCompare
	{
		
		[XmlAttribute]
		public Compare comp0;

[XmlAttribute]
		public byte ref0;

[XmlAttribute]
		public AlphaOp op;

[XmlAttribute]
		public Compare comp1;

[XmlAttribute]
		public byte ref1;
	}
}
