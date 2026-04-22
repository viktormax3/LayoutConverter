using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000052 RID: 82
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class Material_RevoTevStage
	{
		// Token: 0x04000179 RID: 377
		public Material_RevoTevStageColor color;

		// Token: 0x0400017A RID: 378
		public Material_RevoTevStageAlpha alpha;

		// Token: 0x0400017B RID: 379
		public Material_RevoTevStageIndirect indirect;

		// Token: 0x0400017C RID: 380
		[XmlAttribute]
		public TevChannelID colorChannel;

		// Token: 0x0400017D RID: 381
		[XmlAttribute]
		public sbyte texMap;

		// Token: 0x0400017E RID: 382
		[XmlAttribute]
		public sbyte texCoordGen;

		// Token: 0x0400017F RID: 383
		[XmlAttribute]
		public sbyte rasColSwap;

		// Token: 0x04000180 RID: 384
		[XmlAttribute]
		public sbyte texColSwap;
	}
}
