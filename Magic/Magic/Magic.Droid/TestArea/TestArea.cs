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
using Tesseract.Droid;
using Magic.Shared.imgop;
using Magic.Shared.magicocr;

namespace Magic.Droid
{
    [Activity(Label = "TestGrey")]
    public class TestArea : Activity, CameraBridgeViewBase.ICvCameraViewListener2
    {
        
        private CameraBridgeViewBase mOpenCvCameraView;               

        private Mat mIntermediateMat;

        private Callback mLoaderCallback;

        private TesseractApi api;


        private TextView textseek1, textseek2;

        private Button buttonSelectImageGallery, buttonDetectText, buttonGrey, buttonExtractText;

        private SeekBar seek1, seek2;

        private ImageView img1, imgResult;


        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Set View
            SetContentView(Resource.Layout.TestArea);


            //Open CV
            mOpenCvCameraView = FindViewById<CameraBridgeViewBase>(Resource.Id.TestGreyView);
            mOpenCvCameraView.Visibility = ViewStates.Visible;
            mOpenCvCameraView.SetCvCameraViewListener2(this);
            mLoaderCallback = new Callback(this, mOpenCvCameraView);

            //Textview
            textseek1 = FindViewById<TextView>(Resource.Id.TestAreaTextSeek1);
            textseek2 = FindViewById<TextView>(Resource.Id.TestAreaTextSeek2);

            //Get Buttons
            buttonDetectText = FindViewById<Button>(Resource.Id.TestAreaButtonDetectText);
            buttonSelectImageGallery = FindViewById<Button>(Resource.Id.TestAreaSelectImageGallery);
            buttonExtractText = FindViewById<Button>(Resource.Id.TestAreaButtonExtractText);
            buttonGrey = FindViewById<Button>(Resource.Id.TestAreaButtonGrey);

            //ImageView
            img1 = FindViewById<ImageView>(Resource.Id.TestAreaImageView);
            imgResult = FindViewById<ImageView>(Resource.Id.TestAreaImageResult);

            //Event Listeners         
            buttonDetectText.Click += async delegate
            {                
                int img = Resource.Drawable.test2;
                Bitmap result = await BitmapFactory.DecodeResourceAsync(Resources, img);

                string text = await ImageOp.detectAndExtractText(result);
                Console.WriteLine("Ergebnis: " + text);                          

            };

            buttonGrey.Click += async delegate
            {
                
            };

            buttonExtractText.Click += async delegate
            {

            };



            buttonSelectImageGallery.Click += delegate
            {
                var imageIntent = new Intent();
                imageIntent.SetType("image/*");
                imageIntent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(imageIntent, "Select photo"), 0);
            };


            //Slider
            seek1 = FindViewById<SeekBar>(Resource.Id.TestAreaSeekBar1);
            seek2 = FindViewById<SeekBar>(Resource.Id.TestAreaSeekBar2);

            //Slider Listener
            seek1.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                    textseek1.Text = string.Format("Value " + e.Progress);
                }
            };

            seek2.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                    textseek2.Text = string.Format("Value " + e.Progress);
                }
            };

            // Tesseract Api einmal bei start erzeugen
            api = new TesseractApi(this, AssetsDeployment.OncePerVersion);
            bool check = await OCR.initTes(api);           
        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                img1.SetImageURI(data.Data);
            }
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


        //Image an die ImageView Activity übergeben und anzeigen
        public void showImage(byte[] byteImg)
        {
            var intent = new Intent(this, typeof(ImgPreview));

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
            return rgba;
        }        

    }  


    
}