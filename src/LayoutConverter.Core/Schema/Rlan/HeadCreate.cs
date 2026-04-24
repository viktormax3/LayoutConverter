using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[Serializable]
	public class HeadCreate
	{
		
		[XmlAttribute]
		public string user;

[XmlAttribute]
		public string host;

[XmlAttribute]
		public DateTime date;

[XmlAttribute]
		public string source;
	}
}

