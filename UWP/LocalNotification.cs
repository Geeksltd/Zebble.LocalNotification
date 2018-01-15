namespace Zebble
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Data.Xml.Dom;
    using Windows.UI.Notifications;

    public static partial class LocalNotification
    {
        const string TOAST_TEMPLATE = "<toast>"
                                                  + "<visual>"
                                                  + "<binding template='ToastText02'>"
                                                  + "<text id='1'>{0}</text>"
                                                  + "<text id='2'>{1}</text>"
                                                  + "</binding>"
                                                  + "</visual>"
                                                  + "</toast>";

        public static Task<bool> Show(string title, string body)
        {
            var xmlData = string.Format(TOAST_TEMPLATE, title, body);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData);

            var toast = new ToastNotification(xmlDoc);

            var manager = ToastNotificationManager.CreateToastNotifier();

            manager.Show(toast);

            return Task.FromResult(result: true);
        }

        public static Task<bool> Schedule(string title, string body, DateTime notifyTime, int id)
        {
            var xmlData = string.Format(TOAST_TEMPLATE, title, body);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData);

            var correctedTime = notifyTime <= DateTime.Now
              ? DateTime.Now.AddMilliseconds(100)
              : notifyTime;

            var scheduledTileNotification = new ScheduledTileNotification(xmlDoc, correctedTime) { Id = id.ToString() };

            TileUpdateManager.CreateTileUpdaterForApplication().AddToSchedule(scheduledTileNotification);

            return Task.FromResult(result: true);
        }

        public static Task Cancel(int id)
        {
            var scheduledNotifications = TileUpdateManager.CreateTileUpdaterForApplication().GetScheduledTileNotifications();
            var notification =
                scheduledNotifications.FirstOrDefault(n => n.Id.Equals(id.ToString(), StringComparison.OrdinalIgnoreCase));

            if (notification != null)
                TileUpdateManager.CreateTileUpdaterForApplication().RemoveFromSchedule(notification);

            return Task.CompletedTask;
        }

        public static Task Initialize() => Task.CompletedTask;
    }
}