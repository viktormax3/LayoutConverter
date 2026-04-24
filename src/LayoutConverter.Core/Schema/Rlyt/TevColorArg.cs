using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[Serializable]
	public enum TevColorArg
	{
		
		C0,
		
		C1,
		
		C2,
		
		CPrev,
		
		A0,
		
		A1,
		
		A2,
		
		APrev,
		
		TexC,
		
		TexA,
		
		RasC,
		
		RasA,
		
		V1_0,
		
		V0_5,
		
		V0,
		
		Konst
	}
}
