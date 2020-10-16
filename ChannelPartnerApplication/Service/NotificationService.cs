using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ChannelPartnerApplication.Service
{
    public class NotificationService
    {
        #region Variable
        private readonly ClassBookManagementContext _context;
        #endregion

        #region Ctor
        public NotificationService(ClassBookManagementContext context)
        {
            _context = context;
        }
        #endregion

        #region Method
        public string SendNotificationFromFirebaseCloud()
        {
            var toKeys = new List<string>();
            toKeys.Add("e-vq6lNhQhaHE9Y9CtrNZo:APA91bFi0wQrvdbwDdI5Gvpk2NIUGPb8d8CtwpnyXgxeNSCEu_LmZOst8OzrD_TFhosN56pl6cih2hZNuGd72lmSStqTItndt7hADybl4fqAIUOpwQslO486NxHM6S1li6C67whm0dCA");
            string severKey = "AAAAX7kcx1M:APA91bEm8m-e28Rd6_ieuiZbRonHKJEBr9dbysm-ovQ_sujuZfRUfZMtd9CdpxVvMLdjrPMYkawM_BMAY7MEl1-q0VxZcmM1cK-1ddcBE5aWUr_W6Dp4DMrJODBmuZk2rT4XmfGmoxcX";
            string senderKey = "411127564115";
            WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            tRequest.Method = "post";
            //serverKey - Key from Firebase cloud messaging server  
            tRequest.Headers.Add(string.Format("Authorization: key={0}", severKey.ToString()));
            //Sender Id - From firebase project setting  
            tRequest.Headers.Add(string.Format("Sender: id={0}", senderKey.ToString()));
            tRequest.ContentType = "application/json";
            var payload = new
            {
                registration_ids = toKeys,
                priority = "high",
                content_available = true,
                notification = new
                {
                    body = "Testing the notification",
                    title = "Testing Multiple",
                    badge = 1
                },
                data = new
                {
                    key1 = "value1",
                    key2 = "value2"
                }
            };

            string postbody = JsonConvert.SerializeObject(payload).ToString();
            Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
            tRequest.ContentLength = byteArray.Length;
            using (Stream dataStream = tRequest.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
                using (WebResponse tResponse = tRequest.GetResponse())
                {
                    using (Stream dataStreamResponse = tResponse.GetResponseStream())
                    {
                        if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                String sResponseFromServer = tReader.ReadToEnd();
                            }
                    }
                }
            }
            return string.Empty;
        }

        #endregion
    }
}
