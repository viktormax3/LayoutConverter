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
	public class ColorS10_4
	{
		
		public ColorS10_4()
		{
		}

public ColorS10_4(short r, short g, short b, short a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

[XmlAttribute]
		public short r;

[XmlAttribute]
		public short g;

[XmlAttribute]
		public short b;

[XmlAttribute]
		public short a;
	}
}
