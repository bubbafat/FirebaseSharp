using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Interfaces
{
    public interface IFirebase : IFirebaseQuery, IFirebaseStructure, IFirebaseUpdate, IFirebaseUserManager
    {
        IFirebaseApp GetApp();
    }
}


