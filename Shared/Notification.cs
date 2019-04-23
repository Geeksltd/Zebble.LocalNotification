namespace Zebble.Device
{
    using System;
    using System.Collections.Generic;

    public static partial class LocalNotification
    {
        public class Notification
        {
            public string Title { get; set; }
            public string Body { get; set; }
            public int Id { get; set; }
            public DateTime NotifyTime { get; set; }
            public Dictionary<string, string> Parameters { get; set; }
        }
    }
}
