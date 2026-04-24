using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class Color4
	{
		
		[XmlAttribute]
		public byte r;

[XmlAttribute]
		public byte g;

[XmlAttribute]
		public byte b;

[XmlAttribute]
		public byte a;
	}
}

