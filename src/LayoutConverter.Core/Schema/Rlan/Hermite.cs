using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x02000005 RID: 5
	[XmlInclude(typeof(StepU16))]
	[XmlInclude(typeof(HermiteU8))]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlInclude(typeof(StepBool))]
	[DesignerCategory("code")]
	[Serializable]
	public class Hermite
	{
		// Token: 0x06000006 RID: 6 RVA: 0x000020B8 File Offset: 0x000010B8
		public Hermite()
		{
			this.slope = 0f;
			this.slopeType = SlopeType.Fixed;
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000020E0 File Offset: 0x000010E0
		public Hermite Duplicate(float frame)
		{
			Hermite hermite = base.MemberwiseClone() as Hermite;
			hermite.frame = frame;
			return hermite;
		}

		// Token: 0x04000009 RID: 9
		[XmlAttribute]
		public float frame;

		// Token: 0x0400000A RID: 10
		[XmlAttribute]
		public float value;

		// Token: 0x0400000B RID: 11
		[XmlAttribute]
		[DefaultValue(typeof(float), "0")]
		public float slope;

		// Token: 0x0400000C RID: 12
		[DefaultValue(SlopeType.Fixed)]
		[XmlAttribute]
		public SlopeType slopeType;
	}
}

