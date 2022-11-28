using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;

public class QRManager : MonoBehaviour
{
    public int freq = 2; //Frequency in Hz at which we run the QR processing
    public int side = 500; //Width of the square to scan
    public Camera cam;
    public RenderTexture rt;
    public TMP_Text textViewer;
    private float _period = 0f;
    private float _time;

    public ARCameraManager _cameraManager;
    public RawImage rawCameraImage;

    private Texture2D m_CameraTexture;

    private XRCpuImage.Transformation m_Transformation = XRCpuImage.Transformation.MirrorY;

    private IBarcodeReader barCodeReader = new BarcodeReader(){
        AutoRotate = false,
        Options = new ZXing.Common.DecodingOptions{
            TryHarder = false
        }
    }; //The actual decoder

    void Start()
    {
        //Set framerate to pass to QR decoder
        _time = 0f;
        if (freq ==0){
            this.enabled=false;
            throw new System.Exception("Frequency can't be zero");
        }
        _period = 1.0f/freq;

    }

    void Update()
    {
        if (_time >= _period){
            //UpdateCameraImage();
            CheckQR();
            _time = 0.0f;
        }

        _time += Time.deltaTime;
    }
    

    bool CheckQR(){
        try
        {
            //RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D tex = new Texture2D(rt.width,rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0,0,tex.width,tex.height),0,0,false);
            tex.Apply();
            //RenderTexture.active = currentActiveRT;

            Color32[] framebuffer = tex.GetPixels32();
            if (framebuffer.Length == 0)
            {
                return false;
            }

            var data = barCodeReader.Decode(framebuffer, tex.width, tex.height);
            print(data);
            if (data != null)
            {
                // QRCode detected.
                textViewer.text = data.Text;
                //Debug.Log("QR: " + data.Text);

                //OnQrCodeRead(new QrCodeReadEventArgs() { text = data.Text });
                return true;
            }
            else{
                textViewer.text = "";
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading QR");
            Debug.LogError(e.Message);
        }
        return false;
    }

/*
    void Scan()
         {
            System.Action<byte[], int, int> callback = (bytes, width, height) =>
            {
                if (bytes == null)
                {
                    // No image is available.
                    return;
                }

                // Decode the image using ZXing parser
                IBarcodeReader barcodeReader = new BarcodeReader();
                var result = barcodeReader.Decode(bytes, width, height, RGBLuminanceSource.BitmapFormat.Gray8);
                var resultText = result.Text;

                print(resultText);
            };

            CaptureScreenAsync(callback);
        }
        /// <summary>
    /// Capture the screen using CameraImage.AcquireCameraImageBytes.
    /// </summary>
    /// <param name="callback"></param>
    void CaptureScreenAsync(Action<byte[], int, int> callback)
    {
        Task.Run(() =>
        {
            byte[] imageByteArray = null;
            int width;
            int height;

            using (var imageBytes = ARCameraManager.TryAcquireLatestCpuImage())
            {
                if (!imageBytes.IsAvailable)
                {
                    callback(null, 0, 0);
                    return;
                }

                int bufferSize = imageBytes.YRowStride * imageBytes.Height;

                imageByteArray = new byte[bufferSize];

                Marshal.Copy(imageBytes.Y, imageByteArray, 0, bufferSize);

                width = imageBytes.Width;
                height = imageBytes.Height;
            }


            callback(imageByteArray, width, height);
        });
    }*/
    

   unsafe void UpdateCameraImage()
            {
                // Attempt to get the latest camera image. If this method succeeds,
                // it acquires a native resource that must be disposed (see below).
                if (!_cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
                {
                    m_CameraTexture = null;
                    return;
                }

                // Once we have a valid XRCpuImage, we can access the individual image "planes"
                // (the separate channels in the image). XRCpuImage.GetPlane provides
                // low-overhead access to this data. This could then be passed to a
                // computer vision algorithm. Here, we will convert the camera image
                // to an RGBA texture and draw it on the screen.

                // Choose an RGBA format.
                // See XRCpuImage.FormatSupported for a complete list of supported formats.
                var format = TextureFormat.RGBA32;

                if (m_CameraTexture == null || m_CameraTexture.width != image.width || m_CameraTexture.height != image.height)
                {
                    m_CameraTexture = new Texture2D(image.width, image.height, format, false);
                }

                // Convert the image to format, flipping the image across the Y axis.
                // We can also get a sub rectangle, but we'll get the full image here.
                var conversionParams = new XRCpuImage.ConversionParams(image, format, m_Transformation);

                // Texture2D allows us write directly to the raw texture data
                // This allows us to do the conversion in-place without making any copies.
                var rawTextureData = m_CameraTexture.GetRawTextureData<byte>();
                try
                {
                    image.Convert(conversionParams, new IntPtr(rawTextureData.GetUnsafePtr()), rawTextureData.Length);
                }
                finally
                {
                    // We must dispose of the XRCpuImage after we're finished
                    // with it to avoid leaking native resources.
                    image.Dispose();
                }
            }
    }