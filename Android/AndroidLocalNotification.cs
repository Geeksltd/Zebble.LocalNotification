namespace Zebble
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Newtonsoft.Json;
    using System;
    using Zebble.Device;

    internal class AndroidLocalNotification
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public int Id { get; set; }
        public int IntentId { get; set; }
        public AndroidNotificationIcon Icon { get; set; }
        public AndroidNotificationIcon TransparentIcon { get; set; }
        public string TransparentIconColor { get; set; }
        public DateTime NotifyTime { get; set; }
        public bool PlaySound { set; get; }
        public string Parameters { get; set; }

        public Notification Render(Context context, string channelId)
        {
            Notification.Builder builder;

            if (OS.IsAtLeast(BuildVersionCodes.O)) builder = new Notification.Builder(context, channelId);
            else builder = new Notification.Builder(context);

            builder.SetContentTitle(Title);
            builder.SetContentText(Body);
            builder.SetAutoCancel(autoCancel: true);

            if (OS.IsAtLeast(BuildVersionCodes.Lollipop))
            {
                if (TransparentIcon?.Name.HasValue() == true)
                    builder.SetSmallIcon(TransparentIcon.ConvertToId(context));
                builder.SetColor(Color.Parse(TransparentIconColor.Or("transparent")).Render().ToArgb());
            }
            else
            {
                if (Icon?.Name.HasValue() == true)
                    builder.SetSmallIcon(Icon.ConvertToId(context));
            }

            if (PlaySound && !OS.IsAtLeast(BuildVersionCodes.O))
                builder.SetSound(LocalNotification.GetSoundUri(), LocalNotification.GetAudioAttributes());

            builder.SetContentIntent(CreateLaunchIntent(context));
            return builder.Build();
        }

        PendingIntent CreateLaunchIntent(Context context)
        {
            var resultIntent = UIRuntime.LauncherActivity;

            // In case the application needs launch data:
            resultIntent.PutExtra(LocalNotification.LocalNotificationKey, JsonConvert.SerializeObject(this));

            return PendingIntent.GetActivity(context, IntentId, resultIntent, PendingIntentFlags.UpdateCurrent);
        }
    }
}