using Microsoft.WindowsAzure.Mobile.Service;
using MyHomeSecureWeb.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyHomeSecureWeb.Notifications
{
    public class StateNotification : IStateNotification
    {
        private ApiServices _services;

        private const string POST_URL = "https://fcm.googleapis.com/fcm/send";

        public StateNotification(ApiServices services)
        {
            _services = services;
        }

        public async Task Send(string homeHubId, string state, bool active, string node, string rule)
        {
            var statusMessage = new StatusMessage {
                Message = "StateNotification",
                HomeHubId = homeHubId,
                State = state,
                Active = active,
                Node = node,
                Rule = rule
            };

            HttpClient httpClient = new HttpClient();
            var serverKey = AppSettings.GetFirebaseServerKey();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", string.Format("={0}", serverKey));

            var messageJSON = JsonConvert.SerializeObject(statusMessage);

            var requestData = new
            {
                data = new
                {
                    message = messageJSON
                },
                to = string.Format("/topics/{0}", homeHubId)
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(POST_URL, content);

            if (!response.IsSuccessStatusCode)
            {
                string message = "unknown";
                if (response.Content != null)
                {
                    message = await response.Content.ReadAsStringAsync();
                }

                _services.Log.Error(string.Format("{0} - {1}", response.StatusCode, message), null, "StateNotification.Send Error");
            }

            //Dictionary<string, string> data = new Dictionary<string, string>()
            //{
            //    { "message", JsonConvert.SerializeObject(statusMessage) }
            //};
            //GooglePushMessage message = new GooglePushMessage(data, TimeSpan.FromHours(1));

            //try
            //{
            //    var result = await _services.Push.SendAsync(message, homeHubId);
            //    _services.Log.Info(result.State.ToString());
            //}
            //catch (Exception ex)
            //{
            //    _services.Log.Error(ex.Message, null, "Push.SendAsync Error");
            //}
        }
    }
}
