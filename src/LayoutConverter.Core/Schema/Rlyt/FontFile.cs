using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000026 RID: 38
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[Serializable]
	public class FontFile
	{
		// Token: 0x06000026 RID: 38 RVA: 0x000023FC File Offset: 0x000013FC
		public string GetName()
		{
			return Path.GetFileNameWithoutExtension(this.path);
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002414 File Offset: 0x00001414
		public string GetFileName()
		{
			return Path.GetFileName(this.path);
		}

		// Token: 0x040000C5 RID: 197
		[XmlAttribute]
		public string path;
	}
}
