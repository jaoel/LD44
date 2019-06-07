
using System.Collections.Generic;
using UnityEngine;

public static class GUICustom
{
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
}