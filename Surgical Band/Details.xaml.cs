﻿using Microsoft.Band;
using Microsoft.Band.Sensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
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
    public sealed partial class Details : Page
    {
        private IBandInfo bandinfo;
        private IBandClient client;
        SpeechSynthesizer synth = new SpeechSynthesizer();
        private bool WasLocked;
        DateTime LastBandWarning = new DateTime();
        public Details()
        {
            this.InitializeComponent();
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var bands = await BandClientManager.Instance.GetBandsAsync();
            if (bands.Length == 0)
            {

                Speak("Warning. Patient not found.");
                return;
            }
            bandinfo = bands[0];
            client = await BandClientManager.Instance.ConnectAsync(bandinfo);
            //client.SensorManager.HeartRate.ReadingChanged += BandInitialized;
            client.SensorManager.HeartRate.ReadingChanged += HeartRate_ReadingChanged;
            client.SensorManager.SkinTemperature.ReadingChanged += SkinTemperature_ReadingChanged;
            if (client.SensorManager.HeartRate.GetCurrentUserConsent() != UserConsent.Granted)
                await client.SensorManager.HeartRate.RequestUserConsentAsync();
            await client.SensorManager.HeartRate.StartReadingsAsync();
            await client.SensorManager.SkinTemperature.StartReadingsAsync();
        }

        private async void SkinTemperature_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandSkinTemperatureReading> e)
        {
            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { SkinTemperature_ReadingChanged(sender, e); });
                return;
            }

            PatientSkinTemp.Text = e.SensorReading.Temperature.ToString()+" °C";
        }

        private void BandInitialized(object sender, BandSensorReadingEventArgs<IBandHeartRateReading> e)
        {
            Speak("Band Initialized. Current heart rate is {0}.", e.SensorReading.HeartRate);
            client.SensorManager.HeartRate.ReadingChanged -= BandInitialized;
        }
        private void BandInitialized(object sender, BandSensorReadingEventArgs<IBandSkinTemperatureReading> e)
        {
            Speak("Band Initialized. Current body temperature is {0}.", e.SensorReading.Temperature);
            client.SensorManager.SkinTemperature.ReadingChanged -= BandInitialized;
        }

        private async void HeartRate_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandHeartRateReading> e)
        {
            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { HeartRate_ReadingChanged(sender, e); });
                return;
            }
            if (e.SensorReading.Quality == HeartRateQuality.Acquiring)
            {
                PatientHR.Text = "Aquiring...";
                if (WasLocked)
                {
                    if (DateTime.Now.Subtract(LastBandWarning).TotalSeconds > 10)
                    {
                        var WorkflowFrame = ((Window.Current.Content as Frame).Content as MainPage).WorkflowFrame;
                        if (WorkflowFrame.Content is EquipTrack || WorkflowFrame.Content is RecordConsent)
                            return; // Surgery mode!
                        Speak("Warning: Patient has removed their Band.");
                        LastBandWarning = DateTime.Now;
                    }
                    PatientHR.Text = "Lost";
                }
            }
            else
            {
                PatientHR.Text = e.SensorReading.HeartRate.ToString() + " BPM";
                WasLocked |= true;
            }
        }

        public async void Speak(string text, params object[] args)
        {
            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { Speak(text, args); });
                return;
            }

            // The media object for controlling and playing audio.
            MediaElement mediaElement = new MediaElement();

            // Generate the audio stream from plain text.
            SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(String.Format(text, args));

            // Send the stream to the media object.
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }

    }
}
