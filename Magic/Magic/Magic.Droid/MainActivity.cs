
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;

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
			Button button = FindViewById<Button> (Resource.Id.MainButtonCam);
			
            //Event Listeners
			button.Click += delegate 
            {
                var intent = new Intent(this, typeof(CameraPreviewActivity));
                StartActivity(intent);
            };
		}
	}
}


