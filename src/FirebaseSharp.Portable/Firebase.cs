using System;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable
{
    public sealed class Firebase : IDisposable
    {
        private readonly Request _request;

        public Firebase(string rootUri, string authToken = null)
            : this(new Uri(rootUri), authToken)
        {            
        }

        public Firebase(Uri rootUri, string authToken = null)
        {
            if (rootUri == null)
            {
                throw new ArgumentNullException("rootUri");
            }

            _request = new Request(rootUri, authToken);
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
            return await _request.Post(path, payload);
        }

        public string Put(string path, string payload)
        {
            return PutAsync(path, payload).Result;
        }

        public async Task<string> PutAsync(string path, string payload)
        {
            return await _request.Put(path, payload);
        }

        public string Patch(string path, string payload)
        {
            return PatchAsync(path, payload).Result;
        }

        public async Task<string> PatchAsync(string path, string payload)
        {
            return await _request.Patch(path, payload);
        }

        public void Delete(string path)
        {
            DeleteAsync(path).Wait();
        }
        public async Task DeleteAsync(string path)
        {
            await _request.Delete(path);
        }

        public string Get(string path)
        {
            return GetAsync(path).Result;
        }

        public async Task<string> GetAsync(string path)
        {
            return await _request.GetSingle(path);
        }

        public Response GetStreaming(string path, 
            ValueAddedEventHandler added = null,
            ValueChangedEventHandler changed = null,
            ValueRemovedEventHandler removed = null)
        {
            return GetStreamingAsync(path, added, changed, removed).Result;
        }

        public async Task<Response> GetStreamingAsync(string path,
            ValueAddedEventHandler added = null,
            ValueChangedEventHandler changed = null,
            ValueRemovedEventHandler removed = null)
        {
            return await _request.GetStreaming(path, added, changed, removed);
        }

        public void Dispose()
        {
            using (_request) { }
        }
    }
}
