namespace Zebble.Device
{
    public static partial class LocalNotification
    {
        public static void UpdateBadgeCount(int value)
        {
            XamarinShortcutBadger.ShortcutBadger.ApplyCount(UIRuntime.CurrentActivity, value);
        }

        public static void RemoveBadgeCount()
        {
            XamarinShortcutBadger.ShortcutBadger.RemoveCount(UIRuntime.CurrentActivity);
        }
    }
}