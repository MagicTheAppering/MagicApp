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
using OpenCV.Core;
using Android.Util;
using OpenCV.ImgProc;
using Size = OpenCV.Core.Size;
using Java.Util;
using Android.Graphics;
using System.IO;
using System.Threading.Tasks;
using OpenCV.Features2d;

namespace Magic.Droid
{
    [Activity(Label = "TestGrey")]
    public class TestGrey : Activity, CameraBridgeViewBase.ICvCameraViewListener2
    {
        
        private CameraBridgeViewBase mOpenCvCameraView;               

        private Mat mIntermediateMat;

        private Callback mLoaderCallback;

        private Scalar CONTOUR_COLOR = new Scalar(0, 255, 0);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Set View
            SetContentView(Resource.Layout.TestGrey);


            //Open CV
            mOpenCvCameraView = FindViewById<CameraBridgeViewBase>(Resource.Id.TestGreyView);
            mOpenCvCameraView.Visibility = ViewStates.Visible;
            mOpenCvCameraView.SetCvCameraViewListener2(this);
            mLoaderCallback = new Callback(this, mOpenCvCameraView);

            //Get Buttons
            Button buttonCalc = FindViewById<Button>(Resource.Id.TestGreyButton1);
            Button buttonDetect = FindViewById<Button>(Resource.Id.TestGreyButton2);

            //Event Listeners
            buttonCalc.Click += async delegate
            {
                Bitmap result = await greyImg(Resource.Drawable.test1);

                byte[] imgbyte = getBytesFromBitmap(result);

                showImage(imgbyte);
            };

            buttonDetect.Click += delegate
            {
               detectText(Resource.Drawable.test);               

            };
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
            
        }

        public void OnCameraViewStopped()
        {
            // Explicitly deallocate Mats
            if (mIntermediateMat != null)
                mIntermediateMat.Release();

            mIntermediateMat = null;
        }

        //Bitmap in Bytestring umwandeln
        public byte[] getBytesFromBitmap(Bitmap img)
        {
            BitmapFactory.Options options = new BitmapFactory.Options
            {
                InJustDecodeBounds = false
            };


            byte[] bitmapData;
            using (var stream = new MemoryStream())
            {
                img.Compress(Bitmap.CompressFormat.Jpeg, 0, stream);
                bitmapData = stream.ToArray();
            }

            return bitmapData;
        }


        public async Task<Bitmap> greyImg(int img)
        {
            //Bitmap laden
            Bitmap result = await BitmapFactory.DecodeResourceAsync(Resources, img);

            //Matrix f�r das Bild
            Mat imgMat = new Mat();

            //Bild zu Matrix umwandeln
            Utils.BitmapToMat(result, imgMat);




            //-----------------Bild bearbeiten---------------------

            //Variablen
            Size s = new Size(10.0, 10.0);
            OpenCV.Core.Point p = new OpenCV.Core.Point(0, 0);

            //TODO Matrix gr��e beachten?
            Bitmap bmp = null;
            Mat tmpgrey = new Mat(10,10, CvType.Cv8uc1, new Scalar(4));
            Mat tmpblur = new Mat(10, 10, CvType.Cv8uc1, new Scalar(4));
            Mat tmpthresh = new Mat(10, 10, CvType.Cv8uc1, new Scalar(4));
            Mat imgresult = new Mat(10, 10, CvType.Cv8uc1, new Scalar(4));
            try
            {
                //Grau
                Imgproc.CvtColor(imgMat, tmpgrey, Imgproc.ColorBgr2gray, 4);

                //Blur
                Imgproc.Blur(tmpgrey, tmpblur, s,p);

                //Thresh
                Imgproc.Threshold(tmpblur, tmpthresh, 60, 255, Imgproc.ThreshBinary);

                //Kontrast
                //tmpthresh.ConvertTo(imgresult, -1, 9.0, 10);

                bmp = Bitmap.CreateBitmap(tmpthresh.Cols(), tmpthresh.Rows(), Bitmap.Config.Argb8888);
                Utils.MatToBitmap(tmpthresh, bmp);
            }
            catch (CvException e) { Console.WriteLine(""+ e.ToString()); }


            return bmp;

        }

        //Image an die ImageView Activity �bergeben und anzeigen
        public void showImage(byte[] byteImg)
        {
            var intent = new Intent(this, typeof(ImageViewOCR));

            //Bundle erzeugen und ByteArray speichern
            Bundle b = new Bundle();
            b.PutByteArray("img", byteImg);

            //Bundle zu Intent hinzuf�gen
            intent.PutExtra("img", b);

            //Activity starten
            StartActivity(intent);
        }



        private async void detectText(int img)
        {

            //Bitmap laden
            Bitmap result = await BitmapFactory.DecodeResourceAsync(Resources, img);

            Bitmap resultgrey = await greyImg(img);

            //Matrix f�r die Bilder
            Mat mGrey = new Mat();
            Mat mRGB = new Mat();

            //Bild zu Matrix umwandeln
            Utils.BitmapToMat(result, mGrey);
            Utils.BitmapToMat(result, mRGB);


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

            OpenCV.Core.Rect rectan3;
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
                OpenCV.Core.Rect rectant = new OpenCV.Core.Rect(rectanx1, rectany1, rectanx2, rectany2);
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
                    //Sicher mgrey?
                    Imgproc.Rectangle(mRGB, rectan3.Br(), rectan3.Tl(),
                            CONTOUR_COLOR);
            }

            Bitmap resultrgb;
            resultrgb = Bitmap.CreateBitmap(mRGB.Cols(), mRGB.Rows(), Bitmap.Config.Argb8888);
            Utils.MatToBitmap(mRGB, resultrgb);



            showImage(getBytesFromBitmap(resultrgb));
        }



        public Mat OnCameraFrame(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
        {
            Mat rgba = inputFrame.Rgba();
            //Size sizeRgba = rgba.Size();

            //int rows = (int)sizeRgba.Height;
            //int cols = (int)sizeRgba.Width;

            //int left = cols / 8;
            //int top = rows / 8;

            //int width = cols * 3 / 4;
            //int height = rows * 3 / 4;

            

            return rgba;
        }
    }

    class Callback : BaseLoaderCallback
    {
        private readonly CameraBridgeViewBase mOpenCvCameraView;
        public Callback(Context context, CameraBridgeViewBase cameraView): base(context)
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