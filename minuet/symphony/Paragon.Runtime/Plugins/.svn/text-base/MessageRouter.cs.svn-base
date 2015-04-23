using System;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// The base for both browser and render side message routers. 
    /// Provides sending and receiving of <see cref="PluginMessage"/> across the CEF boundary, 
    /// and receiving of <see cref="PluginMessage"/> from an external plugin process.
    /// </summary>
    public abstract class MessageRouter : IMessageRouter
    {
        private readonly CefProcessId _remotePluginProcess;
        private bool _disposed;

        protected MessageRouter(CefProcessId pluginProcess)
        {
            _remotePluginProcess = pluginProcess == CefProcessId.Browser ? CefProcessId.Renderer : CefProcessId.Browser;
        }

        public bool ProcessCefMessage(CefBrowser browser, CefProcessMessage message)
        {
            var args = message.Arguments;
            var messageName = message.Name;
            if (messageName == CallInfo.Call && args.Count >= 10)
            {
                var pluginMessage = new PluginMessage
                {
                    MessageId = new Guid(args.GetString(0)), 
                    MessageType = (PluginMessageType) args.GetInt(1), 
                    PluginId = args.GetString(2), 
                    MemberName = args.GetString(3), 
                    Data = args.GetString(4), 
                    BrowserId = args.GetInt(5), 
                    ContextId = args.GetInt(6), 
                    FrameId = (args.GetInt(8) << 32) | ((long) (args.GetInt(7))), 
                    V8CallbackId = new Guid(args.GetString(9))
                };

                ReceiveMessage(browser, pluginMessage);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected PluginMessage DeserializeProcessMessage(CefListValue args)
        {
            if (args.Count < 10)
            {
                throw new ArgumentNullException("args");
            }

            return new PluginMessage
            {
                MessageId = new Guid(args.GetString(0)), 
                MessageType = (PluginMessageType) args.GetInt(1), 
                PluginId = args.GetString(2),
                MemberName = args.GetString(3),
                Data = args.GetString(4),
                BrowserId = args.GetInt(5),
                ContextId = args.GetInt(6),
                FrameId = (args.GetInt(8) << 32) | ((long) (args.GetInt(7))),
                V8CallbackId = new Guid(args.GetString(9))
            };
        }

        protected void SerializeProcessMessage(PluginMessage pluginMessage, CefListValue args)
        {
            if (pluginMessage == null)
            {
                throw new ArgumentNullException("pluginMessage");
            }
            args.SetString(0, pluginMessage.MessageId.ToString());
            args.SetInt(1, (int) pluginMessage.MessageType);
            args.SetString(2, pluginMessage.PluginId);
            args.SetString(3, pluginMessage.MemberName);
            args.SetString(4, pluginMessage.Data);
            args.SetInt(5, pluginMessage.BrowserId);
            args.SetInt(6, pluginMessage.ContextId);
            args.SetInt(7, (int) (pluginMessage.FrameId & uint.MaxValue)); // Lower 32 bits
            args.SetInt(8, (int) (pluginMessage.FrameId >> 32)); // Upper 32 bits
            args.SetString(9, pluginMessage.V8CallbackId.ToString());
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Receive a plugin message from a CEF or external (MAF) process.
        /// Will be on the CEF main thread for messages received via CEF, will be on thread pool for external plugin processes.
        /// </summary>
        /// <param name="browser">
        /// Only populated for a message received across the CEF boundary. If null it means the message has been received from an external plugin process.
        /// </param>
        /// <param name="pluginMessage">
        /// The message to process.
        /// </param>
        protected abstract void ReceiveMessage(CefBrowser browser, PluginMessage pluginMessage);

        /// <summary>
        /// Send a message across the CEF boundary.
        /// </summary>
        /// <param name="browser">The browser instance to send the mesage to.</param>
        /// <param name="pluginMessage">The message to send.</param>
        protected void SendMessage(CefBrowser browser, PluginMessage pluginMessage)
        {
            if (browser == null)
            {
                throw new ArgumentNullException("browser");
            }
            var message = CefProcessMessage.Create(CallInfo.Call);
            SerializeProcessMessage(pluginMessage, message.Arguments);
            browser.SendProcessMessage(_remotePluginProcess, message);
        }
    }
}