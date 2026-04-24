using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[DebuggerStepThrough]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class Material_RevoTevStageAlpha
	{
		
		[XmlAttribute]
		public TevAlphaArg a;

[XmlAttribute]
		public TevAlphaArg b;

[XmlAttribute]
		public TevAlphaArg c;

[XmlAttribute]
		public TevAlphaArg d;

[XmlAttribute]
		public TevKAlphaSel konst;

[XmlAttribute]
		public TevOpA op;

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
