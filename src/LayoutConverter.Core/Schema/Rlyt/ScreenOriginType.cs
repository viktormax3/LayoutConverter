using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200002F RID: 47
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum ScreenOriginType
	{
		// Token: 0x040000E8 RID: 232
		Classic,
		// Token: 0x040000E9 RID: 233
		Normal
	}
}
