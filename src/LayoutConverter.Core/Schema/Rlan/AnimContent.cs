using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x02000015 RID: 21
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[Serializable]
	public class AnimContent
	{
		// Token: 0x0400007D RID: 125
		[XmlElement("animIndTexSRTTarget", typeof(AnimIndTexSRTTarget))]
		[XmlElement("animVisibilityTarget", typeof(AnimVisibilityTarget))]
		[XmlElement("animVertexColorTarget", typeof(AnimVertexColorTarget))]
		[XmlElement("animTexSRTTarget", typeof(AnimTexSRTTarget))]
		[XmlElement("animMaterialColorTarget", typeof(AnimMaterialColorTarget))]
		[XmlElement("animTexPatternTarget", typeof(AnimTexPatternTarget))]
		[XmlElement("animPainSRTTarget", typeof(AnimPainSRTTarget))]
		public AnimTarget[] Items;

		// Token: 0x0400007E RID: 126
		[XmlAttribute]
		public string name;
	}
}

