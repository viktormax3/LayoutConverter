using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[Serializable]
	public class TextureFile
	{
		
		public string GetName()
		{
			return Path.GetFileNameWithoutExtension(this.imagePath);
		}

public string GetConvertedFileName()
		{
			return Path.ChangeExtension(Path.GetFileName(this.imagePath), ".tpl");
		}

[XmlAttribute]
		public string imagePath;

[XmlAttribute]
		public TexelFormat format;
	}
}
