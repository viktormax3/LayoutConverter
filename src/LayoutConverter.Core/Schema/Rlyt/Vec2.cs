using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class Vec2
	{
		
		public Vec2()
		{
		}

public Vec2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

[XmlAttribute]
		public float x;

[XmlAttribute]
		public float y;
	}
}
