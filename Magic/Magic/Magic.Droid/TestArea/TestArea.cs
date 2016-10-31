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
using Android.Graphics.Drawables;

namespace Magic.Droid
{
    [Activity(Label = "TestGrey")]
    public class TestArea : Activity, CameraBridgeViewBase.ICvCameraViewListener2
    {
        
        private CameraBridgeViewBase mOpenCvCameraView;               

        private Mat mIntermediateMat;

        private Callback mLoaderCallback;

        private TesseractApi api;


        private TextView textseekThresh, textseekBlur, textseekSize, textResult;

        private Button buttonSelectImageGallery, buttonDetectText, buttonGrey, buttonExtractText, buttonSize;

        private SeekBar seekThresh, seekBlur, seekSize;

        private ImageView imgInput, imgResult;


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
            textseekThresh = FindViewById<TextView>(Resource.Id.TestAreaTextSeek1);
            textseekBlur = FindViewById<TextView>(Resource.Id.TestAreaTextSeek2);
            textseekSize = FindViewById<TextView>(Resource.Id.TestAreaTextSeek3);
            textResult = FindViewById<TextView>(Resource.Id.TestAreaResultText);
            
            //Get Buttons
            buttonDetectText = FindViewById<Button>(Resource.Id.TestAreaButtonDetectText);
            buttonSelectImageGallery = FindViewById<Button>(Resource.Id.TestAreaSelectImageGallery);
            buttonExtractText = FindViewById<Button>(Resource.Id.TestAreaButtonExtractText);
            buttonGrey = FindViewById<Button>(Resource.Id.TestAreaButtonGrey);
            buttonSize = FindViewById<Button>(Resource.Id.TestAreaButtonSize);

            //ImageView
            imgInput = FindViewById<ImageView>(Resource.Id.TestAreaImageView);
            imgResult = FindViewById<ImageView>(Resource.Id.TestAreaImageResultGrey);

            //Event Listeners         
            

            buttonGrey.Click += delegate
            {
                Bitmap img = ((BitmapDrawable)imgInput.Drawable).Bitmap;
                double thresh = Convert.ToDouble(textseekThresh.Text.ToString());
                double blur = Convert.ToDouble(textseekBlur.Text.ToString());
                Bitmap result = ImageOp.greyImg(img, thresh, blur);

                imgResult.SetImageBitmap(result);
            };

            buttonDetectText.Click += delegate
            {
                Bitmap img = ((BitmapDrawable)imgResult.Drawable).Bitmap;

                Bitmap result = ImageOp.detectTextRect(img);

                imgResult.SetImageBitmap(result);
            };


            buttonExtractText.Click += async delegate
            {
                Bitmap img = ((BitmapDrawable)imgResult.Drawable).Bitmap;

                string result = await ImageOp.detectAndExtractText(img);

                textResult.Text = result;
            };

            buttonSize.Click += delegate
            {
                Bitmap img = ((BitmapDrawable)imgInput.Drawable).Bitmap;
                double size = Convert.ToDouble(textseekSize.Text.ToString());
                size = size / 100;
                Bitmap imgTemp = ImageOp.resizeImage(img, size, size);
                //Bitmap result = ImageOp.greyImg(imgTemp);

                imgResult.SetImageBitmap(imgTemp);
            };



            buttonSelectImageGallery.Click += delegate
            {
                var imageIntent = new Intent();
                imageIntent.SetType("image/*");
                imageIntent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(imageIntent, "Select photo"), 0);
            };


            //Slider
            seekThresh = FindViewById<SeekBar>(Resource.Id.TestAreaSeekBar1);
            seekBlur = FindViewById<SeekBar>(Resource.Id.TestAreaSeekBar2);
            seekSize = FindViewById<SeekBar>(Resource.Id.TestAreaSeekBar3);

            //Slider Listener
            seekThresh.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                   
                    textseekThresh.Text = string.Format("" + e.Progress);
                }
            };

            seekBlur.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                    textseekBlur.Text = string.Format("" + e.Progress);
                }
            };

            seekSize.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                    textseekSize.Text = string.Format("" + e.Progress);
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
                imgInput.SetImageURI(data.Data);
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