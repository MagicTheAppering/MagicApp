using Android.Graphics;
using Magic.Shared.imgop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract.Droid;

namespace Magic.Shared.magicocr
{
    static class OCR
    {
        private static TesseractApi api = null;

        static OCR()
        {
        }

        //Api starten und Trainingsdaten laden
        public static async Task<bool> initTes(TesseractApi api)
        {
            bool initialised = await api.Init("eng");

            return initialised;
        }

        //Text aus Bild
        public static async Task<string> getText(Bitmap img)
        {
            if(api != null)
            {
                string textResult = "";

                Bitmap grey = ImageOp.greyImg(img);
                byte[] greyByte = ImageOp.getBytesFromBitmap(grey);

                bool success = await api.SetImage(greyByte);
                if (success)
                {
                    textResult = api.Text;
                }
                return textResult;
            }
            else
            {
                System.Console.WriteLine("Error API not Set");
            }
            return null;
            

        }

    }
}
