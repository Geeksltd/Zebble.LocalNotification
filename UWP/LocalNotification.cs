namespace Zebble.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Data.Xml.Dom;
    using Windows.UI.Notifications;
    using Olive;

    public static partial class LocalNotification
    {
        const string TITLE_KEY = "TitleKey";
        const string BODY_KEY = "BodyKey";
        const string ID_KEY = "IdKey";
        const string DATETIME_KEY = "DateTimeKey";
        const string FMT = "O";
        const string TOAST_TEMPLATE = "<toast {3}>"
                                                  + "<visual>"
                                                  + "<binding template='ToastText02'>"
                                                  + "<text id='1'>{0}</text>"
                                                  + "<text id='2'>{1}</text>"
                                                  + "</binding>"
                                                  + "</visual>"
                                                  + "{2}"
                                                  + "</toast>";

        static string GetParameters(string title, string body, string id, DateTime notifyDateTime, Dictionary<string, string> parameters)
        {
            void setDetail()
            {
                parameters.Add(TITLE_KEY, title);
                parameters.Add(BODY_KEY, body);
                parameters.Add(ID_KEY, id);
                parameters.Add(DATETIME_KEY, notifyDateTime.ToString(FMT));
            }

            if (parameters != null) setDetail();
            else
            {
                parameters = new Dictionary<string, string>();
                setDetail();
            }

            return $"launch=\"{parameters.DicToString()}\"";
        }

        public static Task<bool> Show(string title, string body, bool playSound = false, Dictionary<string, string> parameters = null)
        {
            var param = GetParameters(title, body, "", DateTime.Now, parameters);
            var xmlData = string.Format(TOAST_TEMPLATE, title, body, playSound ? "<audio src='ms-winsoundevent:Notification.Reminder'/>" : string.Empty, param);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData);

            var toast = new ToastNotification(xmlDoc);

            var manager = ToastNotificationManager.CreateToastNotifier();

            manager.Show(toast);

            return Task.FromResult(result: true);
        }

        public static Task<bool> Schedule(string title, string body, DateTime notifyTime, string id, bool playSound = false, Dictionary<string, string> parameters = null, int priority = 0)
        {
            var param = GetParameters(title, body, id, notifyTime, parameters);
            var xmlData = string.Format(TOAST_TEMPLATE, title, body, playSound ? "<audio src='ms-winsoundevent:Notification.Reminder'/>" : string.Empty, param);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData);

            var correctedTime = notifyTime <= DateTime.Now
              ? DateTime.Now.AddMilliseconds(100)
              : notifyTime;

            var scheduledToastNotification = new ScheduledToastNotification(xmlDoc, new DateTimeOffset(correctedTime)) { Id = id };

            ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledToastNotification);

            return Task.FromResult(result: true);
        }

        public static Task Cancel(string id)
        {
            var scheduledNotifications = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
            var notification =
                scheduledNotifications.FirstOrDefault(n => n.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

            if (notification != null)
                ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(notification);

            return Task.CompletedTask;
        }

        public static Task Initialize(Action<Notification> onTapped)
        {
            if (onTapped == null) return Task.CompletedTask;

            UIRuntime.OnParameterRecieved.Handle(args =>
            {
                onTapped.Invoke(new Notification
                {
                    Title = args[TITLE_KEY],
                    Body = args[BODY_KEY],
                    Id = args[ID_KEY],
                    NotifyTime = DateTime.ParseExact(args[DATETIME_KEY], FMT, System.Globalization.CultureInfo.InvariantCulture),
                    Parameters = args.Except(x => x.Key == TITLE_KEY || x.Key == BODY_KEY || x.Key == ID_KEY || x.Key == DATETIME_KEY)
                    .ToDictionary(x => x.Key, x => x.Value)
                });
            });

            return Task.CompletedTask;
        }

        public static void UpdateBadgeCount(int value)
        {
            var badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);

            var badgeElement = badgeXml.SelectSingleNode("/badge") as XmlElement;
            badgeElement.SetAttribute("value", value.ToString());
            var badge = new BadgeNotification(badgeXml);
            var badgeUpdater =
                BadgeUpdateManager.CreateBadgeUpdaterForApplication();

            badgeUpdater.Update(badge);
        }

        public static void RemoveBadgeCount()
        {
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
        }
    }
}