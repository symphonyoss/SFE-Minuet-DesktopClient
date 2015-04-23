using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paragon.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public class BrowserCallInfo : CallInfo, IJavaScriptPluginCallback, IJavaScriptParameterCallback
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly IJavaScriptParameterCallback _parameterCallback;
        private readonly IBrowserCallResponseHandler _responseHandler;
        private volatile bool _disposed;

        public BrowserCallInfo(
            IBrowserCallResponseHandler responseHandler,
            CefBrowser browser,
            PluginMessage pluginMessage,
            string pluginId,
            IJavaScriptParameterCallback parameterCallback)
            : base(pluginMessage, pluginId)
        {
            _responseHandler = responseHandler;
            _parameterCallback = parameterCallback;
            Browser = browser;
        }

        public CefBrowser Browser { get; private set; }

        /// <summary>
        /// Invoked by a native plugin object which sees this callback as a <see cref="JavaScriptPluginCallback" />.
        /// </summary>
        /// <param name="data">The result of the method (assuming it didn't throw an exception), or the payload of the event.</param>
        public void Invoke(params object[] data)
        {
            ThrowIfDisposed();

            try
            {
                var result = new ResultData
                {
                    DataType = ResultDataType.Array,
                    Items = (data ?? Enumerable.Empty<object>()).Select(ToResultItem).ToList(),
                    ErrorCode = 0,
                    Error = string.Empty
                };

                _responseHandler.SendFunctionResponse(this, result);
            }
            catch (Exception ex)
            {
                Logger.Error(fmt => fmt("Failed to send function response because: {0}", ex));
            }
        }

        /// <summary>
        /// A unique ID for the callback.
        /// </summary>
        public Guid Identifier
        {
            get { return RequestMessage.MessageId; }
        }

        /// <summary>
        /// Returns a callback to be supplied as a parameter to method owned by <see cref="JavaScriptPlugin" /> when the
        /// method expects to be supplied a callback to invoke itself.
        /// </summary>
        public IJavaScriptParameterCallback GetParameterCallback()
        {
            return _parameterCallback;
        }

        /// <summary>
        /// Invoked by <see cref="JavaScriptPlugin" /> when a function completes or an event fires, depending what the callback
        /// was registered with.
        /// </summary>
        /// <param name="data">The result of the method (assuming it didn't throw an exception), or the payload of the event.</param>
        /// <param name="errorCode">Provided for functions when an exception was caught.</param>
        /// <param name="error">Provided for functions when an exception was caught.</param>
        public void Invoke(object data, int errorCode, string error)
        {
            ThrowIfDisposed();

            if (IsEventListener)
            {
                SendEventFired((object[]) data);
            }
            else
            {
                OnCallComplete(data, errorCode, error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _disposed = true;

            if (disposing)
            {
                Browser.Dispose();
            }
        }

        /// <summary>
        /// Invoked when request is a function call
        /// </summary>
        /// <param name="result"></param>
        /// <param name="errorCode"></param>
        /// <param name="error"></param>
        private void OnCallComplete(object result, int errorCode, string error)
        {
            try
            {
                var resultData = new ResultData
                {
                    ErrorCode = errorCode, 
                    Error = error, 
                    DataType = ResultDataType.Scalar, 
                    Items = new List<ResultItem>()
                };

                if (result != null)
                {
                    var array = result as Array;
                    var dictionary = result as IDictionary;
                    if (array != null)
                    {
                        resultData.DataType = ResultDataType.Array;

                        var arrayLength = array.GetLength(0);
                        for (var arrayIndex = 0; arrayIndex < arrayLength; arrayIndex++)
                        {
                            var arrayItem = array.GetValue(arrayIndex);
                            resultData.Items.Add(ToResultItem(arrayItem));
                        }
                    }
                    else if (dictionary != null)
                    {
                        resultData.DataType = ResultDataType.Dictionary;

                        foreach (var key in dictionary.Keys)
                        {
                            var resultItem = ToResultItem(dictionary[key]);
                            resultItem.Name = key.ToString();
                            resultData.Items.Add(resultItem);
                        }
                    }
                    else
                    {
                        resultData.Items.Add(ToResultItem(result));
                    }
                }

                _responseHandler.SendFunctionResponse(this, resultData);
            }
            catch (Exception ex)
            {
                var errorResult = new ResultData
                {
                    DataType = ResultDataType.Scalar,
                    ErrorCode = -1,
                    Error = ex.Message
                };
                _responseHandler.SendFunctionResponse(this, errorResult);
            }
        }

        /// <summary>
        /// Invoke when request is an event listener
        /// </summary>
        /// <param name="data"></param>
        private void SendEventFired(IEnumerable<object> data)
        {
            try
            {
                var result = new ResultData
                {
                    DataType = ResultDataType.Array, 
                    Items = (data ?? Enumerable.Empty<object>()).Select(ToResultItem).ToList(), 
                    ErrorCode = 0, 
                    Error = string.Empty
                };

                _responseHandler.SendEvent(this, result);
            }
            catch (Exception ex)
            {
                Logger.Error(fmt => fmt("Failed to send event because: {0}", ex));
            }
        }

        private ResultItem ToResultItem(object item)
        {
            var resultItem = new ResultItem();
            if (item != null)
            {
                if (JavaScriptPlugin.IsDynamicPluginType(item.GetType()))
                {
                    resultItem.DynamicPlugin = _responseHandler.AddDynamicPlugin(item);
                }
                else
                {
                    resultItem.PlainData = JToken.FromObject(item, JsonSerializer.Create(ParagonJsonSerializer.Settings));
                }
            }
            return resultItem;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Attempt to use a disposed instance of " + GetType().Name);
            }
        }
    }
}