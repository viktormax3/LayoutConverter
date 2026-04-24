using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[Serializable]
	public class AnimContent
	{
		
		[XmlElement("animIndTexSRTTarget", typeof(AnimIndTexSRTTarget))]
		[XmlElement("animVisibilityTarget", typeof(AnimVisibilityTarget))]
		[XmlElement("animVertexColorTarget", typeof(AnimVertexColorTarget))]
		[XmlElement("animTexSRTTarget", typeof(AnimTexSRTTarget))]
		[XmlElement("animMaterialColorTarget", typeof(AnimMaterialColorTarget))]
		[XmlElement("animTexPatternTarget", typeof(AnimTexPatternTarget))]
		[XmlElement("animPainSRTTarget", typeof(AnimPainSRTTarget))]
		public AnimTarget[] Items;

[XmlAttribute]
		public string name;
	}
}

