using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000078 RID: 120
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class Picture
	{
		// Token: 0x0600005F RID: 95 RVA: 0x00002A24 File Offset: 0x00001A24
		public Picture()
		{
			this.detailSetting = false;
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00002A40 File Offset: 0x00001A40
		public Picture(Material material)
			: this()
		{
			this.vtxColLT = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			this.vtxColRT = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			this.vtxColLB = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			this.vtxColRB = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			this.material = material;
		}

		// Token: 0x040002B4 RID: 692
		public Color4 vtxColLT;

		// Token: 0x040002B5 RID: 693
		public Color4 vtxColRT;

		// Token: 0x040002B6 RID: 694
		public Color4 vtxColLB;

		// Token: 0x040002B7 RID: 695
		public Color4 vtxColRB;

		// Token: 0x040002B8 RID: 696
		[XmlElement("texCoord")]
		public TexCoord[] texCoord;

		// Token: 0x040002B9 RID: 697
		public Material material;

		// Token: 0x040002BA RID: 698
		public Material_Revo materialRevo;

		// Token: 0x040002BB RID: 699
		[DefaultValue(false)]
		[XmlAttribute]
		public bool detailSetting;

		// Token: 0x040002BC RID: 700
		[XmlIgnore]
		public MaterialInfo MaterialInfo;
	}
}
