
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Magic.Droid.Examples;
using Magic.Droid.Examples.CameraControl;
using Magic.Droid.Examples.Manipulation;

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
            Button buttonManipulation = FindViewById<Button>(Resource.Id.MainButtonImageManipulation);
            Button buttonOCRTest = FindViewById<Button>(Resource.Id.MainButtonOCRTest);
            Button buttonGreyTest = FindViewById<Button>(Resource.Id.MainButtonGreyTest);

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

            buttonManipulation.Click += delegate
            {
                var intent = new Intent(this, typeof(ImageManipulationsActivity));
                StartActivity(intent);
            };

            buttonOCRTest.Click += delegate
            {
                var intent = new Intent(this, typeof(TestOCR));
                StartActivity(intent);
            };

            buttonGreyTest.Click += delegate
            {
                var intent = new Intent(this, typeof(TestGrey));
                StartActivity(intent);
            };
        }
	}
}


