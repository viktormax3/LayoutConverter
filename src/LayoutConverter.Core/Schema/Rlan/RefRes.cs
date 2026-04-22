using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x0200000A RID: 10
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[Serializable]
	public class RefRes
	{
		// Token: 0x0600000B RID: 11 RVA: 0x00002140 File Offset: 0x00001140
		public string GetFileName()
		{
			return this.name + ".tpl";
		}

		// Token: 0x04000014 RID: 20
		[XmlAttribute]
		public string name;
	}
}

