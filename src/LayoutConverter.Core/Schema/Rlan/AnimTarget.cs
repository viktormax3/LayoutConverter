using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x0200000B RID: 11
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
		// Token: 0x0600000D RID: 13 RVA: 0x00002174 File Offset: 0x00001174
		public AnimTarget()
		{
			this.id = 0;
			this.preInfinityType = InfinityType.Constant;
			this.postInfinityType = InfinityType.Constant;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x0000219C File Offset: 0x0000119C
		public AnimTarget Duplicate(Hermite[] newKeys)
		{
			AnimTarget animTarget = (AnimTarget)base.MemberwiseClone();
			animTarget.key = newKeys;
			return animTarget;
		}

		// Token: 0x04000015 RID: 21
		[XmlElement("refRes")]
		public RefRes[] refRes;

		// Token: 0x04000016 RID: 22
		[XmlElement("key")]
		public Hermite[] key;

		// Token: 0x04000017 RID: 23
		[XmlAttribute]
		[DefaultValue(typeof(byte), "0")]
		public byte id;

		// Token: 0x04000018 RID: 24
		[XmlAttribute]
		public AnimTargetType target;

		// Token: 0x04000019 RID: 25
		[DefaultValue(InfinityType.Constant)]
		[XmlAttribute]
		public InfinityType preInfinityType;

		// Token: 0x0400001A RID: 26
		[XmlAttribute]
		[DefaultValue(InfinityType.Constant)]
		public InfinityType postInfinityType;
	}
}

