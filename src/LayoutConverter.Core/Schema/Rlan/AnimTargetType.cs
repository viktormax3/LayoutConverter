using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace LayoutConverter.Core.Schema.Rlan
{
	// Token: 0x0200000C RID: 12
	[GeneratedCode("xsd", "2.0.50727.42")]
	[XmlType(Namespace = "http://www.nintendo.co.jp/NW4R/LayoutEditor")]
	[Serializable]
	public enum AnimTargetType
	{
		// Token: 0x0400001C RID: 28
		TranslateX,
		// Token: 0x0400001D RID: 29
		TranslateY,
		// Token: 0x0400001E RID: 30
		TranslateZ,
		// Token: 0x0400001F RID: 31
		RotateX,
		// Token: 0x04000020 RID: 32
		RotateY,
		// Token: 0x04000021 RID: 33
		RotateZ,
		// Token: 0x04000022 RID: 34
		ScaleX,
		// Token: 0x04000023 RID: 35
		ScaleY,
		// Token: 0x04000024 RID: 36
		SizeW,
		// Token: 0x04000025 RID: 37
		SizeH,
		// Token: 0x04000026 RID: 38
		Visibility,
		// Token: 0x04000027 RID: 39
		LT_r,
		// Token: 0x04000028 RID: 40
		LT_g,
		// Token: 0x04000029 RID: 41
		LT_b,
		// Token: 0x0400002A RID: 42
		LT_a,
		// Token: 0x0400002B RID: 43
		RT_r,
		// Token: 0x0400002C RID: 44
		RT_g,
		// Token: 0x0400002D RID: 45
		RT_b,
		// Token: 0x0400002E RID: 46
		RT_a,
		// Token: 0x0400002F RID: 47
		LB_r,
		// Token: 0x04000030 RID: 48
		LB_g,
		// Token: 0x04000031 RID: 49
		LB_b,
		// Token: 0x04000032 RID: 50
		LB_a,
		// Token: 0x04000033 RID: 51
		RB_r,
		// Token: 0x04000034 RID: 52
		RB_g,
		// Token: 0x04000035 RID: 53
		RB_b,
		// Token: 0x04000036 RID: 54
		RB_a,
		// Token: 0x04000037 RID: 55
		PaneAlpha,
		// Token: 0x04000038 RID: 56
		BlackColor_r,
		// Token: 0x04000039 RID: 57
		BlackColor_g,
		// Token: 0x0400003A RID: 58
		BlackColor_b,
		// Token: 0x0400003B RID: 59
		BlackColor_a,
		// Token: 0x0400003C RID: 60
		WhiteColor_r,
		// Token: 0x0400003D RID: 61
		WhiteColor_g,
		// Token: 0x0400003E RID: 62
		WhiteColor_b,
		// Token: 0x0400003F RID: 63
		WhiteColor_a,
		// Token: 0x04000040 RID: 64
		TexBlendRatio0_c,
		// Token: 0x04000041 RID: 65
		TexBlendRatio0_a,
		// Token: 0x04000042 RID: 66
		TexBlendRatio1_c,
		// Token: 0x04000043 RID: 67
		TexBlendRatio1_a,
		// Token: 0x04000044 RID: 68
		TexBlendRatio2_c,
		// Token: 0x04000045 RID: 69
		TexBlendRatio2_a,
		// Token: 0x04000046 RID: 70
		TexBlendRatio3_c,
		// Token: 0x04000047 RID: 71
		TexBlendRatio3_a,
		// Token: 0x04000048 RID: 72
		TexBlendRatio4_c,
		// Token: 0x04000049 RID: 73
		TexBlendRatio4_a,
		// Token: 0x0400004A RID: 74
		TexBlendRatio5_c,
		// Token: 0x0400004B RID: 75
		TexBlendRatio5_a,
		// Token: 0x0400004C RID: 76
		TexBlendRatio6_c,
		// Token: 0x0400004D RID: 77
		TexBlendRatio6_a,
		// Token: 0x0400004E RID: 78
		TexBlendRatio7_c,
		// Token: 0x0400004F RID: 79
		TexBlendRatio7_a,
		// Token: 0x04000050 RID: 80
		MatColor0_r,
		// Token: 0x04000051 RID: 81
		MatColor0_g,
		// Token: 0x04000052 RID: 82
		MatColor0_b,
		// Token: 0x04000053 RID: 83
		MatColor0_a,
		// Token: 0x04000054 RID: 84
		TevColor0_r,
		// Token: 0x04000055 RID: 85
		TevColor0_g,
		// Token: 0x04000056 RID: 86
		TevColor0_b,
		// Token: 0x04000057 RID: 87
		TevColor0_a,
		// Token: 0x04000058 RID: 88
		TevColor1_r,
		// Token: 0x04000059 RID: 89
		TevColor1_g,
		// Token: 0x0400005A RID: 90
		TevColor1_b,
		// Token: 0x0400005B RID: 91
		TevColor1_a,
		// Token: 0x0400005C RID: 92
		TevColor2_r,
		// Token: 0x0400005D RID: 93
		TevColor2_g,
		// Token: 0x0400005E RID: 94
		TevColor2_b,
		// Token: 0x0400005F RID: 95
		TevColor2_a,
		// Token: 0x04000060 RID: 96
		TevKonst0_r,
		// Token: 0x04000061 RID: 97
		TevKonst0_g,
		// Token: 0x04000062 RID: 98
		TevKonst0_b,
		// Token: 0x04000063 RID: 99
		TevKonst0_a,
		// Token: 0x04000064 RID: 100
		TevKonst1_r,
		// Token: 0x04000065 RID: 101
		TevKonst1_g,
		// Token: 0x04000066 RID: 102
		TevKonst1_b,
		// Token: 0x04000067 RID: 103
		TevKonst1_a,
		// Token: 0x04000068 RID: 104
		TevKonst2_r,
		// Token: 0x04000069 RID: 105
		TevKonst2_g,
		// Token: 0x0400006A RID: 106
		TevKonst2_b,
		// Token: 0x0400006B RID: 107
		TevKonst2_a,
		// Token: 0x0400006C RID: 108
		TevKonst3_r,
		// Token: 0x0400006D RID: 109
		TevKonst3_g,
		// Token: 0x0400006E RID: 110
		TevKonst3_b,
		// Token: 0x0400006F RID: 111
		TevKonst3_a,
		// Token: 0x04000070 RID: 112
		TranslateS,
		// Token: 0x04000071 RID: 113
		TranslateT,
		// Token: 0x04000072 RID: 114
		Rotate,
		// Token: 0x04000073 RID: 115
		ScaleS,
		// Token: 0x04000074 RID: 116
		ScaleT,
		// Token: 0x04000075 RID: 117
		Image,
		// Token: 0x04000076 RID: 118
		Palette
	}
}

