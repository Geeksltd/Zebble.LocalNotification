namespace Zebble
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

        public async static Task<bool> Show(string title, string body)
        {
            try
            {
                if (!await DevicePermission.LocalNotification.IsGranted())
                {
                    await Alert.Show("Permission was not granted to show local notifications.");
                    return false;
                }

                if (Device.OS.IsAtLeastiOS(10))
                {
                    var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(.1, repeats: false);
                    ShowUserNotification(title, body, 0, trigger);
                    return true;
                }
                else return await Schedule(title, body, DateTime.Now, 0);
            }
            catch (Exception ex)
            {
                Device.Log.Error(ex.Message);
                return false;
            }
        }

        public async static Task<bool> Schedule(string title, string body, DateTime notifyTime, int id)
        {
            if (!await DevicePermission.LocalNotification.IsGranted())
            {
                await Alert.Show("Permission was not granted to show local notifications.");
                return false;
            }

            if (Device.OS.IsAtLeastiOS(10))
            {
                var trigger = UNCalendarNotificationTrigger.CreateTrigger(GetNSDateComponentsFromDateTime(notifyTime), repeats: false);

                ShowUserNotification(title, body, id, trigger);
            }
            else
            {
                var notification = new UILocalNotification
                {
                    FireDate = (NSDate)notifyTime,
                    AlertTitle = title,
                    AlertBody = body,
                    UserInfo = NSDictionary.FromObjectAndKey(NSObject.FromObject(id), NSObject.FromObject(NOTIFICATION_KEY))
                };

                UIApplication.SharedApplication.ScheduleLocalNotification(notification);
            }

            return true;
        }

        public static Task Cancel(int id)
        {
            if (Device.OS.IsAtLeastiOS(10))
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

        public static Task Initialize(NSDictionary launchOptions)
        {
            if (launchOptions != null && launchOptions.ContainsKey(UIApplication.LaunchOptionsLocalNotificationKey))
            {
                if (launchOptions[UIApplication.LaunchOptionsLocalNotificationKey] is UILocalNotification)
                    Tapped?.Raise(new KeyValuePair<string, string>[] { });
            }

            UNUserNotificationCenter.Current.Delegate = new UserNotificationCenterDelegate();

            return Task.CompletedTask;
        }

        static void ShowUserNotification(string title, string body, int id, UNNotificationTrigger trigger)
        {
            if (Device.OS.IsBeforeiOS(10)) return;

            var content = new UNMutableNotificationContent() { Title = title, Body = body };

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
    }
}