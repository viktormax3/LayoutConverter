using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class AnimTag
	{
		
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
			this.binaryIndex = -1;
		}
		public string GetFileName()
		{
			if (this.fileName == null)
			{
				return this.name;
			}
			return this.fileName;
		}
		public string comment;[XmlArrayItem("string", typeof(UserDataString), IsNullable = false)]
		[XmlArrayItem("float", typeof(UserDataFloatList), IsNullable = false)]
		[XmlArrayItem("int", typeof(UserDataIntList), IsNullable = false)]
		public object[] userData;public Color4 color;[XmlElement("group")]
		public GroupRef[] group;[XmlAttribute]
		public string name;[XmlAttribute]
		public int startFrame;[XmlAttribute]
		public int endFrame;[XmlAttribute]
		[DefaultValue(AnimLoopType.Loop)]
		public AnimLoopType animLoop;[XmlAttribute]
		public string fileName;[DefaultValue(true)]
		[XmlAttribute]
		public bool outputEnabled;[XmlAttribute]
		[DefaultValue(true)]
		public bool outputPaneSRT;[XmlAttribute]
		[DefaultValue(true)]
		public bool outputVisibility;[DefaultValue(true)]
		[XmlAttribute]
		public bool outputVertexColor;[DefaultValue(true)]
		[XmlAttribute]
		public bool outputMaterialColor;[DefaultValue(true)]
		[XmlAttribute]
		public bool outputTextureSRT;[DefaultValue(true)]
		[XmlAttribute]
		public bool outputTexturePattern;[DefaultValue(true)]
		[XmlAttribute]
		public bool outputIndTextureSRT;[DefaultValue(true)]
		[XmlAttribute]
		public bool emptyKeyOutputEnabled;[DefaultValue(true)]
		[XmlAttribute]
		public bool descendingBind;[XmlIgnore]
		public int FrameSize;

		[XmlAttribute]
		public int binaryIndex;

		[XmlIgnore]
		public bool binaryIndexSpecified;
	}
}

