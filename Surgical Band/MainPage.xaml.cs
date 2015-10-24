using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Surgical_Band
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private int noteCounter = 0;

        public Frame WorkflowFrame { get { return workflowFrame; } }

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///vcd.xml"));
            await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(storageFile);
            DetailsFrame.Navigate(typeof(Details));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter.ToString() == "Note")
            {
                listenIn();
                noteCounter++;
            }
        }

        private async void Con_Result(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Text == "note finished")
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack("note recorded"));
                await sender.StopAsync();
                //SetListening(true);
            }
        }

        private async void listenIn() {
            SpeechRecognizer speechRecognizer = new SpeechRecognizer();
            speechRecognizer.Constraints.Add(new SpeechRecognitionListConstraint(new List<String>() { "note finished" }));

            SpeechRecognitionCompilationResult comResult = await speechRecognizer.CompileConstraintsAsync();

            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += Con_Result;

            await speechRecognizer.ContinuousRecognitionSession.StartAsync();
        }

        private async void talkBack(String content) {
            SpeechSynthesizer synt = new SpeechSynthesizer();
            SpeechSynthesisStream syntStream = await synt.SynthesizeTextToStreamAsync(content);
            mediaElement.SetSource(syntStream, syntStream.ContentType);
        }

    }
}
