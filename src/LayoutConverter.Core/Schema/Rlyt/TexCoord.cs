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
	public class TexCoord
	{
		
		public TexCoord()
		{
		}

public TexCoord(float s, float t)
		{
			this.texLT = new TexVec2(0f, 0f);
			this.texRT = new TexVec2(s, 0f);
			this.texLB = new TexVec2(0f, t);
			this.texRB = new TexVec2(s, t);
		}

public TexVec2 texLT;

public TexVec2 texRT;

public TexVec2 texLB;

public TexVec2 texRB;
	}
}
