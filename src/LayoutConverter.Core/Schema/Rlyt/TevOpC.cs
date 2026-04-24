using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum TevOpC
	{
		
		Add,
		
		Sub,
		
		Comp_r8_gt,
		
		Comp_r8_eq,
		
		Comp_gr16_gt,
		
		Comp_gr16_eq,
		
		Comp_bgr24_gt,
		
		Comp_bgr24_eq,
		
		Comp_rgb8_gt,
		
		Comp_rgb8_eq
	}
}
