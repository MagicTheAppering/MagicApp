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
using Android.Graphics;

namespace Magic.Droid
{
    [Activity(Label = "ImageViewOCR")]
    public class ImageViewOCR : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Bundle aus Intent 
            Bundle b = Intent.GetBundleExtra("img");
            //Img aus Bundle 
            byte[] imgbyte = b.GetByteArray("img");

            //Layout
            SetContentView(Resource.Layout.ImageViewOCR);

            //ImageView
            ImageView img = FindViewById<ImageView>(Resource.Id.ImageViewOCRView);


            //ByteArray zu Image umwandeln
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imgbyte, 0, imgbyte.Length);

            //Img darstellen
            img.SetImageBitmap(bitmap);
        }
    }
}