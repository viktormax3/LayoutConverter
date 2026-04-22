using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000031 RID: 49
	[DebuggerStepThrough]
	[XmlRoot("group", Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor", IsNullable = false)]
	[GeneratedCode("xsd", "2.0.50727.42")]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public class Group
	{
		// Token: 0x040000EB RID: 235
		[XmlElement("paneRef")]
		public GroupPaneRef[] paneRef;

		// Token: 0x040000EC RID: 236
		[XmlElement("group")]
		public Group[] group;

		// Token: 0x040000ED RID: 237
		public string comment;

		// Token: 0x040000EE RID: 238
		[XmlArrayItem("int", typeof(UserDataIntList), IsNullable = false)]
		[XmlArrayItem("float", typeof(UserDataFloatList), IsNullable = false)]
		[XmlArrayItem("string", typeof(UserDataString), IsNullable = false)]
		public object[] userData;

		// Token: 0x040000EF RID: 239
		[XmlAttribute]
		public string name;
	}
}
