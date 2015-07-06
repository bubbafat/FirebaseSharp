using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;

namespace FirebaseSharp.Portable.Interfaces
{
    public class FirebaseError
    {
        public FirebaseError(string code)
        {
            Code = code;
        }

        public string Code
        {
            get; private set;
            
        }
    }
            public delegate void FirebaseStatusCallback(FirebaseError error);

    public interface IFirebaseUserManager
    {
        void CreateUser(string email, string password, FirebaseStatusCallback callback = null);
        void ChangeEmail(string oldEmail, string newEmail, string password, FirebaseStatusCallback callback = null);
        void ChangePassword(string email, string oldPassword, string newPassword, FirebaseStatusCallback callback = null);
        void RemoveUser(string email, string password, FirebaseStatusCallback callback = null);
        void ResetPassword(string email, FirebaseStatusCallback callback = null);
    }
}
