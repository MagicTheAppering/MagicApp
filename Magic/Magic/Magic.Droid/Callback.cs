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
using OpenCV.Android;

namespace Magic.Droid
{
    class Callback : BaseLoaderCallback
    {
        private readonly CameraBridgeViewBase mOpenCvCameraView;
        public Callback(Context context, CameraBridgeViewBase cameraView) : base(context)
        {
            mOpenCvCameraView = cameraView;
        }

        public override void OnManagerConnected(int status)
        {
            switch (status)
            {
                case LoaderCallbackInterface.Success:
                    {
                        Android.Util.Log.Info("Bla", "OpenCV loaded successfully");
                        mOpenCvCameraView.EnableView();
                    }
                    break;
                default:
                    {
                        base.OnManagerConnected(status);
                    }
                    break;
            }
        }
    }
}