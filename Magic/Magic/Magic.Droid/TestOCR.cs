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

        private TesseractApi api;

        private TextView result;

        byte[] img;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Set View
            SetContentView(Resource.Layout.OCRTest);

            //Get Textview
            result = FindViewById<TextView>(Resource.Id.OCRTestText);

            //Get Buttons
            Button buttonOcr = FindViewById<Button>(Resource.Id.OCRTestButton1);
            Button buttonShow = FindViewById<Button>(Resource.Id.OCRTestButton2);

            //Event Listeners
            buttonOcr.Click += delegate
            {
                getText();
            };

            buttonShow.Click += delegate
            {
                showImage(img);
            };

            //Tesseract Api einmal bei start erzeugen
            api = new TesseractApi(this, AssetsDeployment.OncePerVersion);

            //Warten auf initialisierung
            bool check = await initTes();

            //Image von Resource laden
            img = await getBytesFromBitmap(Resource.Drawable.test);
        }

        //Text aus Bild
        public async void getText()
        {
            bool success = await api.SetImage(img);
            if (success)
            {
                string textResult = api.Text;
                result.Text = textResult;  
            }
        }

        //Api starten und Trainingsdaten laden
        public async Task<bool> initTes()
        {
            bool initialised = await api.Init("eng");

            return initialised;
        }

        //Bitmap in Bytestring umwandeln
        //------------------Todo: img link einf�gen
        public async System.Threading.Tasks.Task<byte[]> getBytesFromBitmap(int img)
        {
            BitmapFactory.Options options = new BitmapFactory.Options
            {
                InJustDecodeBounds = false
            };

        
            Bitmap result = await BitmapFactory.DecodeResourceAsync(Resources, Resource.Drawable.testAE);

            byte[] bitmapData;
            using (var stream = new MemoryStream())
            {
                result.Compress(Bitmap.CompressFormat.Jpeg, 0, stream);
                bitmapData = stream.ToArray();
            }

            return bitmapData;
        }

        //ByteArray zu IMG umwandeln und speichern
        public void saveImage(byte[] byteImg)
        {
            //ByteArray to Image
            Bitmap bitmap = BitmapFactory.DecodeByteArray(byteImg, 0, byteImg.Length);

            //Speicherort
            var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

            //Create a folder for the images if not exists
            Directory.CreateDirectory(System.IO.Path.Combine(sdCardPath, "images"));

            //Pfad erstellen
            string imatge = System.IO.Path.Combine(sdCardPath, "images", "image.jpg");

            //Umwandeln und speichern
            var stream = new FileStream(imatge, FileMode.Create);
            bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
            stream.Close();
        }

        //Image an die ImageView Activity �bergeben und anzeigen
        public void showImage(byte[] byteImg)
        {
            var intent = new Intent(this, typeof(ImageViewOCR));

            //Bundle erzeugen und ByteArray speichern
            Bundle b = new Bundle();
            b.PutByteArray("img", byteImg);

            //Bundle zu Intent hinzuf�gen
            intent.PutExtra("img",b);

            //Activity starten
            StartActivity(intent);
        }
    }
}