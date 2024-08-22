namespace Zebble
{
    using Android.App;
    using Android.Content;

    static class LocalNotificationExtensions
    {
        public static PendingIntent ToPendingBroadcast(this Intent intent, Android.Content.Context context, int requestCode)
        {
            return PendingIntent.GetBroadcast(context, requestCode, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        }
    }
}
