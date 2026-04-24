using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[Serializable]
	public class Material_RevoTevStageColor
	{
		
		[XmlAttribute]
		public TevColorArg a;

[XmlAttribute]
		public TevColorArg b;

[XmlAttribute]
		public TevColorArg c;

[XmlAttribute]
		public TevColorArg d;

[XmlAttribute]
		public TevKColorSel konst;

[XmlAttribute]
		public TevOpC op;

[XmlAttribute]
		public TevBias bias;

[XmlAttribute]
		public TevScale scale;

[XmlAttribute]
		public bool clamp;

[XmlAttribute]
		public TevRegID outReg;
	}
}
