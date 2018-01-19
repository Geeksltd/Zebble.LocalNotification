namespace Zebble.Device
{
    using Android.App;
    using Android.Content;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    public static partial class LocalNotification
    {
        public static int NotificationIconId { get; set; }

        internal static NotificationManager NotificationManager => NotificationManager.FromContext(Application.Context);

        static AlarmManager AlarmManager => UIRuntime.GetService<AlarmManager>(Context.AlarmService);

        static PendingIntent ToPendingBroadcast(Intent intent)
        {
            return PendingIntent.GetBroadcast(Application.Context, 0, intent, PendingIntentFlags.CancelCurrent);
        }

        public static Task<bool> Show(string title, string body)
        {
            var builder = new Notification.Builder(Application.Context)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetAutoCancel(autoCancel: true)
                .SetSmallIcon(UIRuntime.NotificationSmallIcon)
                .SetLargeIcon(UIRuntime.NotificationLargeIcon);

            if (NotificationIconId != 0) builder.SetSmallIcon(NotificationIconId);

            // Add parameters
            var bundle = new Android.OS.Bundle();

            var resultIntent = UIRuntime.LauncherActivity;
            resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask | ActivityFlags.ClearTop);
            resultIntent.PutExtra("LocalNotification", bundle);

            var resultPendingIntent = PendingIntent.GetActivity(Application.Context, 0, resultIntent, PendingIntentFlags.UpdateCurrent);

            builder.SetContentIntent(resultPendingIntent);
            NotificationManager.Notify(0, builder.Build());

            return Task.FromResult(result: true);
        }

        public static Task<bool> Schedule(string title, string body, DateTime notifyTime, int id)
        {
            var intent = CreateIntent(id);

            var localNotification = new AndroidLocalNotification
            {
                Title = title,
                Body = body,
                Id = id,
                IconId = UIRuntime.NotificationSmallIcon,
                NotifyTime = notifyTime
            };

            if (NotificationIconId != 0) localNotification.IconId = NotificationIconId;

            var serializedNotification = SerializeNotification(localNotification);
            intent.PutExtra(ScheduledAlarmHandler.LocalNotificationKey, serializedNotification);

            AlarmManager.Set(AlarmType.RtcWakeup,
               triggerAtMillis: localNotification.NotifyTime.ToUnixEpoch(),
               operation: ToPendingBroadcast(intent));

            return Task.FromResult(result: true);
        }

        public static Task Cancel(int id)
        {
            AlarmManager.Cancel(ToPendingBroadcast(CreateIntent(id)));
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