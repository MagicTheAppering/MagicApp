using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Tesseract.Droid;
using Tesseract;
using Android.Graphics;
using System.IO;
using Android.Graphics.Drawables;
using Android.Content.Res;
using System.Threading.Tasks;

namespace Magic.Droid
{
    [Activity(Label = "TestOCR")]
    public class TestGrey : Activity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Set View
            SetContentView(Resource.Layout.TestGrey);

            //Get Buttons
            Button buttonCalc = FindViewById<Button>(Resource.Id.TestGreyButton1);

            //Event Listeners
            buttonCalc.Click += async delegate
            {
                byte[] imgbyte = await getBytesFromBitmap(Resource.Drawable.testAE);
                showImage(imgbyte);
            };
        }

        //Bitmap in Bytestring umwandeln
        public async System.Threading.Tasks.Task<byte[]> getBytesFromBitmap(int img)
        {
            BitmapFactory.Options options = new BitmapFactory.Options
            {
                InJustDecodeBounds = false
            };

            Bitmap result = await BitmapFactory.DecodeResourceAsync(Resources, img);

            byte[] bitmapData;
            using (var stream = new MemoryStream())
            {
                result.Compress(Bitmap.CompressFormat.Jpeg, 0, stream);
                bitmapData = stream.ToArray();
            }

            return bitmapData;
        }

        //Image an die ImageView Activity übergeben und anzeigen
        public void showImage(byte[] byteImg)
        {
            var intent = new Intent(this, typeof(ImageViewOCR));

            //Bundle erzeugen und ByteArray speichern
            Bundle b = new Bundle();
            b.PutByteArray("img", byteImg);

            //Bundle zu Intent hinzufügen
            intent.PutExtra("img", b);

            //Activity starten
            StartActivity(intent);
        }
    }
}