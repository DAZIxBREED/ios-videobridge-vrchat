using System;

namespace DAZIxBREED.IOSVideoBridge
{
    public static class VideoSeekUtility
    {
        public static bool TryClampTarget(double requestedSeconds, double durationSeconds, out double targetSeconds)
        {
            targetSeconds = 0.0;
            if (double.IsNaN(requestedSeconds) || double.IsInfinity(requestedSeconds))
            {
                return false;
            }

            targetSeconds = Math.Max(0.0, requestedSeconds);
            if (durationSeconds > 0.0 && !double.IsNaN(durationSeconds) && !double.IsInfinity(durationSeconds))
            {
                targetSeconds = Math.Min(targetSeconds, Math.Max(0.0, durationSeconds - 0.05));
            }

            return true;
        }
    }
}
