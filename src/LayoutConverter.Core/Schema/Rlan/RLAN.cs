using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class RLAN
	{
		
		public RLAN()
		{
			this.animLoop = AnimLoopType.Loop;
		}

[XmlElement("animContent")]
		public AnimContent[] animContent;

[XmlAttribute]
		public AnimationType animType;

[XmlAttribute]
		public int startFrame;

[XmlAttribute]
		public int endFrame;

[XmlAttribute]
		public int convertStartFrame;

[XmlAttribute]
		public int convertEndFrame;

[XmlAttribute]
		[DefaultValue(AnimLoopType.Loop)]
		public AnimLoopType animLoop;
	}
}

