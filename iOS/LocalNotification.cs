namespace Zebble.Device
{
    using Foundation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UIKit;
    using UserNotifications;

    public static partial class LocalNotification
    {
        const string NOTIFICATION_KEY = "LocalNotificationKey";
        const string NOTIFICATION_PARAM_KEY = "LocalNotificationParamKey";
        const string NOTIFICATION_TITLE_KEY = "LocalNotificationTitleKey";
        const string NOTIFICATION_BODY_KEY = "LocalNotificationBodyKey";
        const string NOTIFICATION_Date_KEY = "LocalNotificationDateKey";

        public async static Task<bool> Show(string title, string body, bool playSound = false, Dictionary<string, string> parameters = null)
        {
            try
            {
                if (!await Permission.LocalNotification.IsGranted())
                {
                    await Alert.Show("Permission was not granted to show local notifications.");
                    return false;
                }

                if (OS.IsAtLeastiOS(10))
                {
                    var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(.1, repeats: false);
                    ShowUserNotification(title, body, 0, trigger, playSound, parameters);
                    return true;
                }
                else return await Schedule(title, body, DateTime.Now, 0, parameters: parameters);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return false;
            }
        }

        public async static Task<bool> Schedule(string title, string body, DateTime notifyTime, int id, bool playSound = false, Dictionary<string, string> parameters = null)
        {
            if (!await Permission.LocalNotification.IsGranted())
            {
                await Alert.Show("Permission was not granted to show local notifications.");
                return false;
            }

            if (OS.IsAtLeastiOS(10))
            {
                var trigger = UNCalendarNotificationTrigger.CreateTrigger(GetNSDateComponentsFromDateTime(notifyTime), repeats: false);

                ShowUserNotification(title, body, id, trigger, playSound, parameters);
            }
            else
            {
                var userData = NSDictionary.FromObjectsAndKeys(
                    new NSObject[] { id.ToString().ToNs(), parameters.DicToString().ToNs(), title.ToNs(), body.ToNs(), (NSDate)notifyTime },
                    new NSObject[] { NOTIFICATION_KEY.ToNs(), NOTIFICATION_PARAM_KEY.ToNs(),
                    NOTIFICATION_TITLE_KEY.ToNs(), NOTIFICATION_BODY_KEY.ToNs(), NOTIFICATION_Date_KEY.ToNs() });

                var notification = new UILocalNotification
                {
                    FireDate = (NSDate)notifyTime,
                    AlertTitle = title,
                    AlertBody = body,
                    UserInfo = userData
                };

                UIApplication.SharedApplication.ScheduleLocalNotification(notification);
            }

            return true;
        }

        public static Task Cancel(int id)
        {
            if (OS.IsAtLeastiOS(10))
            {
                UNUserNotificationCenter.Current.RemovePendingNotificationRequests(new string[] { id.ToString() });
                UNUserNotificationCenter.Current.RemoveDeliveredNotifications(new string[] { id.ToString() });
            }
            else
            {
                var notifications = UIApplication.SharedApplication.ScheduledLocalNotifications;
                var notification = notifications.Where(n => n.UserInfo.ContainsKey(NSObject.FromObject(NOTIFICATION_KEY)))
                    .FirstOrDefault(n => n.UserInfo[NOTIFICATION_KEY].Equals(NSObject.FromObject(id)));

                if (notification != null) UIApplication.SharedApplication.CancelLocalNotification(notification);
            }

            return Task.CompletedTask;
        }

        public static Task Initialize(NSDictionary launchOptions, Action<Notification> OnTapped = null)
        {
            if (launchOptions != null && launchOptions.ContainsKey(UIApplication.LaunchOptionsLocalNotificationKey))
            {
                if (launchOptions[UIApplication.LaunchOptionsLocalNotificationKey] is UILocalNotification notification)
                    OnNotificationTapped(OnTapped, notification.UserInfo, isApplicationClosed: true);
            }

            UIRuntime.OnParameterRecieved.Handle(param => OnNotificationTapped(OnTapped, param: param));

            UNUserNotificationCenter.Current.Delegate = new UserNotificationCenterDelegate();

            return Task.CompletedTask;
        }

        static void ShowUserNotification(string title, string body, int id, UNNotificationTrigger trigger, bool playSound = false, Dictionary<string, string> parameters = null)
        {
            if (OS.IsBeforeiOS(10)) return;

            var userData = NSDictionary.FromObjectsAndKeys(
                new NSObject[] { id.ToString().ToNs(), parameters.DicToString().ToNs(), title.ToNs(), body.ToNs(), NSDate.Now },
                new NSObject[] { NOTIFICATION_KEY.ToNs(), NOTIFICATION_PARAM_KEY.ToNs(),
                    NOTIFICATION_TITLE_KEY.ToNs(), NOTIFICATION_BODY_KEY.ToNs(), NOTIFICATION_Date_KEY.ToNs() });

            var content = new UNMutableNotificationContent()
            {
                Title = title,
                Body = body,
                UserInfo = userData
            };

            content.Sound = UNNotificationSound.Default;

            var request = UNNotificationRequest.FromIdentifier(id.ToString(), content, trigger);

            UNUserNotificationCenter.Current.AddNotificationRequest(request, (error) => { });
        }

        static NSDateComponents GetNSDateComponentsFromDateTime(DateTime dateTime)
        {
            return new NSDateComponents
            {
                Month = dateTime.Month,
                Day = dateTime.Day,
                Year = dateTime.Year,
                Hour = dateTime.Hour,
                Minute = dateTime.Minute,
                Second = dateTime.Second
            };
        }

        static Task OnNotificationTapped(Action<Notification> OnTapped, NSDictionary param = null, bool isApplicationClosed = false)
        {
            if (param == null || OnTapped == null) return Task.CompletedTask;

            if (!isApplicationClosed) raiseTapped();
            else Task.Delay(1000).ContinueWith(t => raiseTapped());

            void raiseTapped()
            {
                if (param == null) return;

                var id = Convert.ToInt32(param.ValueForKey(NOTIFICATION_KEY.ToNs()).ToObject().ToString());
                var title = param.ValueForKey(NOTIFICATION_TITLE_KEY.ToNs()).ToObject().ToString();
                var body = param.ValueForKey(NOTIFICATION_BODY_KEY.ToNs()).ToObject().ToString();
                var parameters = param.ValueForKey(NOTIFICATION_PARAM_KEY.ToNs()).ToObject().ToString().StringToDic();
                var notifyTime = (DateTime)param.ValueForKey(NOTIFICATION_Date_KEY.ToNs()).ToObject(typeof(DateTime));

                OnTapped?.Invoke(new Notification
                {
                    Id = id,
                    Body = body,
                    Title = title,
                    NotifyTime = notifyTime,
                    Parameters = parameters
                });
            }

            return Task.CompletedTask;
        }

        static object ToObject(this NSObject obj, Type type = null)
        {
            if (obj is NSString) return obj.ToString();

            if (obj is NSNumber number && type == typeof(int)) return Convert.ToInt32(number.Int32Value);

            if (obj is NSDate date && type == typeof(DateTime)) return DateTime.SpecifyKind((DateTime)date, DateTimeKind.Unspecified);

            return null;
        }
    }
}