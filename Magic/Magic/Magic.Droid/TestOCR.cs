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
    public class TestOCR : Activity
    {

        TesseractApi api;
        TextView result;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Set View
            SetContentView(Resource.Layout.OCRTest);

            //Get Textview
            result = FindViewById<TextView>(Resource.Id.OCRTestText);

            //Get Buttons
            Button buttonOcr = FindViewById<Button>(Resource.Id.OCRTestButton1);

            //Event Listeners
            buttonOcr.Click += delegate
            {
                getText();
            };

            //Tesseract Api einmal bei start erzeugen
            api = new TesseractApi(this, AssetsDeployment.OncePerVersion);

            //Warten auf initialisierung
            bool check = await initTes();

        }

        //Text aus Bild
        public async void getText()
        {
                byte[] img = await getBytesFromBitmap(Resource.Drawable.test);

                bool success = await api.SetImage(img);
                if (success)
                {
                    string textResult = api.Text;
                    result.Text = textResult;
                }

        }


    public async Task<bool> initTes()
        {
            //Copy Asset to sd if needed

            //AssetManager assets = this.Assets;

            //var androidPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

            //var dir = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/tessdata/");

            ////var tessPath = androidPath + "/tessdata";

            //if(!dir.Exists())
            //{
            //    dir.Mkdir();
            //}

            //var dataPath = Android.OS.Environment.ExternalStorageDirectory.Path + "/eng.traineddata";
            //using (var asset = Assets.Open("tessdata/eng.traineddata")) using (var dest = File.Create(dataPath)) asset.CopyTo(dest);

            //var asset = Assets.Open("tessdata/eng.traineddata");

            //bool initialised = await api.Init("/sdcard/", "eng");

            //datapath = FilesDir.Path + "Assets";

            //datapath = System.IO.Path.GetFullPath("eng.traineddata");

            bool initialised = await api.Init("eng");

            return initialised;
        }

        //Bitmap in Bytestring umwandeln
        //------------------Todo: img link einfügen
        public async System.Threading.Tasks.Task<byte[]> getBytesFromBitmap(int img)
        {
            BitmapFactory.Options options = new BitmapFactory.Options
            {
                InJustDecodeBounds = false
            };

        
            Bitmap result = await BitmapFactory.DecodeResourceAsync(Resources, Resource.Drawable.test);

            byte[] bitmapData;
            using (var stream = new MemoryStream())
            {
                result.Compress(Bitmap.CompressFormat.Jpeg, 0, stream);
                bitmapData = stream.ToArray();
            }

            return bitmapData;
        }
    }
}