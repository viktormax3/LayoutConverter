using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000046 RID: 70
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class MaterialTextureStage
	{
		// Token: 0x06000048 RID: 72 RVA: 0x0000278C File Offset: 0x0000178C
		public MaterialTextureStage()
		{
			this.indirectStage = -1;
		}

		// Token: 0x0400013A RID: 314
		[XmlAttribute]
		public sbyte texMap;

		// Token: 0x0400013B RID: 315
		[XmlAttribute]
		public sbyte texCoordGen;

		// Token: 0x0400013C RID: 316
		[XmlAttribute]
		[DefaultValue(typeof(sbyte), "-1")]
		public sbyte indirectStage;
	}
}
