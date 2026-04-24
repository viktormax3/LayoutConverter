using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public enum InfinityType
	{
		
		Constant,
		
		Cycle,
		
		Linear,
		
		CycleOffset,
		
		Oscillate
	}
}

