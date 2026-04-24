using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	[DebuggerStepThrough]
	[XmlRoot("group", Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor", IsNullable = false)]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class Group
	{
		
		[XmlElement("paneRef")]
		public GroupPaneRef[] paneRef;

[XmlElement("group")]
		public Group[] group;

public string comment;

[XmlArrayItem("int", typeof(UserDataIntList), IsNullable = false)]
		[XmlArrayItem("float", typeof(UserDataFloatList), IsNullable = false)]
		[XmlArrayItem("string", typeof(UserDataString), IsNullable = false)]
		public object[] userData;

[XmlAttribute]
		public string name;
	}
}
