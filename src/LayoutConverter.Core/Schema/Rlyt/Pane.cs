using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x0200007A RID: 122
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[Serializable]
	public class Pane
	{
		// Token: 0x06000063 RID: 99 RVA: 0x00002B14 File Offset: 0x00001B14
		public Pane()
		{
			this.visible = true;
			this.influencedAlpha = false;
			this.hidden = false;
			this.locked = false;
			this.alpha = byte.MaxValue;
			this.locationAdjust = false;
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00002B58 File Offset: 0x00001B58
		public Pane(string paneName, Vec2 size)
			: this()
		{
			this.name = paneName;
			this.kind = PaneKind.Null;
			this.basePositionType = new Position();
			this.basePositionType.x = HorizontalPosition.Center;
			this.basePositionType.y = VerticalPosition.Center;
			this.translate = new Vec3(0f, 0f, 0f);
			this.rotate = new Vec3(0f, 0f, 0f);
			this.scale = new Vec2(1f, 1f);
			this.size = size;
		}

		// Token: 0x040002C0 RID: 704
		public string comment;

		// Token: 0x040002C1 RID: 705
		public Position basePositionType;

		// Token: 0x040002C2 RID: 706
		public Vec3 translate;

		// Token: 0x040002C3 RID: 707
		public Vec3 rotate;

		// Token: 0x040002C4 RID: 708
		public Vec2 scale;

		// Token: 0x040002C5 RID: 709
		public Vec2 size;

		// Token: 0x040002C6 RID: 710
		[XmlElement("bounding", typeof(Bounding))]
		[XmlElement("textBox", typeof(TextBox))]
		[XmlElement("window", typeof(Window))]
		[XmlElement("picture", typeof(Picture))]
		public object Item;

		// Token: 0x040002C7 RID: 711
		[XmlArrayItem("float", typeof(UserDataFloatList), IsNullable = false)]
		[XmlArrayItem("int", typeof(UserDataIntList), IsNullable = false)]
		[XmlArrayItem("string", typeof(UserDataString), IsNullable = false)]
		public object[] userData;

		// Token: 0x040002C8 RID: 712
		[XmlAttribute]
		public PaneKind kind;

		// Token: 0x040002C9 RID: 713
		[XmlAttribute]
		public string name;

		// Token: 0x040002CA RID: 714
		[DefaultValue(true)]
		[XmlAttribute]
		public bool visible;

		// Token: 0x040002CB RID: 715
		[DefaultValue(false)]
		[XmlAttribute]
		public bool influencedAlpha;

		// Token: 0x040002CC RID: 716
		[DefaultValue(false)]
		[XmlAttribute]
		public bool hidden;

		// Token: 0x040002CD RID: 717
		[XmlAttribute]
		[DefaultValue(false)]
		public bool locked;

		// Token: 0x040002CE RID: 718
		[DefaultValue(typeof(byte), "255")]
		[XmlAttribute]
		public byte alpha;

		// Token: 0x040002CF RID: 719
		[DefaultValue(false)]
		[XmlAttribute]
		public bool locationAdjust;

		// Token: 0x040002D0 RID: 720
		[XmlIgnore]
		public List<Pane> ChildList;
	}
}
