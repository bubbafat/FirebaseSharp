using System;

namespace FirebaseSharp.Portable
{
    public class PathCanceledEventArgs : EventArgs
    {
        public PathCanceledEventArgs()
        {
        }
    }

    public delegate void PathCanceledHandler(object sender, PathCanceledEventArgs e);

}
