
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Magic.Droid.Examples;

namespace Magic.Droid
{
    [Activity (Label = "Magic.Droid", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		int count = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			//Set View
			SetContentView (Resource.Layout.Main);

            //Get Buttons
			Button buttonCam = FindViewById<Button> (Resource.Id.MainButtonCam);
            Button buttonBlob = FindViewById<Button>(Resource.Id.MainButtonBlob);

            //Event Listeners
            buttonCam.Click += delegate 
            {
                var intent = new Intent(this, typeof(CameraPreviewActivity));
                StartActivity(intent);
            };

            buttonBlob.Click += delegate
            {
                var intent = new Intent(this, typeof(ColorBlobDetectionActivity));
                StartActivity(intent);
            };
        }
	}
}


