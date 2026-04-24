using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[Serializable]
	public class WhiteColor
	{
		
		public WhiteColor()
		{
			this.a = byte.MaxValue;
		}

public WhiteColor(byte r, byte g, byte b)
			: this()
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}

public WhiteColor(byte r, byte g, byte b, byte a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

[XmlAttribute]
		public byte r;

[XmlAttribute]
		public byte g;

[XmlAttribute]
		public byte b;

[DefaultValue(typeof(byte), "255")]
		[XmlAttribute]
		public byte a;
	}
}
