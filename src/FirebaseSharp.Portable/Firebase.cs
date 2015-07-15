using System;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Network;

namespace FirebaseSharp.Portable
{
    /// <summary>
    /// Class to read and write to a Firebase database
    /// 
    /// Firebase fb = new Firebase("http://path.firebaseio.com");
    /// string jsonResponse = await fb.Get("/path");
    /// </summary>
    public sealed class Firebase : IDisposable
    {
        private readonly Request _request;

        /// <summary>
        /// Creates a firebase instance at the specified URI.
        /// </summary>
        /// <param name="rootUri">The absolute URI of the Firebase root</param>
        /// <param name="authToken">The optional Firebase authentication token</param>
        public Firebase(string rootUri, string authToken = null)
            : this(new Uri(rootUri, UriKind.Absolute), authToken)
        {            
        }

        /// <summary>
        /// Creates a firebase instance at the specified URI.
        /// </summary>
        /// <param name="rootUri">The absolute URI of the Firebase root</param>
        /// <param name="authToken">The optional Firebase authentication token</param>
        public Firebase(Uri rootUri, string authToken = null)
        {
            if (rootUri == null)
            {
                throw new ArgumentNullException("rootUri");
            }

            if (!rootUri.IsAbsoluteUri)
            {
                throw new ArgumentException("The root URI must be an absolute URI", "rootUri");
            }

            _request = new Request(new FirebaseHttpClient(rootUri), authToken);
        }

        /// <summary>
        /// The root URI of this firebase instance
        /// </summary>
        public Uri RootUri
        {
            get { return _request.RootUri; }
        }

        [Obsolete("Use PostAsync instead.  This method will be removed in the next version.")]
        public string Post(string path, string payload)
        {
            return PostAsync(path, payload).Result;
        }

        /// <summary>
        /// Performs an HTTP POST (list push) to the specified path with the specified payload.
        /// See: https://www.firebase.com/docs/rest/api/
        /// </summary>
        /// <param name="path">The firebase path (relative to the base URI)</param>
        /// <param name="payload">The payload to post</param>
        /// <returns>The child name of the new data that was added</returns>
        public async Task<string> PostAsync(string path, string payload)
        {
            return await _request.Post(path, payload).ConfigureAwait(false);
        }

        [Obsolete("Use PutAsync instead.  This method will be removed in the next version.")]
        public string Put(string path, string payload)
        {
            return PutAsync(path, payload).Result;
        }

        /// <summary>
        /// Performs an HTTP PUT to the specified path with the specified payload.
        /// See: https://www.firebase.com/docs/rest/api/
        /// </summary>
        /// <param name="path">The firebase path (relative to the base URI)</param>
        /// <param name="payload">The payload to PUT</param>
        /// <returns>The JSON data that was written</returns>
        public async Task<string> PutAsync(string path, string payload)
        {
            return await _request.Put(path, payload).ConfigureAwait(false);
        }

        [Obsolete("Use PatchAsync instead.  This method will be removed in the next version.")]
        public string Patch(string path, string payload)
        {
            return PatchAsync(path, payload).Result;
        }

        /// <summary>
        /// Performs an HTTP PATCH to the specified path with the specified payload.
        /// See: https://www.firebase.com/docs/rest/api/
        /// </summary>
        /// <param name="path">The firebase path (relative to the base URI)</param>
        /// <param name="payload">The payload to PATCH</param>
        /// <returns>The JSON data that was written</returns>
        public async Task<string> PatchAsync(string path, string payload)
        {
            return await _request.Patch(path, payload).ConfigureAwait(false);
        }

        [Obsolete("Use DeleteAsync instead.  This method will be removed in the next version.")]
        public void Delete(string path)
        {
            DeleteAsync(path).Wait();
        }

        /// <summary>
        /// Performs an HTTP DELETE to the specified path.
        /// See: https://www.firebase.com/docs/rest/api/
        /// </summary>
        /// <param name="path">The firebase path (relative to the base URI)</param>
        /// <returns>A void task</returns>
        public async Task DeleteAsync(string path)
        {
            await _request.Delete(path).ConfigureAwait(false);
        }

        [Obsolete("Use GetAsync instead.  This method will be removed in the next version.")]
        public string Get(string path)
        {
            return GetAsync(path).Result;
        }

        /// <summary>
        /// Performs an HTTP GET at the specified path.
        /// See: https://www.firebase.com/docs/rest/api/
        /// </summary>
        /// <param name="path">The firebase path (relative to the base URI)</param>
        /// <returns>The JSON data at that location</returns>
        public async Task<string> GetAsync(string path)
        {
            return await _request.GetSingle(path).ConfigureAwait(false);
        }

        [Obsolete("Use GetStreamingAsync instead.  This method will be removed in the next version.")]
        public Response GetStreaming(string path, 
            ValueAddedEventHandler added = null,
            ValueChangedEventHandler changed = null,
            ValueRemovedEventHandler removed = null)
        {
            return GetStreamingAsync(path, added, changed, removed).Result;
        }

        /// <summary>
        /// Performs an streaming HTTP GET at the specified path.
        /// See: https://www.firebase.com/docs/rest/api/
        /// </summary>
        /// <param name="path">The firebase path (relative to the base URI)</param>
        /// <param name="added">Callback fired when data is added (put event)</param>
        /// <param name="changed">Callback fired when data is changed (patch event)</param>
        /// <param name="removed">Callback fired when data is removed (put with null data)</param>
        /// <returns>A Response object</returns>
        public async Task<Response> GetStreamingAsync(string path,
            ValueAddedEventHandler added = null,
            ValueChangedEventHandler changed = null,
            ValueRemovedEventHandler removed = null)
        {
            return await _request.GetStreaming(path, added, changed, removed).ConfigureAwait(false);
        }

        public void Dispose()
        {
            using (_request) { }
        }
    }
}
