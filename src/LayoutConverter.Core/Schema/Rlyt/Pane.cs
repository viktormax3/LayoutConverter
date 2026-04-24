using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[Serializable]
	public class Pane
	{
		
		public Pane()
		{
			this.visible = true;
			this.influencedAlpha = false;
			this.hidden = false;
			this.locked = false;
			this.alpha = byte.MaxValue;
			this.locationAdjust = false;
			this.binaryReservedBytes = string.Empty;
		}

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

public string comment;

public Position basePositionType;

public Vec3 translate;

public Vec3 rotate;

public Vec2 scale;

public Vec2 size;

[XmlElement("bounding", typeof(Bounding))]
		[XmlElement("textBox", typeof(TextBox))]
		[XmlElement("window", typeof(Window))]
		[XmlElement("picture", typeof(Picture))]
		public object Item;

[XmlArrayItem("float", typeof(UserDataFloatList), IsNullable = false)]
		[XmlArrayItem("int", typeof(UserDataIntList), IsNullable = false)]
		[XmlArrayItem("string", typeof(UserDataString), IsNullable = false)]
		public object[] userData;

[XmlAttribute]
		public PaneKind kind;

[XmlAttribute]
		public string name;

[DefaultValue(true)]
		[XmlAttribute]
		public bool visible;

[DefaultValue(false)]
		[XmlAttribute]
		public bool influencedAlpha;

[DefaultValue(false)]
		[XmlAttribute]
		public bool hidden;

[XmlAttribute]
		[DefaultValue(false)]
		public bool locked;

[DefaultValue(typeof(byte), "255")]
		[XmlAttribute]
		public byte alpha;

[DefaultValue(false)]
		[XmlAttribute]
		public bool locationAdjust;

		[XmlAttribute]
		[DefaultValue("")]
		public string binaryReservedBytes;

[XmlIgnore]
		public List<Pane> ChildList;
	}
}
