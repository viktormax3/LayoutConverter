using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000079 RID: 121
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class Vec3
	{
		// Token: 0x06000061 RID: 97 RVA: 0x00002AD8 File Offset: 0x00001AD8
		public Vec3()
		{
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00002AEC File Offset: 0x00001AEC
		public Vec3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		// Token: 0x040002BD RID: 701
		[XmlAttribute]
		public float x;

		// Token: 0x040002BE RID: 702
		[XmlAttribute]
		public float y;

		// Token: 0x040002BF RID: 703
		[XmlAttribute]
		public float z;
	}
}
