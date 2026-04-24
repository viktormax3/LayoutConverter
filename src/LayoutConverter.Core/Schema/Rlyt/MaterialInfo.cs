using System;

namespace LayoutConverter.Core.Schema.Rlyt
{
	
	public class MaterialInfo
	{
		
		// (get) Token: 0x06000069 RID: 105 RVA: 0x00002C48 File Offset: 0x00001C48
		public Pane Pane
		{
			get
			{
				return this._pane;
			}
		}

// (get) Token: 0x0600006A RID: 106 RVA: 0x00002C5C File Offset: 0x00001C5C
		public bool IsOutTexture
		{
			get
			{
				return this._bOutTexture;
			}
		}

// (get) Token: 0x0600006B RID: 107 RVA: 0x00002C70 File Offset: 0x00001C70
		public bool IsDetailSetting
		{
			get
			{
				return this._bDetailSetting;
			}
		}

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

public MaterialInfo(Pane pane, Material simple, Material_Revo detail, bool bOutTexture, bool bDetailSetting)
		{
			this._pane = pane;
			this._simple = simple;
			this._detail = detail;
			this._bOutTexture = bOutTexture;
			this._bDetailSetting = bDetailSetting;
		}

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

private Pane _pane;

private Material _simple;

private Material_Revo _detail;

private bool _bOutTexture;

private bool _bDetailSetting;

private int _binaryIndex;

public string name;

public ColorS10_4[] tevColReg;

public Color4[] tevConstReg;

public TexMap[] texMap;

public TexMatrix[] texMatrix;

public TexCoordGen[] texCoordGen;

public Material_RevoChannelControl[] channelControl;

public Color4 matColReg;

public Material_RevoSwapTable[] swapTable;

public TexMatrix[] indirectMatrix;

public Material_RevoIndirectStage[] indirectStage;

public Material_RevoTevStage[] tevStage;

public Material_RevoAlphaCompare alphaCompare;

public Material_RevoBlendMode blendMode;
	}
}
