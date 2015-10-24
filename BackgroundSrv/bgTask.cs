using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Windows.UI.Xaml.Media.Imaging;

namespace BackgroundSrv
{
    public sealed class bgTask : XamlRenderingBackgroundTask
    {
        private BackgroundTaskDeferral serviceDeferral;
        VoiceCommandServiceConnection voiceServiceConnection;

        protected override async void OnRun(IBackgroundTaskInstance taskInstance)
        {
            this.serviceDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnTaskCanceled;

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            VoiceCommandUserMessage userMessage;
            VoiceCommandResponse response;
            try
            {
                voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);
                voiceServiceConnection.VoiceCommandCompleted += VoiceCommandCompleted;
                VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();

                switch (voiceCommand.CommandName)
                {

                    case "getPatientData":
                        userMessage = new VoiceCommandUserMessage();
                        userMessage.SpokenMessage = "Here is the Patient Data";

                        var responseMessage = new VoiceCommandUserMessage();
                        responseMessage.DisplayMessage = responseMessage.SpokenMessage = "Patient Name: John Spartan\nAge: 47\nBlood Type: O+\nPatient ID: 000S00117";

                        response = VoiceCommandResponse.CreateResponse(responseMessage);
                        await voiceServiceConnection.ReportSuccessAsync(response);

                        break;

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                if (this.serviceDeferral != null)
                {
                    //Complete the service deferral
                    this.serviceDeferral.Complete();
                }
            }
        }

        private void VoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }
    }
}
