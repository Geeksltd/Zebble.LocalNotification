using Android.Media;

namespace Zebble.Device
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Widget;
    using System;
    using System.IO;
    using System.Xml.Serialization;

    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Plugin Broadcast Receiver")]
    public class ScheduledAlarmHandler : BroadcastReceiver
    {
        public const string LocalNotificationKey = "LocalNotification";

        public override void OnReceive(Context context, Intent intent)
        {
            var extra = intent.GetStringExtra(LocalNotificationKey);
            var notification = DeserializeNotification(extra);

            if (notification == null) return;

            var builder = new Notification.Builder(context)
                .SetContentTitle(notification.Title)
                .SetContentText(notification.Body)
                .SetSmallIcon(notification.IconId)
                .SetAutoCancel(autoCancel: true);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelId = Guid.NewGuid().ToString();
                var channel = new NotificationChannel(channelId, "app_channel_name", NotificationImportance.Default);
                LocalNotification.NotificationManager.CreateNotificationChannel(channel);

                builder.SetChannelId(channelId);
            }

            if (notification.PlaySound) builder.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification));

            var resultIntent = UIRuntime.LauncherActivity;
            resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);

            var resultPendingIntent = PendingIntent.GetActivity(context, 0, resultIntent, PendingIntentFlags.UpdateCurrent);

            builder.SetContentIntent(resultPendingIntent);

            LocalNotification.NotificationManager.Notify(notification.Id, builder.Build());
        }

        AndroidLocalNotification DeserializeNotification(string notificationString)
        {
            if (string.IsNullOrEmpty(notificationString)) return null;

            var xmlSerializer = new XmlSerializer(typeof(AndroidLocalNotification));
            using (var stringReader = new StringReader(notificationString))
            {
                var notification = (AndroidLocalNotification)xmlSerializer.Deserialize(stringReader);
                return notification;
            }
        }
    }

    [Service(Enabled = true)]
    public class ScheduledAlarmService : Service
    {
        public override IBinder OnBind(Intent intent) => null;

        public override void OnCreate()
        {
            base.OnCreate();

            var intentFilter = new IntentFilter();
            intentFilter.AddAction("android.intent.action.SCREEN_ON");
            intentFilter.AddAction("android.intent.action.SCREEN_OFF");
            intentFilter.AddAction("android.intent.action.BOOT_COMPLETED");

            intentFilter.Priority = 100;

            LocalNotification.ReceiverInstance = new ScheduledAlarmHandler();
            RegisterReceiver(LocalNotification.ReceiverInstance, intentFilter);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (LocalNotification.ReceiverInstance != null) UnregisterReceiver(LocalNotification.ReceiverInstance);
        }
    }
}