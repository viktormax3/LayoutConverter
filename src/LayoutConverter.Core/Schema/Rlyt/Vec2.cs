using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200002A RID: 42
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class Vec2
	{
		// Token: 0x0600002D RID: 45 RVA: 0x000024A4 File Offset: 0x000014A4
		public Vec2()
		{
		}

		// Token: 0x0600002E RID: 46 RVA: 0x000024B8 File Offset: 0x000014B8
		public Vec2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		// Token: 0x040000D6 RID: 214
		[XmlAttribute]
		public float x;

		// Token: 0x040000D7 RID: 215
		[XmlAttribute]
		public float y;
	}
}
