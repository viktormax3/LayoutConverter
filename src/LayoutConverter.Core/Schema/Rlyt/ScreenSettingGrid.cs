using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[Serializable]
	public class ScreenSettingGrid
	{
		
		public Color4 color;

[XmlAttribute]
		public float thickLineInterval;

[XmlAttribute]
		public uint thinDivisionNum;

[XmlAttribute]
		public bool visible;

[XmlAttribute]
		public GridMoveMethod moveMethod;
	}
}
