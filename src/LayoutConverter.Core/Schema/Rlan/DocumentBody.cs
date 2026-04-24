using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true, Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[Serializable]
	public class DocumentBody
	{
		
		[XmlElement("animShare")]
		public AnimShare[] animShare;

[XmlElement("animTag")]
		public AnimTag[] animTag;

[XmlElement("rlan")]
		public RLAN[] rlan;
	}
}

