using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Surgical_Band
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RecordConsent : Page
    {
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);

        public RecordConsent()
        {
            this.InitializeComponent();
            Loaded += RecordConsent_Loaded;
        }

        private void RecordConsent_Loaded(object sender, RoutedEventArgs e)
        {
            listenIn();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            /*if (e.Parameter.ToString() == "Record")
            {
                mciSendString("open new Type waveaudio Alias recsound", "",0,0);
                mciSendString("record recsound","",0,0);
            }
            else if (e.Parameter.ToString() == "Stop") {
                mciSendString("save recsound c:\\test.wav","",0,0);
                mciSendString("close recsound", "", 0, 0);
            }
            this.textBlock.Text = e.Parameter.ToString();*/
        }

        private async void listenIn()
        {
            SpeechRecognizer speechRecognizer = new SpeechRecognizer();
            speechRecognizer.Constraints.Add(new SpeechRecognitionListConstraint(new List<String>() { "correct" }));

            SpeechRecognitionCompilationResult comResult = await speechRecognizer.CompileConstraintsAsync();

            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += Con_Result;

            await speechRecognizer.ContinuousRecognitionSession.StartAsync();
        }

        private async void Con_Result(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Text == "correct")
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => talkBack());
                //SetListening(true);
            }
        }

        private async void talkBack()
        {
            SpeechSynthesizer synt = new SpeechSynthesizer();
            SpeechSynthesisStream syntStream = await synt.SynthesizeTextToStreamAsync("Thank you. You can proceed now.");
            mediaElement.SetSource(syntStream, syntStream.ContentType);
        }
    }
}
