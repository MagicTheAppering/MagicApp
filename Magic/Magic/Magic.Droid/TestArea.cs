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

namespace Magic.Droid
{
    [Activity(Label = "SelectImageGallery")]
    class TestArea: Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.TestArea);

            var buttonSelectImageGallery = FindViewById<Button>(Resource.Id.TestAreaSelectImageGallery);

            buttonSelectImageGallery.Click += delegate {

                var imageIntent = new Intent();
                imageIntent.SetType("image/*");
                imageIntent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(imageIntent, "Select photo"), 0);
            };

            var seekBar = FindViewById<SeekBar>(Resource.Id.TestAreaSeekBar1);
            var textView = FindViewById<TextView>(Resource.Id.TestAreaTextView);

            seekBar.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
             {
                 if (e.FromUser)
                 {
                     textView.Text = string.Format("The value of the SeekBar is " + e.Progress);
                 }
             };
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {

                var imageView = FindViewById<ImageView>(Resource.Id.TestAreaImageView);
                imageView.SetImageURI(data.Data);
            }
        }
    }
}