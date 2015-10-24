using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class CheckList : Page
    {
        private Boolean marked = true;
        private Boolean allegy = false;
        private Boolean aspiration = false;
        private Boolean airwayEquip = false;
        private Boolean bloodLoss = false;
        private Boolean ivAccess = false;
        private Boolean proceed = true;
        private string result;
        private int queCounter = 0;
        private string[] questionList = new string[]{ "Has the site been marked?",
                                                        "Does the patient have a known allergy?",
                                                        "Has due consideration been given?",
                                                        "Does the patient have a difficult airway or aspiration risk?",
                                                        "Is the difficult airway equipment available?"};

        public CheckList()
        {
            this.InitializeComponent();
            Loaded += CheckList_Loaded;
        }

        private void CheckList_Loaded(object sender, RoutedEventArgs e)
        {
            question1.Text = questionList[0];
            question2.Text = questionList[1];
            question3.Text = questionList[2];
            question4.Text = questionList[3];
            question5.Text = questionList[4];
            talkBack(questionList[0]);
            listenIn();
        }

        private async void listenIn()
        {
            SpeechRecognizer speechRecognizer = new SpeechRecognizer();
            speechRecognizer.Constraints.Add(new SpeechRecognitionListConstraint(new List<String>() { "yes" }));

            SpeechRecognitionCompilationResult comResult = await speechRecognizer.CompileConstraintsAsync();

            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += Con_Result;

            await speechRecognizer.ContinuousRecognitionSession.StartAsync();
        }

        private async void Con_Result(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Text.Contains("yes"))
            {
                if (marked)
                {
                    queCounter++;
                    marked = false;
                }
                if (queCounter == 1)
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack(questionList[queCounter]));
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => tick1.Visibility = Visibility.Visible);
                    queCounter++;
                }
                else if (queCounter == 2)
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack(questionList[queCounter]));
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => tick2.Visibility = Visibility.Visible);
                    queCounter++;
                }
                else if (queCounter == 3)
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack(questionList[queCounter]));
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => tick3.Visibility = Visibility.Visible);
                    queCounter++;
                }
                else if (queCounter == 4)
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack(questionList[queCounter]));
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => tick4.Visibility = Visibility.Visible);
                    queCounter++;
                }
                else if (queCounter == questionList.Count()) {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => tick5.Visibility = Visibility.Visible);
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack("Sign in checklist complete, you can track equipment now."));
                    await sender.StopAsync();
                }
            }
        }

        private async void talkBack(String content)
        {
            SpeechSynthesizer synt = new SpeechSynthesizer();
            SpeechSynthesisStream syntStream = await synt.SynthesizeTextToStreamAsync(content);
            mediaElement.SetSource(syntStream, syntStream.ContentType);
        }

        private async void lineRecog()
        {
                SpeechRecognizer speechRecognizer = new SpeechRecognizer();

                // Compile the default dictionary
                SpeechRecognitionCompilationResult compilationResult =
                                                        await speechRecognizer.CompileConstraintsAsync();

                // Start recognizing
                // Note: you can also use RecognizeWithUIAsync()
                SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeAsync();
                result = speechRecognitionResult.Text;
        }
    }
}
