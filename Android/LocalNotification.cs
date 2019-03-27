namespace Zebble.Device
{
    using Android.App;
    using Android.Content;
    using Android.Media;
    using Android.OS;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    public static partial class LocalNotification
    {
        public static int NotificationIconId { get; set; }

        public static string ChannelId { get; internal set; }

        internal static NotificationManager NotificationManager => NotificationManager.FromContext(Application.Context);

        static AlarmManager AlarmManager => UIRuntime.GetService<AlarmManager>(Context.AlarmService);

        public static Task<bool> Show(string title, string body, bool playSound = false)
        {
            var builder = new Notification.Builder(Application.Context)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetAutoCancel(autoCancel: true)
                .SetSmallIcon(UIRuntime.NotificationSmallIcon)
                .SetLargeIcon(UIRuntime.NotificationLargeIcon);

            if (OS.IsAtLeast(BuildVersionCodes.O)) builder.SetChannelId(ChannelId);

            if (playSound)
                builder.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification));


            if (NotificationIconId != 0) builder.SetSmallIcon(NotificationIconId);

            // Add parameters 
            var resultIntent = UIRuntime.LauncherActivity;
            resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask | ActivityFlags.ClearTop);
            resultIntent.PutExtra("LocalNotification", new Bundle());

            builder.SetContentIntent(resultIntent.ToPendingBroadcast());
            NotificationManager.Notify(0, builder.Build());

            return Task.FromResult(result: true);
        }

        public static Task<bool> Schedule(string title, string body, DateTime notifyTime, int id, bool playSound = false)
        {
            var intent = CreateIntent(id);

            var localNotification = new AndroidLocalNotification
            {
                Title = title,
                Body = body,
                Id = id,
                IconId = UIRuntime.NotificationSmallIcon,
                NotifyTime = notifyTime,
                PlaySound = playSound
            };

            if (NotificationIconId != 0) localNotification.IconId = NotificationIconId;

            var serializedNotification = SerializeNotification(localNotification);
            intent.PutExtra(ScheduledAlarmHandler.LocalNotificationKey, serializedNotification);

            AlarmManager.Set(AlarmType.RtcWakeup,
               triggerAtMillis: localNotification.NotifyTime.ToUnixEpoch(),
               operation: intent.ToPendingBroadcast());

            return Task.FromResult(result: true);
        }

        public static Task Cancel(int id)
        {
            AlarmManager.Cancel(CreateIntent(id).ToPendingBroadcast());
            NotificationManager.Cancel(id);

            return Task.CompletedTask;
        }

        public static Task Initialize(Intent intent)
        {
            var extras = intent.Extras;

            if (extras?.ContainsKey("LocalNotification") == true)
            {
                var parameters = extras.GetBundle("LocalNotification");
                Tapped?.Raise(parameters.ToArray<KeyValuePair<string, string>>());
            }

            if (OS.IsAtLeast(BuildVersionCodes.O))
            {
                ChannelId = UIRuntime.CurrentActivity.ApplicationContext.PackageName;
                var channel = new NotificationChannel(ChannelId, UIRuntime.CurrentActivity.ApplicationContext.ApplicationInfo.Name, NotificationImportance.Default);
                NotificationManager.CreateNotificationChannel(channel);
            }

            return Task.CompletedTask;
        }

        static Intent CreateIntent(int id)
        {
            return new Intent(Application.Context, typeof(ScheduledAlarmHandler))
                .SetAction("LocalNotifierIntent" + id);
        }

        static string SerializeNotification(AndroidLocalNotification notification)
        {
            var xmlSerializer = new XmlSerializer(notification.GetType());
            using (var writer = new StringWriter())
            {
                xmlSerializer.Serialize(writer, notification);
                return writer.ToString();
            }
        }
    }
}