using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000040 RID: 64
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public enum TexWrapMode
	{
		// Token: 0x04000123 RID: 291
		Clamp,
		// Token: 0x04000124 RID: 292
		Repeat,
		// Token: 0x04000125 RID: 293
		Mirror
	}
}
