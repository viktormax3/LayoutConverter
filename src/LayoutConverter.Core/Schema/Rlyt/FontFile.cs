using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DebuggerStepThrough]
	[Serializable]
	public class FontFile
	{
		
		public string GetName()
		{
			return Path.GetFileNameWithoutExtension(this.path);
		}

public string GetFileName()
		{
			return Path.GetFileName(this.path);
		}

[XmlAttribute]
		public string path;
	}
}
