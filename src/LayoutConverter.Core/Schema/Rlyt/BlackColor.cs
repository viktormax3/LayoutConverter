using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class BlackColor
	{
		
		public BlackColor()
		{
			this.a = 0;
		}

public BlackColor(byte r, byte g, byte b)
			: this()
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}

[XmlAttribute]
		public byte r;

[XmlAttribute]
		public byte g;

[XmlAttribute]
		public byte b;

[XmlAttribute]
		[DefaultValue(typeof(byte), "0")]
		public byte a;
	}
}
