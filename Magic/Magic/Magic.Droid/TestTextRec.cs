using System;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpenCV.Android;
using OpenCV.Core;
using Android.Util;
using OpenCV.ImgProc;
using Size = OpenCV.Core.Size;
using Java.Util;
using System.IO;
using System.Threading.Tasks;
using OpenCV.Features2d;
using System.Collections.Generic;

namespace Magic.Droid.TestRec
{
    [Activity(Label = "TestTextRec")]
    public class TestTextRec : Activity, CameraBridgeViewBase.ICvCameraViewListener2
    {

        private CameraBridgeViewBase mOpenCvCameraView;

        private Mat mIntermediateMat;

        private Mat mGrey, mRgba;

        private Callback mLoaderCallback;

        private Scalar CONTOUR_COLOR = new Scalar(0, 255, 255);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Set View
            SetContentView(Resource.Layout.TestTextRec);


            //Open CV
            mOpenCvCameraView = FindViewById<CameraBridgeViewBase>(Resource.Id.TestTextRecView);
            mOpenCvCameraView.Visibility = ViewStates.Visible;
            mOpenCvCameraView.SetCvCameraViewListener2(this);
            mLoaderCallback = new Callback(this, mOpenCvCameraView);
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (mOpenCvCameraView != null)
                mOpenCvCameraView.DisableView();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (mOpenCvCameraView != null)
                mOpenCvCameraView.DisableView();
        }



        protected override void OnResume()
        {
            base.OnResume();
            if (!OpenCVLoader.InitDebug())
            {
                Log.Debug("Bla", "Internal OpenCV library not found. Using OpenCV Manager for initialization");
                OpenCVLoader.InitAsync(OpenCVLoader.OpencvVersion300, this, mLoaderCallback);
            }
            else
            {
                Log.Debug("Bla", "OpenCV library found inside package. Using it!");
                mLoaderCallback.OnManagerConnected(LoaderCallbackInterface.Success);
            }
        }



        public void OnCameraViewStarted(int width, int height)
        {
            mIntermediateMat = new Mat();
            mGrey = new Mat(height, width, CvType.Cv8uc4);
            mRgba = new Mat(height, width, CvType.Cv8uc4);
        }

        public void OnCameraViewStopped()
        {
            // Explicitly deallocate Mats
            if (mIntermediateMat != null)
                mIntermediateMat.Release();

            mIntermediateMat = null;
        }        



        public Mat OnCameraFrame(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
        {
            mGrey = inputFrame.Gray();
            mRgba = inputFrame.Rgba();

            detectText();
            return mRgba;
        }



        private void detectText()
        {
            MatOfKeyPoint keypoint = new MatOfKeyPoint();
            //IList oder List?
            IList<KeyPoint> listpoint;
            KeyPoint kpoint;
            //Cvtype richtig?
            Mat mask = Mat.Zeros(mGrey.Size(), CvType.Cv8uc1);
            int rectanx1;
            int rectany1;
            int rectanx2;
            int rectany2;
            int imgsize = mGrey.Height() * mGrey.Width();
            Scalar zeos = new Scalar(0, 0, 0);

            List<MatOfPoint> contour2 = new List<MatOfPoint>();
            //CvType?
            Mat kernel = new Mat(1, 50, CvType.Cv8uc1, Scalar.All(255));
            Mat morbyte = new Mat();
            Mat hierarchy = new Mat();

            Rect rectan3;
            //
            FeatureDetector detector = FeatureDetector
                    .Create(FeatureDetector.Mser);
            detector.Detect(mGrey, keypoint);
            listpoint = keypoint.ToList();
            
            //Counter?
            for (int ind = 0; ind < listpoint.Count; ind++)
            {
                kpoint = listpoint[ind];
                rectanx1 = (int)(kpoint.Pt.X - 0.5 * kpoint.Size);
                rectany1 = (int)(kpoint.Pt.Y - 0.5 * kpoint.Size);
                rectanx2 = (int)(kpoint.Size);
                rectany2 = (int)(kpoint.Size);
                if (rectanx1 <= 0)
                    rectanx1 = 1;
                if (rectany1 <= 0)
                    rectany1 = 1;
                if ((rectanx1 + rectanx2) > mGrey.Width())
                    rectanx2 = mGrey.Width() - rectanx1;
                if ((rectany1 + rectany2) > mGrey.Height())
                    rectany2 = mGrey.Height() - rectany1;
                Rect rectant = new Rect(rectanx1, rectany1, rectanx2, rectany2);
                try
                {
                    Mat roi = new Mat(mask, rectant);
                    roi.SetTo(CONTOUR_COLOR);
                }
                catch (Exception ex)
                {
                    Log.Debug("mylog", "mat roi error " + ex.Message);
                }
            }

            Imgproc.MorphologyEx(mask, morbyte, Imgproc.MorphDilate, kernel);
            Imgproc.FindContours(morbyte, contour2, hierarchy,
                    Imgproc.RetrExternal, Imgproc.ChainApproxNone);
            for (int ind = 0; ind < contour2.Count; ind++)
            {
                rectan3 = Imgproc.BoundingRect(contour2[ind]);
                rectan3 = Imgproc.BoundingRect(contour2[ind]);
                if (rectan3.Area() > 0.5 * imgsize || rectan3.Area() < 100
                        || rectan3.Width / rectan3.Height < 2)
                {
                    Mat roi = new Mat(morbyte, rectan3);
                    roi.SetTo(zeos);

                }
                else
                    Imgproc.Rectangle(mRgba, rectan3.Br(), rectan3.Tl(),
                            CONTOUR_COLOR);
            }
        }



    }

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
                        Log.Info("Bla", "OpenCV loaded successfully");
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