namespace Zebble
{
    using Android.App;
    using Context = Android.Content.Context;
    using Android.OS;
    using Newtonsoft.Json;
    using System;
    using Zebble.Device;
    using Olive;
    using System.Collections.Generic;
    using Android.Content;
    using Java.Lang;

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
        public string LaunchActivityClassName { get; set; }
        public Dictionary<string, string> Parameters { get; set; }

        public Notification Render(Context context)
        {
            Class activityClass;

            if (UIRuntime.CurrentActivity is not null)
                activityClass = Class.FromType(UIRuntime.CurrentActivity.GetType());
            else
                activityClass = Class.ForName(LaunchActivityClassName);

#if MONOANDROID
            var builder = new AndroidX.Core.App.NotificationCompat.Builder(context, ChannelId);
#else
            var builder = new Notification.Builder(context, ChannelId);
#endif

            builder
                .SetContentTitle(Title)
                .SetContentText(Body)
                .SetCategory(Notification.CategoryMessage)
                .SetContentIntent(CreateLaunchIntent(context, activityClass))
                .SetPriority(Priority)
                .SetAutoCancel(IsAutoCancel)
                .SetWhen(new DateTimeOffset(NotifyTime).ToUnixTimeMilliseconds());

#if MONOANDROID
            builder.SetVisibility((int)NotificationVisibility.Public);
#else
            builder.SetVisibility(NotificationVisibility.Public);
#endif

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

        PendingIntent CreateLaunchIntent(Context context, Class activityClass)
        {
            Intent intent = new Intent(context, activityClass);
            intent.PutExtra(LocalNotification.LocalNotificationKey, JsonConvert.SerializeObject(this));

            return PendingIntent.GetActivity(context, IntentId, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        }
    }
}