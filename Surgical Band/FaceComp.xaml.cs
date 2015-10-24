using Microsoft.ProjectOxford.Face;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Surgical_Band
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FaceComp : Page
    {
        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("45b56e2595034c439a1cd41d34e76cb2");
        private MediaCapture takePhotoManager;

        public object Detection { get; private set; }

        public FaceComp()
        {
            this.InitializeComponent();
            Loaded += FaceComp_Loaded;
        }

        private async void FaceComp_Loaded(object sender, RoutedEventArgs e)
        {
            var cameraID = await GetCameraID(Windows.Devices.Enumeration.Panel.Back);
            takePhotoManager = new MediaCapture();

            await takePhotoManager.InitializeAsync(new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Video,
                //PhotoCaptureSource = PhotoCaptureSource.Photo,
                AudioDeviceId = string.Empty,
                VideoDeviceId = cameraID.Id
            });

            await takePhotoManager.ClearEffectsAsync(MediaStreamType.Photo);
            PhotoPreview.Source = takePhotoManager;
            await takePhotoManager.StartPreviewAsync();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();

            // a file to save a photo
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    "Photo.jpg", CreationCollisionOption.ReplaceExisting);

            await takePhotoManager.CapturePhotoToStorageFileAsync(imgFormat, file);
            Stream imageFileStream = File.OpenRead(ApplicationData.Current.LocalFolder.Path+"\\Photo.jpg");
            var currentFace = await faceServiceClient.DetectAsync(imageFileStream,false, false, false, false);
            if (currentFace.Count() == 1) {
                foreach (var detectedFace in currentFace)
                {
                    //talkBack(detectedFace.FaceId.ToString());
                    compareFaces(detectedFace.FaceId.ToString());
                }
            }   
        }

        private async void compareFaces(string patFaceID)
        {
            try
            {
                var gizmo = new Faces() { face1 = patFaceID, face2 = patFaceID };
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://api.projectoxford.ai/face/v0/verifications");

                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "45b56e2595034c439a1cd41d34e76cb2");

                // List data response.
                HttpResponseMessage response = await client.PostAsJsonAsync(new Uri("https://api.projectoxford.ai/face/v0/verifications"), gizmo); // Blocking call!
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body. Blocking!
                    var dataObjects = response.Content.ReadAsAsync<IEnumerable<string>>().Result;
                    foreach (var d in dataObjects)
                    {

                    }

                }
            }
            catch (Exception ex)
            {

            }

        }

        private static async Task<DeviceInformation> GetCameraID(Windows.Devices.Enumeration.Panel desired)
        {
            DeviceInformation deviceID = (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture))
                .FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desired);

            if (deviceID != null) return deviceID;
            else throw new Exception(string.Format("Camera of type {0} doesn't exist.", desired));
        }

        private async void talkBack(string content) {
            SpeechSynthesizer synt = new SpeechSynthesizer();
            SpeechSynthesisStream syntStream = await synt.SynthesizeTextToStreamAsync(content);
            mediaElement.SetSource(syntStream, syntStream.ContentType);
        }

    }

    public class Faces {
        public string face1 { get; set; }
        public string face2 { get; set; }
    }
}
