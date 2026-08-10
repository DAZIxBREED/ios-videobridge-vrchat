namespace DAZIxBREED.IOSVideoBridge
{
    public static class VideoPlaybackStatePolicy
    {
        public static bool CanTransition(VideoPlaybackState current, VideoPlaybackState next)
        {
            if (current == next || next == VideoPlaybackState.Idle)
            {
                return true;
            }

            switch (current)
            {
                case VideoPlaybackState.Idle:
                    return next == VideoPlaybackState.Loading ||
                           next == VideoPlaybackState.Stopped ||
                           next == VideoPlaybackState.Failed;

                case VideoPlaybackState.Loading:
                    return next == VideoPlaybackState.Preparing ||
                           next == VideoPlaybackState.Stopped ||
                           next == VideoPlaybackState.Failed;

                case VideoPlaybackState.Preparing:
                    return next == VideoPlaybackState.Ready ||
                           next == VideoPlaybackState.Loading ||
                           next == VideoPlaybackState.Stopped ||
                           next == VideoPlaybackState.Failed;

                case VideoPlaybackState.Ready:
                    return next == VideoPlaybackState.Playing ||
                           next == VideoPlaybackState.Loading ||
                           next == VideoPlaybackState.Stopped ||
                           next == VideoPlaybackState.Failed;

                case VideoPlaybackState.Playing:
                    return next == VideoPlaybackState.Ready ||
                           next == VideoPlaybackState.Paused ||
                           next == VideoPlaybackState.Buffering ||
                           next == VideoPlaybackState.Recovering ||
                           next == VideoPlaybackState.Loading ||
                           next == VideoPlaybackState.Stopped ||
                           next == VideoPlaybackState.Failed;

                case VideoPlaybackState.Paused:
                    return next == VideoPlaybackState.Playing ||
                           next == VideoPlaybackState.Recovering ||
                           next == VideoPlaybackState.Loading ||
                           next == VideoPlaybackState.Stopped ||
                           next == VideoPlaybackState.Failed;

                case VideoPlaybackState.Buffering:
                    return next == VideoPlaybackState.Ready ||
                           next == VideoPlaybackState.Playing ||
                           next == VideoPlaybackState.Paused ||
                           next == VideoPlaybackState.Recovering ||
                           next == VideoPlaybackState.Loading ||
                           next == VideoPlaybackState.Stopped ||
                           next == VideoPlaybackState.Failed;

                case VideoPlaybackState.Recovering:
                    return next == VideoPlaybackState.Preparing ||
                           next == VideoPlaybackState.Ready ||
                           next == VideoPlaybackState.Playing ||
                           next == VideoPlaybackState.Paused ||
                           next == VideoPlaybackState.Loading ||
                           next == VideoPlaybackState.Stopped ||
                           next == VideoPlaybackState.Failed;

                case VideoPlaybackState.Failed:
                    return next == VideoPlaybackState.Loading ||
                           next == VideoPlaybackState.Recovering ||
                           next == VideoPlaybackState.Stopped;

                case VideoPlaybackState.Stopped:
                    return next == VideoPlaybackState.Loading ||
                           next == VideoPlaybackState.Preparing ||
                           next == VideoPlaybackState.Failed;

                default:
                    return false;
            }
        }
    }
}
