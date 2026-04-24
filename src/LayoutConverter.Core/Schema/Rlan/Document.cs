using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[XmlRoot("nw4r_layout", Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor", IsNullable = false)]
	[DebuggerStepThrough]
	[Serializable]
	public class Document
	{
		
		public Document()
		{
			this.version = "1.2.0";
		}

public Head head;

public DocumentBody body;

[XmlAttribute]
		public string version;
	}
}

