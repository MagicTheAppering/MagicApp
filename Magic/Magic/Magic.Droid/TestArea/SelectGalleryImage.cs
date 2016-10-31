using System;

using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Provider;
using System.IO;

namespace Magic.Droid
{
    [Activity(Label = "SelectImageGallery")]
    public class SelectGalleryImage : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.SelectGalleryImage);

            var button = FindViewById<Button>(Resource.Id.MainSelectImageGallery1);

            button.Click += delegate {

                var imageIntent = new Intent();
                imageIntent.SetType("image/*");
                imageIntent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(imageIntent, "Select photo"), 0);
            };
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {

                var imageView = FindViewById<ImageView>(Resource.Id.MainImageView);
                imageView.SetImageURI(data.Data);
            }
        }

    }
}