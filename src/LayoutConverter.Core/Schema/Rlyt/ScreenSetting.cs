using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000029 RID: 41
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class ScreenSetting
	{
		// Token: 0x040000D2 RID: 210
		public Vec2 layoutSize;

		// Token: 0x040000D3 RID: 211
		public ScreenSettingBackGround backGround;

		// Token: 0x040000D4 RID: 212
		public ScreenSettingGrid grid;

		// Token: 0x040000D5 RID: 213
		[XmlAttribute]
		public ScreenOriginType origin;
	}
}
