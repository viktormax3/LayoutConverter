using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200007C RID: 124
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class RLYT
	{
		// Token: 0x040002D7 RID: 727
		[XmlArrayItem("pane", IsNullable = false)]
		public Pane[] paneSet;

		// Token: 0x040002D8 RID: 728
		public PaneHierarchy paneHierarchy;

		// Token: 0x040002D9 RID: 729
		public GroupSet groupSet;

		// Token: 0x040002DA RID: 730
		public ScreenSetting screenSetting;

		// Token: 0x040002DB RID: 731
		[XmlElement("textureFile")]
		public TextureFile[] textureFile;

		// Token: 0x040002DC RID: 732
		[XmlElement("fontFile")]
		public FontFile[] fontFile;
	}
}
