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
using Magic.Shared.magicocr;
using System.Threading.Tasks;

namespace Magic.Shared.imgop
{
    static class ImageOp
    {

        static ImageOp()
        {

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static byte[] getBytesFromBitmap(Bitmap img)
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Bitmap greyImg(Bitmap img)
        {
            //Matrix für das Bild
            Mat imgMat = new Mat();

            //Bild zu Matrix umwandeln
            Utils.BitmapToMat(img, imgMat);

            //-----------------Bild bearbeiten---------------------

            //Variablen
            Size s = new Size(10.0, 10.0);
            OpenCV.Core.Point p = new OpenCV.Core.Point(0, 0);

            //TODO Matrix größe beachten?
            Bitmap bmp = null;
            Mat tmpgrey = new Mat(10, 10, CvType.Cv8uc1, new Scalar(4));
            Mat tmpblur = new Mat(10, 10, CvType.Cv8uc1, new Scalar(4));
            Mat tmpthresh = new Mat(10, 10, CvType.Cv8uc1, new Scalar(4));
            Mat imgresult = new Mat(10, 10, CvType.Cv8uc1, new Scalar(4));
            try
            {
                //Grau
                Imgproc.CvtColor(imgMat, tmpgrey, Imgproc.ColorBgr2gray, 4);

                //Blur
                Imgproc.Blur(tmpgrey, tmpblur, s, p);

                //Thresh
                Imgproc.Threshold(tmpblur, tmpthresh, 90, 255, Imgproc.ThreshBinary);

                            //Kontrast
                //tmpthresh.ConvertTo(imgresult, -1, 9.0, 10);

                bmp = Bitmap.CreateBitmap(tmpthresh.Cols(), tmpthresh.Rows(), Bitmap.Config.Argb8888);
                Utils.MatToBitmap(tmpthresh, bmp);
            }
            catch (CvException e) { System.Console.WriteLine(e.Message); }


            return bmp;

        }


        /// <summary>
        /// Resizes a image by the given parameters
        /// </summary>
        /// <param name="img">Image to resize</param>
        /// <param name="width">Old Size * width</param>
        /// <param name="heigth">Old Size * height</param>
        /// <returns>resized image</returns>
        public static Bitmap resizeImage(Bitmap img, int width, int heigth)
        {
            //Matrix für das Bild
            Mat src = new Mat();
            Mat dst = new Mat();
            //Bild zu Matrix umwandeln
            Utils.BitmapToMat(img, src);
            //Bild zu Matrix umwandeln
            // downsample and use it for processing
            Imgproc.Resize(src, dst, new Size(src.Size().Width * width, src.Size().Height * heigth));
            Bitmap img1;
            img1 = Bitmap.CreateBitmap(dst.Cols(), dst.Rows(), Bitmap.Config.Argb8888);
            Utils.MatToBitmap(dst, img1);

            return img1;
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        public static Bitmap detectTextRect(Bitmap img)
        {
            //Matrix für die Bilder
            Mat large = new Mat();
            Mat small = new Mat();
            Mat rgb = new Mat();

            //Bild zu Matrix umwandeln
            Utils.BitmapToMat(img, large);

            // downsample and use it for processing
            Imgproc.PyrDown(large, rgb);

            //Grey
            Imgproc.CvtColor(rgb, small, Imgproc.ColorBgr2gray);

            //Gradiant
            Mat grad = new Mat();
            Size morphsize = new Size(3.0, 3.0);
            Mat morphKernel = Imgproc.GetStructuringElement(Imgproc.MorphEllipse, morphsize);

            Imgproc.MorphologyEx(small, grad, Imgproc.MorphGradient, morphKernel);

            //Binarize
            Mat bw = new Mat();

            Imgproc.Threshold(grad, bw, 0.0, 255.0, Imgproc.ThreshBinary | Imgproc.ThreshOtsu);

            // connect horizontally oriented regions
            Mat connected = new Mat();
            Size connectsize = new Size(9.0, 1.0);
            morphKernel = Imgproc.GetStructuringElement(Imgproc.MorphRect, connectsize);
            Imgproc.MorphologyEx(bw, connected, Imgproc.MorphClose, morphKernel);

            // find contours
            Mat mask = Mat.Zeros(bw.Size(), CvType.Cv8uc1);

            JavaList<MatOfPoint> contours = new JavaList<MatOfPoint>();
            Mat hierarchy = new Mat();

            OpenCV.Core.Point contourPoint = new OpenCV.Core.Point(0, 0);

            Imgproc.FindContours(connected, contours, hierarchy, Imgproc.RetrCcomp, Imgproc.ChainApproxSimple, contourPoint);

            Scalar zero = new Scalar(0, 0, 0);
            Scalar contourscal = new Scalar(255, 255, 255);

            Scalar rectScalar = new Scalar(0, 255, 0);



            //Variablen
            OpenCV.Core.Rect rect;
            Mat maskROI;
            double r;
            double[] contourInfo;

            for (int i = 0; i >= 0;)
            {
                rect = Imgproc.BoundingRect(contours[i]);

                maskROI = new Mat(mask, rect);
                maskROI.SetTo(zero);

                //fill the contour
                Imgproc.DrawContours(mask, contours, i, contourscal, Core.Filled);

                // ratio of non-zero pixels in the filled region
                r = (double)Core.CountNonZero(maskROI) / (rect.Width * rect.Height);

                /* assume at least 45% of the area is filled if it contains text */
                /* constraints on region size */
                /* these two conditions alone are not very robust. better to use something 
                like the number of significant peaks in a horizontal projection as a third condition */
                if (r > .45 && (rect.Height > 8 && rect.Width > 8))
                {
                    Imgproc.Rectangle(rgb, rect.Br(), rect.Tl(), rectScalar, 2);
                }

                //Nächste Element bestimmen
                contourInfo = hierarchy.Get(0, i);
                i = (int)contourInfo[0];

            }


            Bitmap resultrgb;
            resultrgb = Bitmap.CreateBitmap(rgb.Cols(), rgb.Rows(), Bitmap.Config.Argb8888);
            Utils.MatToBitmap(rgb, resultrgb);



            return resultrgb;
        }




        public static async Task<string> detectAndExtractText(Bitmap img)
        {
            //Matrix für die Bilder
            Mat large = new Mat();
            Mat small = new Mat();
            Mat rgb = new Mat();

            //Bild zu Matrix umwandeln
            Utils.BitmapToMat(img, rgb);

            // downsample and use it for processing
            //Imgproc.PyrDown(large, rgb);

            //Grey
            Imgproc.CvtColor(rgb, small, Imgproc.ColorBgr2gray);

            //Gradiant
            Mat grad = new Mat();
            Size morphsize = new Size(3.0, 3.0);
            Mat morphKernel = Imgproc.GetStructuringElement(Imgproc.MorphEllipse, morphsize);

            Imgproc.MorphologyEx(small, grad, Imgproc.MorphGradient, morphKernel);

            //Binarize
            Mat bw = new Mat();

            Imgproc.Threshold(grad, bw, 0.0, 255.0, Imgproc.ThreshBinary | Imgproc.ThreshOtsu);

            // connect horizontally oriented regions
            Mat connected = new Mat();
            Size connectsize = new Size(9.0, 1.0);
            morphKernel = Imgproc.GetStructuringElement(Imgproc.MorphRect, connectsize);
            Imgproc.MorphologyEx(bw, connected, Imgproc.MorphClose, morphKernel);

            // find contours
            Mat mask = Mat.Zeros(bw.Size(), CvType.Cv8uc1);

            JavaList<MatOfPoint> contours = new JavaList<MatOfPoint>();
            Mat hierarchy = new Mat();

            OpenCV.Core.Point contourPoint = new OpenCV.Core.Point(0, 0);

            Imgproc.FindContours(connected, contours, hierarchy, Imgproc.RetrCcomp, Imgproc.ChainApproxSimple, contourPoint);

            Scalar zero = new Scalar(0, 0, 0);
            Scalar contourscal = new Scalar(255, 255, 255);

            Scalar rectScalar = new Scalar(0, 255, 0);


            OpenCV.Core.Rect rect;
            Mat maskROI;
            double r;
            double[] contourInfo;

            string resulttext = "";
            string part;

            Bitmap bmpOcr;
            Mat croppedPart;


            for (int i = 0; i >= 0;)
            {
                rect = Imgproc.BoundingRect(contours[i]);

                maskROI = new Mat(mask, rect);
                maskROI.SetTo(zero);

                //fill the contour
                Imgproc.DrawContours(mask, contours, i, contourscal, Core.Filled);

                // ratio of non-zero pixels in the filled region
                r = (double)Core.CountNonZero(maskROI) / (rect.Width * rect.Height);

                /* assume at least 45% of the area is filled if it contains text */
                /* constraints on region size */
                /* these two conditions alone are not very robust. better to use something 
                like the number of significant peaks in a horizontal projection as a third condition */
                if (r > .45 && (rect.Height > 8 && rect.Width > 8))
                {
                    //Imgproc.Rectangle(rgb, rect.Br(), rect.Tl(), rectScalar, 2);
                    try
                    {
                        
                        croppedPart = rgb.Submat(rect);

                        bmpOcr = Bitmap.CreateBitmap(croppedPart.Width(), croppedPart.Height(), Bitmap.Config.Argb8888);
                        Utils.MatToBitmap(croppedPart, bmpOcr);

                        part = await OCR.getText(bmpOcr);
                        resulttext = resulttext + part;

                    }
                    catch (Exception e)
                    {
                        Android.Util.Log.Debug("Fehler", "cropped part data error " + e.Message);
                    }
                }


                //Nächste Element bestimmen
                contourInfo = hierarchy.Get(0, i);
                i = (int)contourInfo[0];



            }


            return resulttext;
        }



































        //----------------------------Old Stuff-------------------------


        //private async void detectText(int img)
        //{
        //    //Bitmap laden
        //    Bitmap result = await BitmapFactory.DecodeResourceAsync(Resources, img);

        //    Bitmap resultgrey = await greyImg(img);

        //    //Matrix für die Bilder
        //    Mat mGrey = new Mat();
        //    Mat mRGB = new Mat();

        //    //Bild zu Matrix umwandeln
        //    Utils.BitmapToMat(resultgrey, mGrey);
        //    Utils.BitmapToMat(result, mRGB);


        //    MatOfKeyPoint keypoint = new MatOfKeyPoint();
        //    //IList oder List?
        //    IList<KeyPoint> listpoint;
        //    KeyPoint kpoint;
        //    //Cvtype richtig?
        //    Mat mask = Mat.Zeros(mGrey.Size(), CvType.Cv8uc1);
        //    int rectanx1;
        //    int rectany1;
        //    int rectanx2;
        //    int rectany2;
        //    int imgsize = mGrey.Height() * mGrey.Width();
        //    Scalar zeos = new Scalar(0, 0, 0);

        //    List<MatOfPoint> contour2 = new List<MatOfPoint>();
        //    //CvType?
        //    Mat kernel = new Mat(1, 50, CvType.Cv8uc1, Scalar.All(255));
        //    Mat morbyte = new Mat();
        //    Mat hierarchy = new Mat();

        //    OpenCV.Core.Rect rectan3;
        //    //
        //    FeatureDetector detector = FeatureDetector
        //            .Create(FeatureDetector.Mser);
        //    detector.Detect(mGrey, keypoint);
        //    listpoint = keypoint.ToList();

        //    //Counter?
        //    for (int ind = 0; ind < listpoint.Count; ind++)
        //    {
        //        kpoint = listpoint[ind];
        //        rectanx1 = (int)(kpoint.Pt.X - 0.5 * kpoint.Size);
        //        rectany1 = (int)(kpoint.Pt.Y - 0.5 * kpoint.Size);
        //        rectanx2 = (int)(kpoint.Size);
        //        rectany2 = (int)(kpoint.Size);
        //        if (rectanx1 <= 0)
        //            rectanx1 = 1;
        //        if (rectany1 <= 0)
        //            rectany1 = 1;
        //        if ((rectanx1 + rectanx2) > mGrey.Width())
        //            rectanx2 = mGrey.Width() - rectanx1;
        //        if ((rectany1 + rectany2) > mGrey.Height())
        //            rectany2 = mGrey.Height() - rectany1;
        //        OpenCV.Core.Rect rectant = new OpenCV.Core.Rect(rectanx1, rectany1, rectanx2, rectany2);
        //        try
        //        {
        //            Mat roi = new Mat(mask, rectant);
        //            roi.SetTo(CONTOUR_COLOR);
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Debug("mylog", "mat roi error " + ex.Message);
        //        }
        //    }

        //    Imgproc.MorphologyEx(mask, morbyte, Imgproc.MorphDilate, kernel);
        //    Imgproc.FindContours(morbyte, contour2, hierarchy,
        //            Imgproc.RetrExternal, Imgproc.ChainApproxNone);
        //    for (int ind = 0; ind < contour2.Count; ind++)
        //    {
        //        rectan3 = Imgproc.BoundingRect(contour2[ind]);
        //        rectan3 = Imgproc.BoundingRect(contour2[ind]);
        //        if (rectan3.Area() > 0.5 * imgsize || rectan3.Area() < 100
        //                || rectan3.Width / rectan3.Height < 2)
        //        {
        //            Mat roi = new Mat(morbyte, rectan3);
        //            roi.SetTo(zeos);

        //        }
        //        else
        //            Imgproc.Rectangle(mRGB, rectan3.Br(), rectan3.Tl(), CONTOUR_COLOR);
        //    }

        //    Bitmap resultrgb;
        //    resultrgb = Bitmap.CreateBitmap(mRGB.Cols(), mRGB.Rows(), Bitmap.Config.Argb8888);
        //    Utils.MatToBitmap(mRGB, resultrgb);

        //    showImage(getBytesFromBitmap(resultrgb));
        //}

    }
}
