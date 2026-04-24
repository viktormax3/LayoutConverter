using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum TexelFormat
	{
		
		I4,
		
		IA4,
		
		I8,
		
		IA8,
		
		RGB565,
		
		RGB5A3,
		
		RGBA8,
		
		CMPR,
		
		NW4R_TGA
	}
}
