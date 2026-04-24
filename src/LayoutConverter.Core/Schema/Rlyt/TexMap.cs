using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public class TexMap
	{
		
		public TexMap()
		{
			this.minFilter = TexFilter.Linear;
			this.magFilter = TexFilter.Linear;
		}

[XmlAttribute]
		public string imageName;

[XmlAttribute]
		public string paletteName;

[XmlAttribute]
		public TexWrapMode wrap_s;

[XmlAttribute]
		public TexWrapMode wrap_t;

[DefaultValue(TexFilter.Linear)]
		[XmlAttribute]
		public TexFilter minFilter;

[DefaultValue(TexFilter.Linear)]
		[XmlAttribute]
		public TexFilter magFilter;
	}
}
