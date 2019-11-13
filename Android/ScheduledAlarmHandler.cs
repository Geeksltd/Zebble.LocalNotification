namespace Zebble.Device
{
    using Android.Content;
    using Newtonsoft.Json;
    using System;

    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Plugin Broadcast Receiver")]
    public class ScheduledAlarmHandler : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent /* Alarm Handler Intent*/)
        {
            var extra = intent.GetStringExtra(LocalNotification.LocalNotificationKey);
            if (extra.LacksValue()) return;

            try
            {
                var notification = JsonConvert.DeserializeObject<AndroidLocalNotification>(extra);
                if (notification != null)
                {
                    LocalNotification.CreateNotification(notification, context.ApplicationContext);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}