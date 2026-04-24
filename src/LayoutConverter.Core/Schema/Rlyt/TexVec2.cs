using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[Serializable]
	public class TexVec2
	{
		
		public TexVec2()
		{
		}

public TexVec2(float s, float t)
		{
			this.s = s;
			this.t = t;
		}

[XmlAttribute]
		public float s;

[XmlAttribute]
		public float t;
	}
}
