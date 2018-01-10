﻿namespace Zebble
{
    using Android.App;
    using Android.Content;
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

            var builder = new Notification.Builder(Application.Context)
                .SetContentTitle(notification.Title)
                .SetContentText(notification.Body)
                .SetSmallIcon(notification.IconId)
                .SetAutoCancel(autoCancel: true);

            var resultIntent = UIRuntime.LauncherActivity;
            resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);

            var resultPendingIntent = PendingIntent.GetActivity(Application.Context, 0, resultIntent, PendingIntentFlags.UpdateCurrent);

            builder.SetContentIntent(resultPendingIntent);

            LocalNotification.NotificationManager.Notify(notification.Id, builder.Build());
        }

        AndroidLocalNotification DeserializeNotification(string notificationString)
        {
            var xmlSerializer = new XmlSerializer(typeof(AndroidLocalNotification));
            using (var stringReader = new StringReader(notificationString))
            {
                var notification = (AndroidLocalNotification)xmlSerializer.Deserialize(stringReader);
                return notification;
            }
        }
    }
}