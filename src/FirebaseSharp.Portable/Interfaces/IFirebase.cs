using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Interfaces
{
    public interface IFirebase : IFirebaseStructure, IFirebaseUpdate, IFirebaseQueryExecutorAny
    {
        IFirebaseApp GetApp();
    }
}


