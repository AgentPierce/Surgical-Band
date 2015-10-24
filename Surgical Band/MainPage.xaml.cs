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
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///vcd.xml"));
            await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(storageFile);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private async void Con_Result(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Text == "hello")
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => talkBack());
                //SetListening(true);
            }
        }

        private async void listenIn() {
            SpeechRecognizer speechRecognizer = new SpeechRecognizer();
            speechRecognizer.Constraints.Add(new SpeechRecognitionListConstraint(new List<String>() { "hello" }));

            SpeechRecognitionCompilationResult comResult = await speechRecognizer.CompileConstraintsAsync();

            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += Con_Result;

            await speechRecognizer.ContinuousRecognitionSession.StartAsync();
        }

        private async void talkBack() {
            SpeechSynthesizer synt = new SpeechSynthesizer();
            SpeechSynthesisStream syntStream = await synt.SynthesizeTextToStreamAsync("I'm Cortana");
            mediaElement.SetSource(syntStream, syntStream.ContentType);
        }

        private async void lineRecog() {
            SpeechRecognizer speechRecognizer = new SpeechRecognizer();

            // Compile the default dictionary
            SpeechRecognitionCompilationResult compilationResult =
                                                    await speechRecognizer.CompileConstraintsAsync();

            // Start recognizing
            // Note: you can also use RecognizeWithUIAsync()
            SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();
            string result = speechRecognitionResult.Text;
            this.textBlock.Text = result;
        }
    }
}
