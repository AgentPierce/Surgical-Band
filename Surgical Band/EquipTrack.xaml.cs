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
    public sealed partial class EquipTrack : Page
    {
        private int spongeIn = 0;
        private int needleIn = 0;
        private int instruIn = 0;
        private int instruOut = 0;
        private int needleOut = 0;
        private int spongeOut = 0;
        public EquipTrack()
        {
            this.InitializeComponent();
            Loaded += EquipTrack_Loaded;
        }

        private void EquipTrack_Loaded(object sender, RoutedEventArgs e)
        {
            listenIn();
        }

        private async void listenIn()
        {
            SpeechRecognizer speechRecognizer = new SpeechRecognizer();
            speechRecognizer.Constraints.Add(new SpeechRecognitionListConstraint(new List<String>() { "sponge in", "sponge out", "instrument in", "needle in","needle out", "instrument out", "going to close" }));

            SpeechRecognitionCompilationResult comResult = await speechRecognizer.CompileConstraintsAsync();

            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += Con_Result;

            await speechRecognizer.ContinuousRecognitionSession.StartAsync();
        }

        private async void Con_Result(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            switch (args.Result.Text) {
                case "sponge in":
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack("Noted"));
                    spongeIn++;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => SetNumber(spongInC, spongeIn));
                    break;
                case "sponge out":
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack("Noted"));
                    spongeOut++;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => SetNumber( spongOutC,spongeOut));
                    break;
                case "instrument in":
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack("Noted"));
                    instruIn++;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => SetNumber( instInC, instruIn));
                    break;
                case "needle in":
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack("Noted"));
                    needleIn++;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => SetNumber( needleInC,needleIn));
                    break;
                case "instrument out":
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack("Noted"));
                    instruOut++;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => SetNumber( instOutC,instruOut));
                    break;
                case "needle out":
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack("Noted"));
                    needleOut++;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => SetNumber( needleOutC,needleOut));
                    break;
                case "going to close":
                    if (spongeIn != spongeOut)
                    {
                        await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack("I'm concerned. There is "+ (int) Math.Abs(spongeIn-spongeOut)+" sponge not accounted for. "+spongeIn+" In, "+ spongeOut+" out"));
                        
                    } else if (needleIn != needleOut)
                    {
                        await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack("I'm concerned. There is " + (int) Math.Abs(needleIn - needleOut) + " needle not accounted for. " + needleIn + " in, " + needleOut + " out."));
                        
                    }
                    else if (instruIn != instruOut)
                    {
                        await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack("I'm concerned. There is " + (int) Math.Abs(instruIn - instruOut) + " instrument not accounted for. " + instruIn + " in, " + instruOut + " out."));
                        
                    }
                    else {
                        await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => talkBack("I have countered " + spongeIn + " in sponge, " + spongeOut + " out sponge, " + instruIn + " in instrument, " + instruOut + " out instrument, " + needleIn + " in needle, " + needleOut + " out needle. Count seems OK"));
                        instruIn = instruOut = needleIn = needleOut = spongeIn = spongeOut = 0;
                        await sender.StopAsync();
                    }
                    break;
            }
        }

        private void SetNumber(TextBlock label, int value)
        {
            foreach (var l in new TextBlock[] { spongInC, spongOutC, needleInC, needleOutC, instInC, instOutC})
            {
                l.FontSize = 30;
            }
            label.FontSize = 40;
            label.Text = value.ToString();
        }

        private async void talkBack(String content)
        {
            SpeechSynthesizer synt = new SpeechSynthesizer();
            SpeechSynthesisStream syntStream = await synt.SynthesizeTextToStreamAsync(content);
           
            mediaElement.SetSource(syntStream, syntStream.ContentType);
        }
    }
}
