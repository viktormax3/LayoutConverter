using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	
	[XmlInclude(typeof(AnimTexSRTTarget))]
	[DebuggerStepThrough]
	[XmlInclude(typeof(AnimVertexColorTarget))]
	[XmlInclude(typeof(AnimTexPatternTarget))]
	[XmlInclude(typeof(AnimPainSRTTarget))]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlInclude(typeof(AnimVisibilityTarget))]
	[XmlInclude(typeof(AnimMaterialColorTarget))]
	[XmlInclude(typeof(AnimIndTexSRTTarget))]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class AnimTarget
	{
		
		public AnimTarget()
		{
			this.id = 0;
			this.preInfinityType = InfinityType.Constant;
			this.postInfinityType = InfinityType.Constant;
		}

public AnimTarget Duplicate(Hermite[] newKeys)
		{
			AnimTarget animTarget = (AnimTarget)base.MemberwiseClone();
			animTarget.key = newKeys;
			return animTarget;
		}

[XmlElement("refRes")]
		public RefRes[] refRes;

[XmlElement("key")]
		public Hermite[] key;

[XmlAttribute]
		[DefaultValue(typeof(byte), "0")]
		public byte id;

[XmlAttribute]
		public AnimTargetType target;

[DefaultValue(InfinityType.Constant)]
		[XmlAttribute]
		public InfinityType preInfinityType;

[XmlAttribute]
		[DefaultValue(InfinityType.Constant)]
		public InfinityType postInfinityType;
	}
}

