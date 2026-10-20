namespace Jaket.Content;

using UnityEngine;

using Jaket.Assets;

/// <summary> Skin color presets for the player's chassis/doll. </summary>
public static class SkinColors
{
    static PrefsManager pm => PrefsManager.Instance;

    public struct Preset
    {
        public string Key;
        public string Name;
        public Color32 Color;

        public Preset(string key, string name, Color32 color)
        {
            Key = key;
            Name = name;
            Color = color;
        }
    }

    public static readonly Preset[] All = new Preset[]
    {
        new("skin.v1",     "V1 (Синий)",          new Color32(255, 255, 255, 255)), // Natural V1 texture
        new("skin.v2",     "V2 (Красный)",        new Color32(235, 45, 45, 255)),   // V2 Red
        new("skin.v3",     "V3 (Зеленый)",        new Color32(50, 225, 75, 255)),   // V3 Green
        new("skin.gold",   "Золотой / Желтый",    new Color32(245, 215, 35, 255)),  // Gold
        new("skin.pink",   "Розовый (UwU)",       new Color32(255, 115, 205, 255)), // Pink
        new("skin.purple", "Фиолетовый",          new Color32(185, 75, 255, 255)),  // Gabriel Purple
        new("skin.orange", "Оранжевый",           new Color32(255, 135, 30, 255)),  // Swordsmachine Orange
        new("skin.white",  "Мираж (Белый)",       new Color32(230, 235, 245, 255)), // Mirage White
        new("skin.shadow", "Стелс (Тёмный)",      new Color32(65, 70, 80, 255)),    // Stealth Black
        new("skin.cyan",   "Минос (Бирюзовый)",   new Color32(35, 215, 255, 255)),  // Minos Cyan
    };

    /// <summary> Index of the chosen doll skin color. </summary>
    public static int SelectedIndex
    {
        get => pm.GetInt("jaket.skin-color", 0);
        set => pm.SetInt("jaket.skin-color", (value % All.Length + All.Length) % All.Length);
    }

    /// <summary> Current chosen doll skin color. </summary>
    public static Color32 CurrentColor => Color(SelectedIndex);

    /// <summary> Name of the current chosen doll skin color. </summary>
    public static string CurrentName => Name(SelectedIndex);

    public static Color32 Color(int index) => All[Mathf.Clamp(index, 0, All.Length - 1)].Color;

    public static string Name(int index)
    {
        int i = Mathf.Clamp(index, 0, All.Length - 1);
        return Bundle.Get(All[i].Key, All[i].Name);
    }
}
