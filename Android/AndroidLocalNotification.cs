namespace Zebble
{
    using System;

    internal class AndroidLocalNotification
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public int Id { get; set; }
        public int IntentId { get; set; }
        public int IconId { get; set; }
        public DateTime NotifyTime { get; set; }
        public bool PlaySound { set; get; }
        public string Parameters { get; set; }
    }
}