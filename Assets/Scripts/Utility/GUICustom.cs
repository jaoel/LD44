
using System.Collections.Generic;
using UnityEngine;

public static class GUICustom
{
    private static Texture2D _pixelTexture = null;

    public static bool Dropdown(Rect controlRect, string label, bool expanded, List<string> items, ref int selectedItem)
    {
        bool closeOnClickOutside = true;
        selectedItem = -1;


        if (GUI.Button(controlRect, label))
        {
            expanded = !expanded;
            closeOnClickOutside = false;
        }


        if (expanded)
        {
            Rect dropdownRect = new Rect(controlRect.x + controlRect.width, controlRect.y, controlRect.width, controlRect.height * items.Count);
            GUI.Box(dropdownRect, "");
            int offset = 0;
            for (int i = 0; i < items.Count; i++)
            {
                string item = items[i];
                Rect itemRect = controlRect;
                itemRect.x = dropdownRect.x;
                itemRect.y = dropdownRect.y + offset;
                if (GUI.Button(itemRect, item))
                {
                    closeOnClickOutside = false;
                    selectedItem = i;
                    expanded = !expanded;
                }
                offset += (int)controlRect.height;
            }
        }

        if (closeOnClickOutside && Event.current.type == EventType.MouseDown)
        {
            expanded = false;
        }

        return expanded;
    }

    public static void Separator(Rect controlRect)
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.normal.background = PixelTexture();

        Rect separatorRect = controlRect;
        separatorRect.y = controlRect.y + controlRect.height / 2 + 3;
        separatorRect.height = 1;
        GUI.Box(separatorRect, "", guiStyle);
    }

    private static Texture2D PixelTexture()
    {
        if(_pixelTexture == null)
        {
            _pixelTexture = new Texture2D(1, 1);
            _pixelTexture.SetPixel(0, 0, Color.white);
            _pixelTexture.Apply();
        }
        return _pixelTexture;
    }
}