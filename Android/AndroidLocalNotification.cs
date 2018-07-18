namespace Zebble
{
    using System;

    public class AndroidLocalNotification
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public int Id { get; set; }
        public int IconId { get; set; }
        public DateTime NotifyTime { get; set; }
        public bool PlaySound { set; get; }
    }
}