//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Glide
{
    
    internal partial class Resources
    {
        private static System.Resources.ResourceManager manager;
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if ((Resources.manager == null))
                {
                    Resources.manager = new System.Resources.ResourceManager("Glide.Resources", typeof(Resources).Assembly);
                }
                return Resources.manager;
            }
        }
        internal static nanoFramework.UI.Bitmap GetBitmap(Resources.BitmapResources id)
        {
            return ((nanoFramework.UI.Bitmap)(nanoFramework.Runtime.Native.ResourceUtility.GetObject(ResourceManager, id)));
        }
        internal static nanoFramework.UI.Font GetFont(Resources.FontResources id)
        {
            return ((nanoFramework.UI.Font)(nanoFramework.Runtime.Native.ResourceUtility.GetObject(ResourceManager, id)));
        }
        [System.SerializableAttribute()]
        internal enum BitmapResources : short
        {
            DropdownButton_Down = -31110,
            DataGridIcon_Desc = -27556,
            Keyboard_320x128_Up_Lowercase = -27169,
            Button_Down = -18532,
            Button_Up = -12892,
            ProgressBar_Fill = -12135,
            Modal = -11358,
            Keyboard_320x128_Up_Symbols = -5791,
            RadioButton = 1102,
            DropdownText_Up = 1846,
            Keyboard_320x128_Up_Uppercase = 2629,
            TextBox = 3718,
            DropdownButton_Up = 4119,
            loading = 6128,
            DropdownText_Down = 13519,
            ProgressBar = 15350,
            Keyboard_320x128_Up_Numbers = 16467,
            CheckBox_Off = 23409,
            DataGridIcon_Asc = 27566,
            CheckBox_On = 27788,
        }
        [System.SerializableAttribute()]
        internal enum FontResources : short
        {
            droid_reg10 = -27850,
            droid_reg12 = -27848,
            droid_reg11 = -27847,
            droid_reg14 = -27846,
            droid_reg18 = -27842,
            droid_reg24 = -13442,
            droid_reg09 = -7428,
            droid_reg08 = -7427,
            droid_reg32 = 14655,
            droid_reg48 = 24547,
        }
    }
}
