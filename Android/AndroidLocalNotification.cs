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
    using System.Collections.Generic;
    using Android.Content;

    class AndroidLocalNotification
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Id { get; set; }
        public int IntentId { get; set; }
        public string ChannelId { get; set; }
        public AndroidNotificationIcon Icon { get; set; }
        public AndroidNotificationIcon TransparentIcon { get; set; }
        public string TransparentIconColor { get; set; }
        public DateTime NotifyTime { get; set; } = DateTime.Now;
        public bool PlaySound { set; get; }
        public bool IsAutoCancel { get; set; }
        public int Priority { set; get; }
        public Dictionary<string, string> Parameters { get; set; }

        public Notification Render(Context context)
        {
            var builder = new NotificationCompat.Builder(context, ChannelId)
                .SetContentTitle(Title)
                .SetContentText(Body)
                .SetVisibility((int)NotificationVisibility.Public)
                .SetCategory(Notification.CategoryMessage)
                .SetContentIntent(CreateLaunchIntent(context))
                .SetPriority(Priority)
                .SetAutoCancel(IsAutoCancel)
                .SetWhen(new DateTimeOffset(NotifyTime).ToUnixTimeMilliseconds());
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
            Intent intent = new Intent(context, UIRuntime.CurrentActivity.GetType());
            intent.PutExtra(LocalNotification.LocalNotificationKey, JsonConvert.SerializeObject(this));

            return PendingIntent.GetActivity(context, IntentId, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        }
    }
}