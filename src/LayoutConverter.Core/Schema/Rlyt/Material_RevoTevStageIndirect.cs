using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[Serializable]
	public class Material_RevoTevStageIndirect
	{
		
		[XmlAttribute]
		public byte indStage;

[XmlAttribute]
		public IndTexFormat format;

[XmlAttribute]
		public IndTexBiasSel bias;

[XmlAttribute]
		public IndTexMtxID matrix;

[XmlAttribute]
		public IndTexWrap wrap_s;

[XmlAttribute]
		public IndTexWrap wrap_t;

[XmlAttribute]
		public bool addPrev;

[XmlAttribute]
		public bool utcLod;

[XmlAttribute]
		public IndTexAlphaSel alpha;
	}
}
