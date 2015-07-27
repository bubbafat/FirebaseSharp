using System;
using System.Threading.Tasks;
using FakeItEasy;
using FirebaseSharp.Portable;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Messages;

namespace FirebaseSharp.Tests
{
    internal static class AppFactory
    {
        internal static FirebaseApp Empty()
        {
            return FromJson("{}");
        }

        internal static FirebaseApp FromJson(string json)
        {
            // after the connect call, make it look like some data has shown up.
            var connection = A.Fake<IFirebaseNetworkConnection>();
            A.CallTo(() => connection.Connect()).Invokes(() =>
            {
                var msg = new FirebaseMessage(WriteBehavior.Replace, new FirebasePath(), json, null, MessageSouce.Remote);
                var args = new FirebaseEventReceivedEventArgs(msg);

                // do it on a separate thread to make sure we don't ignore
                // locking issues during tests
                Task.Run(() => connection.Received += Raise.With(args));
            });

            return new FirebaseApp(new Uri("https://example.com/"), connection);
        }

        internal static FirebaseApp Dinosaurs()
        {
            string json =
                "{\"dinosaurs\":{\"bruhathkayosaurus\":{\"appeared\":-70000000,\"height\":25,\"length\":44,\"order\":\"saurischia\",\"vanished\":-70000000,\"weight\":135000},\"lambeosaurus\":{\"appeared\":-76000000,\"height\":2.1,\"length\":12.5,\"order\":\"ornithischia\",\"vanished\":-75000000,\"weight\":5000},\"linhenykus\":{\"appeared\":-85000000,\"height\":0.6,\"length\":1,\"order\":\"theropoda\",\"vanished\":-75000000,\"weight\":3},\"pterodactyl\":{\"appeared\":-150000000,\"height\":0.6,\"length\":0.8,\"order\":\"pterosauria\",\"vanished\":-148500000,\"weight\":2},\"stegosaurus\":{\"appeared\":-155000000,\"height\":4,\"length\":9,\"order\":\"ornithischia\",\"vanished\":-150000000,\"weight\":2500},\"triceratops\":{\"appeared\":-68000000,\"height\":3,\"length\":8,\"order\":\"ornithischia\",\"vanished\":-66000000,\"weight\":11000}},\"scores\":{\"bruhathkayosaurus\":55,\"lambeosaurus\":21,\"linhenykus\":80,\"pterodactyl\":93,\"stegosaurus\":5,\"triceratops\":22}}";

            return FromJson(json);
        }
    }
}