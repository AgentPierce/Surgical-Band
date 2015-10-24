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
                    string steps = "";

                    // Subscribe to HeartRate data.
                    bandClient.SensorManager.Pedometer.ReadingChanged += (s, args) => { steps = args.SensorReading.TotalSteps.ToString(); };
                        await bandClient.SensorManager.Pedometer.StartReadingsAsync();
                    hReading.Text = pairedBands.ToString();

                    // Receive HeartRate data for a while, then stop the subscription.
                    await bandClient.SensorManager.Pedometer.StopReadingsAsync();

                    SpeechSynthesizer synt = new SpeechSynthesizer();
                    SpeechSynthesisStream syntStream = await synt.SynthesizeTextToStreamAsync("Patient did "+steps+" steps today.");
                    mediaElement.SetSource(syntStream, syntStream.ContentType);
                    hReading.Text = steps + "\nSteps";
                }
            }
            catch (Exception ex)
            {
                //this.viewModel.StatusMessage = ex.ToString();
                //hReading.Text = ex.ToString();
            }
        }
    }
}
