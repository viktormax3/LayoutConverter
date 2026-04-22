using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x02000016 RID: 22
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class RLAN
	{
		// Token: 0x06000017 RID: 23 RVA: 0x00002260 File Offset: 0x00001260
		public RLAN()
		{
			this.animLoop = AnimLoopType.Loop;
		}

		// Token: 0x0400007F RID: 127
		[XmlElement("animContent")]
		public AnimContent[] animContent;

		// Token: 0x04000080 RID: 128
		[XmlAttribute]
		public AnimationType animType;

		// Token: 0x04000081 RID: 129
		[XmlAttribute]
		public int startFrame;

		// Token: 0x04000082 RID: 130
		[XmlAttribute]
		public int endFrame;

		// Token: 0x04000083 RID: 131
		[XmlAttribute]
		public int convertStartFrame;

		// Token: 0x04000084 RID: 132
		[XmlAttribute]
		public int convertEndFrame;

		// Token: 0x04000085 RID: 133
		[XmlAttribute]
		[DefaultValue(AnimLoopType.Loop)]
		public AnimLoopType animLoop;
	}
}

