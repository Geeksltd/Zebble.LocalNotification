namespace Zebble
{
    using Android.App;
    using Android.Content;
    using Android.Media;
    using Android.OS;
    using Android.Support.V4.App;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
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
        public DateTime NotifyTime { get; set; } = DateTime.Now;
        public bool PlaySound { set; get; }
        public string Parameters { get; set; }

        public Notification Render(Context context, string channelId)
        {
            // Instantiate the builder and set notification elements:
            var builder = new NotificationCompat.Builder(context, channelId)
                .SetContentTitle(Title)
                .SetContentText(Body)
                .SetVisibility((int)NotificationVisibility.Public)
                .SetCategory(Notification.CategoryMessage)
                .SetContentIntent(CreateLaunchIntent(context));

            builder.SetWhen(NotifyTime.ToUnixEpoch());

            if (Icon?.Name.HasValue() == true)
                builder.SetSmallIcon(Icon.ConvertToId(context));

            if (OS.IsAtLeast(BuildVersionCodes.Lollipop) && TransparentIcon?.Name.HasValue() == true)
            {
                builder.SetSmallIcon(TransparentIcon.ConvertToId(context));
                builder.SetColor(Color.Parse(TransparentIconColor.Or("transparent")).Render().ToArgb());
            }

            if (PlaySound) builder.SetSound(LocalNotification.GetSoundUri());

            var notification = builder.Build();

            var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager.Notify(Id, notification);

            EnsureScreenLightIsOn(context);

            return notification;
        }

        void EnsureScreenLightIsOn(Context context)
        {
            try
            {
                var pm = (PowerManager)context.GetSystemService(Context.PowerService);

                var isScreenOn = OS.IsAtLeast(BuildVersionCodes.KitkatWatch) ? pm.IsInteractive : pm.IsScreenOn;
                if (!isScreenOn)
                {
                    var wl = pm.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup, GetType().FullName);
                    wl.Acquire(3000); //set your time in milliseconds
                }
            }
            catch
            {

            }
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