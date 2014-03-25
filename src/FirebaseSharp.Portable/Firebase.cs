using System;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable
{
    public sealed class Firebase : IDisposable
    {
        private readonly Request _request;
        public Firebase(Uri rootUri)
        {
            if (rootUri == null)
            {
                throw new ArgumentNullException("rootUri");
            }

            _request = new Request(rootUri);
        }

        public Uri RootUri
        {
            get { return _request.RootUri; }
        }

        public string Post(string path, string payload)
        {
            return PostAsync(path, payload).Result;
        }

        public async Task<string> PostAsync(string path, string payload)
        {
            return await _request.Post(BuildPath(path), payload);
        }


        public string Put(string path, string payload)
        {
            return PutAsync(path, payload).Result;
        }

        public async Task<string> PutAsync(string path, string payload)
        {
            return await _request.Put(BuildPath(path), payload);
        }

        public void Patch(string path, string payload)
        {
            PatchAsync(path, payload).Wait();
        }

        public async Task PatchAsync(string path, string payload)
        {
            await _request.Patch(BuildPath(path), payload);
        }

        public void Delete(string path)
        {
            DeleteAsync(path).Wait();
        }
        public async Task DeleteAsync(string path)
        {
            await _request.Delete(BuildPath(path));
        }

        public string Get(string path)
        {
            return GetAsync(path).Result;
        }

        public async Task<string> GetAsync(string path)
        {
            return await _request.GetSingle(BuildPath(path));
        }

        public Response GetStreaming(string path, Action<StreamingEvent> callback)
        {
            return GetStreamingAsync(path, callback).Result;
        }

        public async Task<Response> GetStreamingAsync(string path, Action<StreamingEvent> callback)
        {
            return await _request.GetStreaming(BuildPath(path), callback);
        }

        private Uri BuildPath(string path)
        {
            return new Uri(RootUri.AbsoluteUri + path + ".json");
        }

        public void Dispose()
        {
            using (_request) { }
        }
    }
}
