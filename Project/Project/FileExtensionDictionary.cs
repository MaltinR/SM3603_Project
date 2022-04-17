using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Project
{
    public static class FileExtensionDictionary
    {
        public enum AppEnum
        {
            TextEditor,
            Calculator,
            VideoPlayer,
            ImageEditor,
            FileExplorer,
            Other
        }

        public static List<BitmapImage> BitmapImages_Normal { get; private set; }
        public static List<BitmapImage> BitmapImages_Selecting { get; private set; }
        public static Dictionary<string, AppEnum> Dictionary { get; private set; }
        static FileExtensionDictionary()
        {
            Dictionary = new Dictionary<string, AppEnum>();
            Dictionary.Add("txt", AppEnum.TextEditor);
            Dictionary.Add("png", AppEnum.ImageEditor);
            Dictionary.Add("jpg", AppEnum.ImageEditor);
            Dictionary.Add("jpeg", AppEnum.ImageEditor);
            Dictionary.Add("bmp", AppEnum.ImageEditor);
            Dictionary.Add("mp4", AppEnum.VideoPlayer);
            Dictionary.Add("m4v", AppEnum.VideoPlayer);
            Dictionary.Add("mp4v", AppEnum.VideoPlayer);

            BitmapImages_Normal = new List<BitmapImage>();
            BitmapImages_Selecting = new List<BitmapImage>();

            BitmapImages_Normal.Add(new BitmapImage(new Uri("Images/Icon_TextEditor_Normal.png", UriKind.Relative)));
            BitmapImages_Normal.Add(new BitmapImage(new Uri("Images/Icon_Calculator_Normal.png", UriKind.Relative)));
            BitmapImages_Normal.Add(new BitmapImage(new Uri("Images/Icon_VideoPlayer_Normal.png", UriKind.Relative)));
            BitmapImages_Normal.Add(new BitmapImage(new Uri("Images/Icon_Image_Normal.png", UriKind.Relative)));
            BitmapImages_Normal.Add(new BitmapImage(new Uri("Images/Icon_FileExplorer_Normal.png", UriKind.Relative)));
            BitmapImages_Normal.Add(new BitmapImage(new Uri("Images/Icon_Other_Normal.png", UriKind.Relative)));

            BitmapImages_Selecting.Add(new BitmapImage(new Uri("Images/Icon_TextEditor_Selecting.png", UriKind.Relative)));
            BitmapImages_Selecting.Add(new BitmapImage(new Uri("Images/Icon_Calculator_Selecting.png", UriKind.Relative)));
            BitmapImages_Selecting.Add(new BitmapImage(new Uri("Images/Icon_VideoPlayer_Selecting.png", UriKind.Relative)));
            BitmapImages_Selecting.Add(new BitmapImage(new Uri("Images/Icon_Image_Selecting.png", UriKind.Relative)));
            BitmapImages_Selecting.Add(new BitmapImage(new Uri("Images/Icon_FileExplorer_Selecting.png", UriKind.Relative)));
            BitmapImages_Selecting.Add(new BitmapImage(new Uri("Images/Icon_Other_Selecting.png", UriKind.Relative)));
        }

        public static BitmapImage GetImage(AppEnum appEnum, bool isSelecting)
        {
            return isSelecting ? BitmapImages_Selecting[(int)appEnum] : BitmapImages_Normal[(int)appEnum];
        }

        public static AppEnum GetEnum(string ext)
        {
            ext = ext.ToLower();

            AppEnum appEnum;
            if (!Dictionary.TryGetValue(ext, out appEnum))
            {
                appEnum = AppEnum.Other;
            }

            return appEnum;
        }
    }
}
