using UnityEngine;
using System.Collections.Generic;
using System;

//popup system used to display ColorField's ColorPicker and settings windows.
namespace PW
{
	public static class PWPopup {
	
		public const int SHOW_COLORPICKER			= 0;
		public const int SHOW_SAMPLER2D_SETTINGS	= 1;
		public const int SHOW_SAMPLER3D_SETTINGS	= 2;
		public const int SHOW_TEXTURE_SETTINGS		= 3;
	
		private const int POPUP_COUNT = 4;

		private static int					toRender;
		private static PWPopupSettings[]	toRenderDatas = new PWPopupSettings[POPUP_COUNT];

		public static void AddToRender(PWPopupSettings settings, int type)
		{
			toRender |= (1 << type);
			toRenderDatas[type] = settings;
		}

		public static void RenderAll()
		{
			for (int i = 0; i < POPUP_COUNT; i++)
				if ((toRender & (1 << i)) != 0)
					RenderPopup(toRenderDatas[i], i);
		}

		private static void RenderPopup(PWPopupSettings data, int type)
		{
			switch (type)
			{
				case SHOW_COLORPICKER:
					RenderColorPicker(data as PWColorPickerSettings);
					break ;
				case SHOW_SAMPLER2D_SETTINGS:
					break ;
				case SHOW_SAMPLER3D_SETTINGS:
					break ;
				case SHOW_TEXTURE_SETTINGS:
					break ;
			}
		}
	
		private static void RenderColorPicker(PWColorPickerSettings data)
		{
			
		}

	}
}