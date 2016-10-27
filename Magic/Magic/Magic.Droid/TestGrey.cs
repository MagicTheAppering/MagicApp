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

namespace Magic.Droid
{
    [Activity(Label = "TestOCR")]
    public class TestGrey : Activity, CameraBridgeViewBase.ICvCameraViewListener2
    {

        private Mat imgMat;

        public const string TAG = "OCVSample::Activity";

        public const int VIEW_MODE_RGBA = 0;
        public const int VIEW_MODE_HIST = 1;
        public const int VIEW_MODE_CANNY = 2;
        public const int VIEW_MODE_SEPIA = 3;
        public const int VIEW_MODE_SOBEL = 4;
        public const int VIEW_MODE_ZOOM = 5;
        public const int VIEW_MODE_PIXELIZE = 6;
        public const int VIEW_MODE_POSTERIZE = 7;
        
        private CameraBridgeViewBase mOpenCvCameraView;

        private Size mSize0;

        private Mat mIntermediateMat;
        private Mat mMat0;
        private MatOfInt[] mChannels;
        private MatOfInt mHistSize;
        private int mHistSizeNum = 25;
        private MatOfFloat mRanges;
        private Scalar[] mColorsRGB;
        private Scalar[] mColorsHue;
        private Scalar mWhilte;
        private OpenCV.Core.Point mP1;
        private OpenCV.Core.Point mP2;
        private float[] mBuff;
        private Mat mSepiaKernel;

        public static int viewMode = VIEW_MODE_RGBA;
        private Callback mLoaderCallback;

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

            //Event Listeners
            buttonCalc.Click += async delegate
            {
                Bitmap result = await greyImg(Resource.Drawable.test1);

                byte[] imgbyte = getBytesFromBitmap(result);

                showImage(imgbyte);
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
            mIntermediateMat = new Mat();
            mSize0 = new Size();
            mChannels = new MatOfInt[] { new MatOfInt(0), new MatOfInt(1), new MatOfInt(2) };
            mBuff = new float[mHistSizeNum];
            mHistSize = new MatOfInt(mHistSizeNum);
            mRanges = new MatOfFloat(0f, 256f);
            mMat0 = new Mat();
            mColorsRGB = new Scalar[] { new Scalar(200, 0, 0, 255), new Scalar(0, 200, 0, 255), new Scalar(0, 0, 200, 255) };
            mColorsHue = new Scalar[] {
                new Scalar(255, 0, 0, 255),   new Scalar(255, 60, 0, 255),  new Scalar(255, 120, 0, 255), new Scalar(255, 180, 0, 255), new Scalar(255, 240, 0, 255),
                new Scalar(215, 213, 0, 255), new Scalar(150, 255, 0, 255), new Scalar(85, 255, 0, 255),  new Scalar(20, 255, 0, 255),  new Scalar(0, 255, 30, 255),
                new Scalar(0, 255, 85, 255),  new Scalar(0, 255, 150, 255), new Scalar(0, 255, 215, 255), new Scalar(0, 234, 255, 255), new Scalar(0, 170, 255, 255),
                new Scalar(0, 120, 255, 255), new Scalar(0, 60, 255, 255),  new Scalar(0, 0, 255, 255),   new Scalar(64, 0, 255, 255),  new Scalar(120, 0, 255, 255),
                new Scalar(180, 0, 255, 255), new Scalar(255, 0, 255, 255), new Scalar(255, 0, 215, 255), new Scalar(255, 0, 85, 255),  new Scalar(255, 0, 0, 255)
        };
            mWhilte = Scalar.All(255);
            mP1 = new OpenCV.Core.Point();
            mP2 = new OpenCV.Core.Point();

            // Fill sepia kernel
            mSepiaKernel = new Mat(4, 4, CvType.Cv32f);
            mSepiaKernel.Put(0, 0, /* R */0.189f, 0.769f, 0.393f, 0f);
            mSepiaKernel.Put(1, 0, /* G */0.168f, 0.686f, 0.349f, 0f);
            mSepiaKernel.Put(2, 0, /* B */0.131f, 0.534f, 0.272f, 0f);
            mSepiaKernel.Put(3, 0, /* A */0.000f, 0.000f, 0.000f, 1f);
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

            //Matrix für das Bild
            imgMat = new Mat();

            //Bild zu Matrix umwandeln
            Utils.BitmapToMat(result, imgMat);

            //Bild bearbeiten
            Bitmap bmp = null;
            Mat tmp = new Mat(100,100, CvType.Cv8uc1, new Scalar(4));
            try
            {
                Imgproc.CvtColor(imgMat, tmp, Imgproc.ColorBgr2gray, 4);
                bmp = Bitmap.CreateBitmap(tmp.Cols(), tmp.Rows(), Bitmap.Config.Argb8888);
                Utils.MatToBitmap(tmp, bmp);
            }
            catch (CvException e) { Console.WriteLine(""+ e.ToString()); }


            return bmp;

        }

        //Image an die ImageView Activity übergeben und anzeigen
        public void showImage(byte[] byteImg)
        {
            var intent = new Intent(this, typeof(ImageViewOCR));

            //Bundle erzeugen und ByteArray speichern
            Bundle b = new Bundle();
            b.PutByteArray("img", byteImg);

            //Bundle zu Intent hinzufügen
            intent.PutExtra("img", b);

            //Activity starten
            StartActivity(intent);
        }



        public Mat OnCameraFrame(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
        {
            Mat rgba = inputFrame.Rgba();
            Size sizeRgba = rgba.Size();

            int rows = (int)sizeRgba.Height;
            int cols = (int)sizeRgba.Width;

            int left = cols / 8;
            int top = rows / 8;

            int width = cols * 3 / 4;
            int height = rows * 3 / 4;

            

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