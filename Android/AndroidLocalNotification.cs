namespace Zebble
{
    using Android.App;
    using Context = Android.Content.Context;
    using Android.OS;
    using AndroidX.Core.App;
    using Newtonsoft.Json;
    using System;
    using Zebble.Device;
    using Olive;
    using Android.Content;
    using System.Collections.Generic;

    class AndroidLocalNotification
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public int Id { get; set; }
        public int IntentId { get; set; }
        public AndroidNotificationIcon Icon { get; set; }
        public AndroidNotificationIcon TransparentIcon { get; set; }
        public string TransparentIconColor { get; set; }
        public DateTime NotifyTime { get; set; } = DateTime.Now;
        public bool PlaySound { set; get; }
        public Dictionary<string, string> Parameters { get; set; }

        public Notification Render(Context context, string channelId)
        {
            var builder = new NotificationCompat.Builder(context, channelId)
                .SetContentTitle(Title)
                .SetContentText(Body)
                .SetVisibility((int)NotificationVisibility.Public)
                .SetCategory(Notification.CategoryMessage)
                .SetContentIntent(CreateLaunchIntent(context));

            builder.SetWhen(NotifyTime.ToUnixTime());

            if (Icon?.Name.HasValue() == true)
                builder.SetSmallIcon(Icon.ConvertToId(context));

            if (OS.IsAtLeast(BuildVersionCodes.Lollipop) && TransparentIcon?.Name.HasValue() == true)
            {
                builder.SetSmallIcon(TransparentIcon.ConvertToId(context));
                builder.SetColor(Color.Parse(TransparentIconColor.Or("transparent")).Render().ToArgb());
            }

            if (PlaySound) builder.SetSound(LocalNotification.GetSoundUri());

            return builder.Build();
        }

        PendingIntent CreateLaunchIntent(Context context)
        {
            var intent = new Intent(context, UIRuntime.CurrentActivity.GetType())
                .PutExtra(LocalNotification.LocalNotificationKey, JsonConvert.SerializeObject(this));

            return PendingIntent.GetActivity(context, IntentId, intent, PendingIntentFlags.UpdateCurrent);
        }
    }
}