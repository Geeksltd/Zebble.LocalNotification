namespace Zebble.Device
{
    using Android.Content;
    using Android.OS;
    using Newtonsoft.Json;
    using System;

    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Plugin Broadcast Receiver")]
    public class ScheduledAlarmHandler : BroadcastReceiver
    {
        internal event Action<AndroidLocalNotification, Android.App.Notification> Shown;

        public override void OnReceive(Context context, Intent intent /* Alarm Handler Intent*/)
        {
            var extra = intent.GetStringExtra(LocalNotification.LocalNotificationKey);
            if (extra.LacksValue()) return;

            try
            {
                var notification = JsonConvert.DeserializeObject<AndroidLocalNotification>(extra);
                if (notification != null)
                {
                    var native = LocalNotification.CreateNotification(notification, context.ApplicationContext);
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