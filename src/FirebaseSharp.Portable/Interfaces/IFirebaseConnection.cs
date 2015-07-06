using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirebaseSharp.Portable.Interfaces
{
    public interface IFirebaseConnection
    {
        void GoOnline();
        void GoOffline();
    }
}
