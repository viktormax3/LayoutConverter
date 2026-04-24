using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class RLYT
	{
		
		[XmlArrayItem("pane", IsNullable = false)]
		public Pane[] paneSet;

public PaneHierarchy paneHierarchy;

public GroupSet groupSet;

public ScreenSetting screenSetting;

[XmlElement("textureFile")]
		public TextureFile[] textureFile;

[XmlElement("fontFile")]
		public FontFile[] fontFile;
	}
}
