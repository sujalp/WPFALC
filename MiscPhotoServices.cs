using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WPFALC
{
    class MiscPhotoServices
    {
        public static void CreateThumbnail(string source, string dest)
        {
            try
            {
                System.Drawing.Image src_image = System.Drawing.Image.FromFile(source);

                int smallDim = 100;
                int SMALLWIDTH, SMALLHEIGHT;
                if (src_image.Width > src_image.Height)
                {
                    SMALLWIDTH = smallDim;
                    SMALLHEIGHT = (int)((((decimal)SMALLWIDTH) * src_image.Height) / src_image.Width);
                }
                else
                {
                    SMALLHEIGHT = smallDim;
                    SMALLWIDTH = (int)((((decimal)SMALLHEIGHT) * src_image.Width) / src_image.Height);
                }
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(SMALLWIDTH, SMALLHEIGHT, src_image.PixelFormat);
                System.Drawing.Graphics new_g = System.Drawing.Graphics.FromImage(bitmap);

                new_g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                new_g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                new_g.DrawImage(src_image, 0, 0, bitmap.Width, bitmap.Height);
                bitmap.Save(dest, System.Drawing.Imaging.ImageFormat.Jpeg);

                return;
            }
            catch
            {
                return;
            }
        }

        public static string GetDateTimeStamp(string source)
        {
            string decoded = "";

            System.Drawing.Image src_image = System.Drawing.Image.FromFile(source);
            System.Drawing.Imaging.PropertyItem[] pis = src_image.PropertyItems;
            foreach (System.Drawing.Imaging.PropertyItem p in pis)
            {
                if (p.Id == 0x132)
                {
                    ASCIIEncoding ascii = new ASCIIEncoding();
                    decoded = ascii.GetString(p.Value, 0, 19);
                    Console.WriteLine(decoded);
                }
            }

            return decoded;
        }
    }
}