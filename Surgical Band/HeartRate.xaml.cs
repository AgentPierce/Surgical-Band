using Microsoft.Band;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechSynthesis;
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
    public sealed partial class HeartRate : Page
    {
        public HeartRate()
        {
            this.InitializeComponent();
            Loaded += HeartRate_Loaded;
        }

        private async void HeartRate_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the list of Microsoft Bands paired to the phone.
                IBandInfo[] pairedBands = await BandClientManager.Instance.GetBandsAsync();
                if (pairedBands.Length < 1)
                {
                    //this.viewModel.StatusMessage = "This sample app requires a Microsoft Band paired to your device. Also make sure that you have the latest firmware installed on your Band, as provided by the latest Microsoft Health app.";
                    return;
                }

                // Connect to Microsoft Band.
                using (IBandClient bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                {
                    bool heartRateConsentGranted;
                    var hr = "";
                    var bodyTemp = "";
                    // Check whether the user has granted access to the HeartRate sensor.
                    if (bandClient.SensorManager.HeartRate.GetCurrentUserConsent() == UserConsent.Granted)
                    {
                        heartRateConsentGranted = true;
                    }
                    else
                    {
                        heartRateConsentGranted = await bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
                    }

                    if (!heartRateConsentGranted)
                    {
                        //this.viewModel.StatusMessage = "Access to the heart rate sensor is denied.";
                    }
                    else
                    {
                        int samplesReceived = 0; // the number of HeartRate samples received
                        
                        // Subscribe to HeartRate data.
                        bandClient.SensorManager.HeartRate.ReadingChanged += (s, args) => { samplesReceived++; hr = args.SensorReading.HeartRate.ToString();};
                        await bandClient.SensorManager.HeartRate.StartReadingsAsync();

                        // Receive HeartRate data for a while, then stop the subscription.
                        await Task.Delay(TimeSpan.FromSeconds(3));
                        await bandClient.SensorManager.HeartRate.StopReadingsAsync();
                       
                        //this.viewModel.StatusMessage = string.Format("Done. {0} HeartRate samples were received.", samplesReceived);
                    }

                    bandClient.SensorManager.SkinTemperature.ReadingChanged += (s, args) =>
                    {
                        bodyTemp = args.SensorReading.Temperature.ToString();
                    };
                    await bandClient.SensorManager.SkinTemperature.StartReadingsAsync();

                    SpeechSynthesizer synt = new SpeechSynthesizer();
                    SpeechSynthesisStream syntStream = await synt.SynthesizeTextToStreamAsync("Heart Rate is: " + hr+ " per minute and Skin Temperature is: "+bodyTemp+" degrees");
                    mediaElement.SetSource(syntStream, syntStream.ContentType);
                    hReading.Text = hr + " /min";
                    tReading.Text = bodyTemp + " C";
                }
            }
            catch (Exception ex)
            {
                //this.viewModel.StatusMessage = ex.ToString();
            }
        }
    }
}
