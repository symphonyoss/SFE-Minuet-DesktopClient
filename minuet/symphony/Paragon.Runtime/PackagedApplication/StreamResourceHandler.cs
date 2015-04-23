using System.Collections.Specialized;
using System.IO;
using System.Net;
using Xilium.CefGlue;

namespace Paragon.Runtime.PackagedApplication
{
    public sealed class StreamResourceHandler : CefResourceHandler
    {
        private readonly string _mimeType;
        private Stream _resourceStream;

        public StreamResourceHandler(string mimeType, Stream resourceStream)
        {
            _mimeType = mimeType;
            _resourceStream = resourceStream;
        }

        protected override bool CanGetCookie(CefCookie cookie)
        {
            return true;
        }

        protected override bool CanSetCookie(CefCookie cookie)
        {
            return false;
        }

        protected override bool ProcessRequest(CefRequest request, CefCallback callback)
        {
            if (callback != null)
            {
                callback.Continue();
            }
            return true;
        }

        protected override void GetResponseHeaders(CefResponse response, out long responseLength, out string redirectUrl)
        {
            // Add the "no-cache" directive to make sure that chrome doesn't cache this resource
            var headers = new NameValueCollection {{"Cache-Control", "no-cache"}, {"Access-Control-Allow-Origin", "*"}};
            response.SetHeaderMap(headers);
            response.MimeType = _mimeType;
            if (_resourceStream == null)
            {
                response.Status = (int) HttpStatusCode.NotFound;
                response.StatusText = "Not found";
                responseLength = -1;
                redirectUrl = null;
            }
            else
            {
                response.Status = (int) HttpStatusCode.OK;
                response.StatusText = "OK";
                responseLength = _resourceStream.Length;
                redirectUrl = null;
            }
        }

        protected override bool ReadResponse(Stream response, int bytesToRead, out int bytesRead, CefCallback callback)
        {
            bytesRead = 0;
            if (_resourceStream != null)
            {
                var buffer = new byte[bytesToRead];
                bytesRead = _resourceStream.Read(buffer, 0, bytesToRead);
                if (bytesRead > 0)
                {
                    response.Write(buffer, 0, bytesRead);
                }
                if (bytesRead == bytesToRead && callback != null)
                {
                    callback.Continue();
                }
            }
            return bytesRead == bytesToRead;
        }

        protected override void Cancel()
        {
            // Required method override.
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (_resourceStream != null)
                {
                    _resourceStream.Close();
                }
                _resourceStream = null;
            }
        }
    }
}