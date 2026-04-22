using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000070 RID: 112
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[Serializable]
	public class TexCoord
	{
		// Token: 0x06000058 RID: 88 RVA: 0x00002928 File Offset: 0x00001928
		public TexCoord()
		{
		}

		// Token: 0x06000059 RID: 89 RVA: 0x0000293C File Offset: 0x0000193C
		public TexCoord(float s, float t)
		{
			this.texLT = new TexVec2(0f, 0f);
			this.texRT = new TexVec2(s, 0f);
			this.texLB = new TexVec2(0f, t);
			this.texRB = new TexVec2(s, t);
		}

		// Token: 0x0400028E RID: 654
		public TexVec2 texLT;

		// Token: 0x0400028F RID: 655
		public TexVec2 texRT;

		// Token: 0x04000290 RID: 656
		public TexVec2 texLB;

		// Token: 0x04000291 RID: 657
		public TexVec2 texRB;
	}
}
