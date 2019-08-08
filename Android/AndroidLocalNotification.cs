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
        public int IconId { get; set; }
        public int TransparentIconId { get; set; }
        public string TransparentIconColor { get; set; }
        public DateTime NotifyTime { get; set; }
        public bool PlaySound { set; get; }
        public string Parameters { get; set; }

        public Notification Render(Context context, string channelId)
        {
            Notification.Builder builder;

            if (OS.IsAtLeast(BuildVersionCodes.O)) builder = new Notification.Builder(Application.Context, channelId);
            else builder = new Notification.Builder(Application.Context);

            builder.SetContentTitle(Title);
            builder.SetContentText(Body);
            builder.SetAutoCancel(autoCancel: true);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                builder.SetSmallIcon(TransparentIconId);
                builder.SetColor(Color.Parse(TransparentIconColor.Or("transparent")).Render().ToArgb());
            }
            else
            {
                builder.SetSmallIcon(IconId);
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