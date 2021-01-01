namespace Zebble.Device
{
    using Android.Content;
    using Context = Android.Content.Context;
    using Newtonsoft.Json;
    using System;
    using Olive;

    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Plugin Broadcast Receiver")]
    public class ScheduledAlarmHandler : BroadcastReceiver
    {
        internal event Action<AndroidLocalNotification, Android.App.Notification> Shown;

        public override void OnReceive(Context context, Intent intent /* Alarm Handler Intent*/)
        {
            var extra = intent.GetStringExtra(LocalNotification.LocalNotificationKey);

            if (extra.IsEmpty()) return;

            try
            {
                var notification = JsonConvert.DeserializeObject<AndroidLocalNotification>(extra);
                if (notification != null)
                {
                    var native = notification.Register(context.ApplicationContext);
                    Shown?.Invoke(notification, native);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}