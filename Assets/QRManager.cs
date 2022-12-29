using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public GameObject AROrigin;
    public ARSession arSession;
    private RenderTexture rt; //Texture where the QR camera is portrayed on

    
    private TMP_Text textViewer;

    public GameObject QR_GUI;
    public GameObject MiniMap_GUI;
    private float _period = 0f;
    private float _time;

    private ARCameraManager _cameraManager;
    private Texture2D rawCameraImage;

    private Texture2D m_CameraTexture;
    
    private XRCpuImage.Transformation m_Transformation = XRCpuImage.Transformation.MirrorX;

    private IBarcodeReader barCodeReader = new BarcodeReader(){
        AutoRotate = false,
        Options = new ZXing.Common.DecodingOptions{
            TryHarder = false
        }
    }; //The actual decoder

    private GameObject[] locations;
    private GameObject player;
    void Start()
    {
        _cameraManager = GameObject.Find("AR Camera").GetComponent<ARCameraManager>();
        textViewer = MiniMap_GUI.GetComponentInChildren<TMP_Text>();
        player = GameObject.Find("GPSViewer");
        locations = GameObject.FindGameObjectsWithTag("destination");
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
        bool found=false;
        if (_time >= _period){
            UpdateCameraImage();
            found = CheckQRAndroid();
            _time = 0.0f;
            if(found){
                this.enabled = false;
                }
        }

        _time += Time.deltaTime;
    }
    
    bool CheckQRAndroid(){
        try
        {
            rt = new RenderTexture(rawCameraImage.width / 2, rawCameraImage.height / 2, 0);
            RenderTexture.active = rt;
            // Copy your texture ref to the render texture
            Graphics.Blit(rawCameraImage, rt);

            Color32[] framebuffer = rawCameraImage.GetPixels32();
            if (framebuffer.Length == 0)
            {
                return false;
            }

            var data = barCodeReader.Decode(framebuffer, rawCameraImage.width, rawCameraImage.height);

            if (data != null)
            {
                // QRCode detected. Get origin and destination
                string[] info = data.Text.Split('/');
                GameObject newOrigin = locations.Where(obj => obj.name == info[0]).SingleOrDefault();
                GameObject newDest = locations.Where(obj => obj.name == info[1]).SingleOrDefault();   
                
                NavController controller = player.GetComponent<NavController>();

                //Move player to the desired location and set destination
                arSession.Reset();
                AROrigin.transform.position = newOrigin.transform.position;
                AROrigin.transform.rotation = newOrigin.transform.rotation;
                controller.Objective = newDest;
                controller.updateDest();
                textViewer.text = "Hello there";

                QR_GUI.SetActive(false);
                MiniMap_GUI.SetActive(true);
                return true;
            }
            else{
                textViewer.text = "Scan QR";
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading QR");
            Debug.LogError(e.Message);
        }
        return false;
    }
    

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

                // Apply the updated texture data to our texture
                m_CameraTexture.Apply();
                rawCameraImage = m_CameraTexture;
            }
    }