using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x0200001E RID: 30
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class AnimTag
	{
		// Token: 0x0600001D RID: 29 RVA: 0x000022E0 File Offset: 0x000012E0
		public AnimTag()
		{
			this.animLoop = AnimLoopType.Loop;
			this.outputEnabled = true;
			this.outputPaneSRT = true;
			this.outputVisibility = true;
			this.outputVertexColor = true;
			this.outputMaterialColor = true;
			this.outputTextureSRT = true;
			this.outputTexturePattern = true;
			this.outputIndTextureSRT = true;
			this.emptyKeyOutputEnabled = true;
			this.descendingBind = true;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002340 File Offset: 0x00001340
		public string GetFileName()
		{
			if (this.fileName == null)
			{
				return this.name;
			}
			return this.fileName;
		}

		// Token: 0x0400009C RID: 156
		public string comment;

		// Token: 0x0400009D RID: 157
		[XmlArrayItem("string", typeof(UserDataString), IsNullable = false)]
		[XmlArrayItem("float", typeof(UserDataFloatList), IsNullable = false)]
		[XmlArrayItem("int", typeof(UserDataIntList), IsNullable = false)]
		public object[] userData;

		// Token: 0x0400009E RID: 158
		public Color4 color;

		// Token: 0x0400009F RID: 159
		[XmlElement("group")]
		public GroupRef[] group;

		// Token: 0x040000A0 RID: 160
		[XmlAttribute]
		public string name;

		// Token: 0x040000A1 RID: 161
		[XmlAttribute]
		public int startFrame;

		// Token: 0x040000A2 RID: 162
		[XmlAttribute]
		public int endFrame;

		// Token: 0x040000A3 RID: 163
		[XmlAttribute]
		[DefaultValue(AnimLoopType.Loop)]
		public AnimLoopType animLoop;

		// Token: 0x040000A4 RID: 164
		[XmlAttribute]
		public string fileName;

		// Token: 0x040000A5 RID: 165
		[DefaultValue(true)]
		[XmlAttribute]
		public bool outputEnabled;

		// Token: 0x040000A6 RID: 166
		[XmlAttribute]
		[DefaultValue(true)]
		public bool outputPaneSRT;

		// Token: 0x040000A7 RID: 167
		[XmlAttribute]
		[DefaultValue(true)]
		public bool outputVisibility;

		// Token: 0x040000A8 RID: 168
		[DefaultValue(true)]
		[XmlAttribute]
		public bool outputVertexColor;

		// Token: 0x040000A9 RID: 169
		[DefaultValue(true)]
		[XmlAttribute]
		public bool outputMaterialColor;

		// Token: 0x040000AA RID: 170
		[DefaultValue(true)]
		[XmlAttribute]
		public bool outputTextureSRT;

		// Token: 0x040000AB RID: 171
		[DefaultValue(true)]
		[XmlAttribute]
		public bool outputTexturePattern;

		// Token: 0x040000AC RID: 172
		[DefaultValue(true)]
		[XmlAttribute]
		public bool outputIndTextureSRT;

		// Token: 0x040000AD RID: 173
		[DefaultValue(true)]
		[XmlAttribute]
		public bool emptyKeyOutputEnabled;

		// Token: 0x040000AE RID: 174
		[DefaultValue(true)]
		[XmlAttribute]
		public bool descendingBind;

		// Token: 0x040000AF RID: 175
		[XmlIgnore]
		public int FrameSize;
	}
}

