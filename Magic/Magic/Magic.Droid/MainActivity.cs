
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Magic.Droid.Examples;
using Magic.Droid.Examples.CameraControl;

namespace Magic.Droid
{
    [Activity (Label = "Magic.Droid", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			//Set View
			SetContentView (Resource.Layout.Main);

            //Get Buttons
			Button buttonCam = FindViewById<Button> (Resource.Id.MainButtonCam);
            Button buttonBlob = FindViewById<Button>(Resource.Id.MainButtonBlob);
            Button buttonControl = FindViewById<Button>(Resource.Id.MainButtonCameraControl);

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

            buttonControl.Click += delegate
            {
                var intent = new Intent(this, typeof(CameraControlActivity));
                StartActivity(intent);
            };
        }
	}
}


