using System;

namespace LayoutConverter.Core.Schema.Rlyt
{
	// Token: 0x02000080 RID: 128
	public class MaterialInfo
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000069 RID: 105 RVA: 0x00002C48 File Offset: 0x00001C48
		public Pane Pane
		{
			get
			{
				return this._pane;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600006A RID: 106 RVA: 0x00002C5C File Offset: 0x00001C5C
		public bool IsOutTexture
		{
			get
			{
				return this._bOutTexture;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600006B RID: 107 RVA: 0x00002C70 File Offset: 0x00001C70
		public bool IsDetailSetting
		{
			get
			{
				return this._bDetailSetting;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600006C RID: 108 RVA: 0x00002C84 File Offset: 0x00001C84
		// (set) Token: 0x0600006D RID: 109 RVA: 0x00002C98 File Offset: 0x00001C98
		public int BinaryIndex
		{
			get
			{
				return this._binaryIndex;
			}
			set
			{
				this._binaryIndex = value;
			}
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00002CAC File Offset: 0x00001CAC
		public MaterialInfo(Pane pane, Material simple, Material_Revo detail, bool bOutTexture, bool bDetailSetting)
		{
			this._pane = pane;
			this._simple = simple;
			this._detail = detail;
			this._bOutTexture = bOutTexture;
			this._bDetailSetting = bDetailSetting;
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00002CE4 File Offset: 0x00001CE4
		public bool EqualsContent(MaterialInfo other)
		{
			int num = 3;
			for (;;)
			{
				switch (num)
				{
				case 0:
					if (this._detail == other._detail)
					{
						num = 4;
						continue;
					}
					return false;
				case 1:
					num = 0;
					continue;
				case 2:
					if (this._bOutTexture == other._bOutTexture)
					{
						num = 5;
						continue;
					}
					return false;
				case 4:
					num = 2;
					continue;
				case 5:
					goto IL_007F;
				}
				if (this._simple != other._simple)
				{
					return false;
				}
				num = 1;
			}
			IL_007F:
			return this._bDetailSetting == other._bDetailSetting;
		}

		// Token: 0x040002E3 RID: 739
		private Pane _pane;

		// Token: 0x040002E4 RID: 740
		private Material _simple;

		// Token: 0x040002E5 RID: 741
		private Material_Revo _detail;

		// Token: 0x040002E6 RID: 742
		private bool _bOutTexture;

		// Token: 0x040002E7 RID: 743
		private bool _bDetailSetting;

		// Token: 0x040002E8 RID: 744
		private int _binaryIndex;

		// Token: 0x040002E9 RID: 745
		public string name;

		// Token: 0x040002EA RID: 746
		public ColorS10_4[] tevColReg;

		// Token: 0x040002EB RID: 747
		public Color4[] tevConstReg;

		// Token: 0x040002EC RID: 748
		public TexMap[] texMap;

		// Token: 0x040002ED RID: 749
		public TexMatrix[] texMatrix;

		// Token: 0x040002EE RID: 750
		public TexCoordGen[] texCoordGen;

		// Token: 0x040002EF RID: 751
		public Material_RevoChannelControl[] channelControl;

		// Token: 0x040002F0 RID: 752
		public Color4 matColReg;

		// Token: 0x040002F1 RID: 753
		public Material_RevoSwapTable[] swapTable;

		// Token: 0x040002F2 RID: 754
		public TexMatrix[] indirectMatrix;

		// Token: 0x040002F3 RID: 755
		public Material_RevoIndirectStage[] indirectStage;

		// Token: 0x040002F4 RID: 756
		public Material_RevoTevStage[] tevStage;

		// Token: 0x040002F5 RID: 757
		public Material_RevoAlphaCompare alphaCompare;

		// Token: 0x040002F6 RID: 758
		public Material_RevoBlendMode blendMode;
	}
}
