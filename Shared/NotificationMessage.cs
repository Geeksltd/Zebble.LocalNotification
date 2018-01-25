namespace Zebble
{
    using Newtonsoft.Json.Linq;

    public class NotificationMessage
    {
        public NotificationMessage(JObject data) => Data = data;

        public JObject Data { get; }
    }
}
