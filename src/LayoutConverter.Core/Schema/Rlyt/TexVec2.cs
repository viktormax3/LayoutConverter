using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000071 RID: 113
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[Serializable]
	public class TexVec2
	{
		// Token: 0x0600005A RID: 90 RVA: 0x00002994 File Offset: 0x00001994
		public TexVec2()
		{
		}

		// Token: 0x0600005B RID: 91 RVA: 0x000029A8 File Offset: 0x000019A8
		public TexVec2(float s, float t)
		{
			this.s = s;
			this.t = t;
		}

		// Token: 0x04000292 RID: 658
		[XmlAttribute]
		public float s;

		// Token: 0x04000293 RID: 659
		[XmlAttribute]
		public float t;
	}
}
