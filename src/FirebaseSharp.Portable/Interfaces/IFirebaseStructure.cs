using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirebaseSharp.Portable.Interfaces
{
    public interface IFirebaseStructure
    {
        IFirebase Child(string childPath);
        IFirebase Parent();
        IFirebase Root();
        string Key { get; }
        Uri AbsoluteUri { get; }

    }
}
