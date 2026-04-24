using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	
	[XmlInclude(typeof(StepU16))]
	[XmlInclude(typeof(HermiteU8))]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[DebuggerStepThrough]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlInclude(typeof(StepBool))]
	[DesignerCategory("code")]
	[Serializable]
	public class Hermite
	{
		
		public Hermite()
		{
			this.slope = 0f;
			this.slopeType = SlopeType.Fixed;
		}

public Hermite Duplicate(float frame)
		{
			Hermite hermite = (Hermite)base.MemberwiseClone();
			hermite.frame = frame;
			return hermite;
		}

[XmlAttribute]
		public float frame;

[XmlAttribute]
		public float value;

[XmlAttribute]
		[DefaultValue(typeof(float), "0")]
		public float slope;

[DefaultValue(SlopeType.Fixed)]
		[XmlAttribute]
		public SlopeType slopeType;
	}
}

