using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class ScreenSetting
	{
		
		public Vec2 layoutSize;

public ScreenSettingBackGround backGround;

public ScreenSettingGrid grid;

[XmlAttribute]
		public ScreenOriginType origin;
	}
}
