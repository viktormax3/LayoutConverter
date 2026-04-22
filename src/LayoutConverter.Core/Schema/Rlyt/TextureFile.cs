using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000027 RID: 39
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[Serializable]
	public class TextureFile
	{
		// Token: 0x06000029 RID: 41 RVA: 0x00002440 File Offset: 0x00001440
		public string GetName()
		{
			return Path.GetFileNameWithoutExtension(this.imagePath);
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002458 File Offset: 0x00001458
		public string GetConvertedFileName()
		{
			return Path.ChangeExtension(Path.GetFileName(this.imagePath), ".tpl");
		}

		// Token: 0x040000C6 RID: 198
		[XmlAttribute]
		public string imagePath;

		// Token: 0x040000C7 RID: 199
		[XmlAttribute]
		public TexelFormat format;
	}
}
