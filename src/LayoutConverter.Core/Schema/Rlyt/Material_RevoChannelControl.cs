using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200004B RID: 75
	[DebuggerStepThrough]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class Material_RevoChannelControl
	{
		// Token: 0x04000160 RID: 352
		[XmlAttribute]
		public ChannelID channel;

		// Token: 0x04000161 RID: 353
		[XmlAttribute]
		public ColorSource materialSource;
	}
}
