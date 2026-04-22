using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200003F RID: 63
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class TexMap
	{
		// Token: 0x06000045 RID: 69 RVA: 0x00002740 File Offset: 0x00001740
		public TexMap()
		{
			this.minFilter = TexFilter.Linear;
			this.magFilter = TexFilter.Linear;
		}

		// Token: 0x0400011C RID: 284
		[XmlAttribute]
		public string imageName;

		// Token: 0x0400011D RID: 285
		[XmlAttribute]
		public string paletteName;

		// Token: 0x0400011E RID: 286
		[XmlAttribute]
		public TexWrapMode wrap_s;

		// Token: 0x0400011F RID: 287
		[XmlAttribute]
		public TexWrapMode wrap_t;

		// Token: 0x04000120 RID: 288
		[DefaultValue(TexFilter.Linear)]
		[XmlAttribute]
		public TexFilter minFilter;

		// Token: 0x04000121 RID: 289
		[DefaultValue(TexFilter.Linear)]
		[XmlAttribute]
		public TexFilter magFilter;
	}
}
