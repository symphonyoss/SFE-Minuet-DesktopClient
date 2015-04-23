using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using Paragon.Plugins;
using Paragon.Runtime.Annotations;
using Paragon.Runtime.Kernel.Windowing;
using Xilium.CefGlue;

namespace Paragon.Runtime.Kernel.Plugins
{
    [JavaScriptPlugin(Name = "paragon.contextMenu")]
    public class ParagonContextMenuPlugin : ParagonPlugin
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private static readonly object Lock = new object();

        public static readonly ContextMenuProperties DevToolsMenuItem = new ContextMenuProperties
        {
            Id = (int) CefMenuId.UserFirst + 1,
            Type = "normal",
            Title = "Inspect Element",
            Contexts = new List<string> {"all"}
        };

        public static readonly ContextMenuProperties ReloadMenuItem = new ContextMenuProperties
        {
            Id = (int) CefMenuId.UserFirst + 2,
            Type = "normal",
            Title = "Reload",
            Contexts = new List<string> {"page", "frame"}
        };

        public static readonly ContextMenuProperties PrintMenuItem = new ContextMenuProperties
        {
            Id = (int) CefMenuId.Print,
            Type = "normal",
            Title = "Print...",
            Contexts = new List<string> {"page", "frame"}
        };

        public static readonly ContextMenuProperties ViewSourceMenuItem = new ContextMenuProperties
        {
            Id = (int) CefMenuId.ViewSource,
            Type = "normal",
            Title = "View Source",
            Contexts = new List<string> {"page", "frame"}
        };

        private readonly List<ContextMenuProperties> _items = new List<ContextMenuProperties>();
        private int _id;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Application.WindowManager.CreatedWindow += OnWindowCreated;
        }

        private void OnWindowCreated(IApplicationWindow window, bool arg2)
        {
            if (window != null)
            {
                var w = (IApplicationWindowEx) window;
                w.Browser.BeforeContextMenu += OnBeforeContextMenu;
                w.Browser.ContextMenuCommand += OnContextMenuCommand;
            }
        }

        private void OnContextMenuCommand(object sender, ContextMenuCommandEventArgs e)
        {
            var browser = sender as ICefWebBrowser;
            if (browser != null)
            {
                var window = Application.FindWindow(browser.Identifier) as IApplicationWindowEx;
                if (window != null)
                {
                    if (e.Command == DevToolsMenuItem.Id)
                    {
                        window.ShowDeveloperTools(new Point(e.State.X, e.State.Y));
                    }
                    else if (e.Command == ReloadMenuItem.Id)
                    {
                        window.RefreshWindow();
                    }
                    else
                    {
                        RaiseClicked(e.Command, e.State);
                    }
                }
            }
        }

        private void OnBeforeContextMenu(object sender, ContextMenuEventArgs ea)
        {
            ea.Model.Remove((int) CefMenuId.Back);
            ea.Model.Remove((int) CefMenuId.Forward);
            ea.Model.Remove((int) CefMenuId.ViewSource);
            ea.Model.Remove((int) CefMenuId.Print);

            if (ea.Model.GetItemTypeAt(0) == CefMenuItemType.Separator)
            {
                ea.Model.RemoveAt(0);
            }

            var model = ea.Model;
            var prevCount = model.Count;
            Populate(ref model, ea.State);

            if (model.Count > prevCount)
            {
                model.InsertSeparatorAt(prevCount);
            }

            if (model.Count > 0)
            {
                model.AddSeparator();
            }

            if (Application.Metadata.Environment != ApplicationEnvironment.Production)
            {
                ReloadMenuItem.Populate(ref model, ea.State);
                ViewSourceMenuItem.Populate(ref model, ea.State);
                DevToolsMenuItem.Populate(ref model, ea.State);
            }

            PrintMenuItem.Populate(ref model, ea.State);

            ea.Handled = true;
        }

        [JavaScriptPluginMember(Name = "onclick"), UsedImplicitly]
        public event JavaScriptPluginCallback OnClick;

        /// <summary>
        /// Creates a new context menu item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [JavaScriptPluginMember, UsedImplicitly]
        public int Create(ContextMenuProperties item)
        {
            Logger.Debug("Creating context menu item");
            if (item.Id >= 0 && FindItemById(item.Id) != null)
            {
                throw new Exception(string.Format("An item with the id {0} already exists", item.Id));
            }

            if (item.Id< 0)
            {
                item.Id = ++_id;
            }

            if (item.Contexts == null)
            {
                item.Contexts = new List<string> {"page"};
            }

            if (string.IsNullOrEmpty(item.Title))
            {
                throw new Exception("Title is required");
            }

            ContextMenuProperties parent = null;
            if (item.ParentId >= 0)
            {
                parent = FindItemById(item.ParentId);
                if (parent == null)
                {
                    throw new Exception(string.Format("Parent item with id {0} does not exists", item.ParentId));
                }
            }

            if (parent != null)
            {
                if (parent.ChildItems == null)
                {
                    parent.ChildItems = new List<ContextMenuProperties>();
                }

                parent.ChildItems.Add(item);
            }
            else
            {
                lock (Lock)
                {
                    _items.Add(item);
                }
            }
            return item.Id;
        }

        /// <summary>
        /// Creates a context menu item
        /// </summary>
        /// <param name="id">id of the item</param>
        /// <param name="newProps"></param>
        [JavaScriptPluginMember, UsedImplicitly]
        public void Update(int id, UpdateContextMenuProperties newProps)
        {
            Logger.Debug("Updating context menu item");
            if (id >= 0)
            {
                var item = FindItemById(id);
                if (item == null)
                {
                    throw new Exception(string.Format("An item with the id {0} does not exist", id));
                }
                if (!string.IsNullOrEmpty(newProps.Type))
                {
                    item.Type = newProps.Type;
                }
                if (!string.IsNullOrEmpty(newProps.Title))
                {
                    item.Title = newProps.Title;
                }
                if (!string.IsNullOrEmpty(newProps.Title))
                {
                    item.Title = newProps.Title;
                }
                if (item.Type.Equals("radio", StringComparison.InvariantCultureIgnoreCase) ||
                    item.Type.Equals("checkbox", StringComparison.InvariantCultureIgnoreCase))
                {
                    item.Checked = newProps.Checked;
                }
                // TODO : finish
            }
            throw new ArgumentException("invalid id");
        }

        /// <summary>
        /// Removes a context menu item
        /// </summary>
        /// <param name="id">id of the item</param>
        [JavaScriptPluginMember, UsedImplicitly]
        public void Remove(int id)
        {
            Logger.Debug(string.Format("Updating context menu item : {0}", id));
            if (id >= 0)
            {
                var item = FindItemById(id);
                if (item == null)
                {
                    throw new Exception(string.Format("An item with the id {0} does not exist", id));
                }
                if (item.ParentId >= 0)
                {
                    var parent = FindItemById(item.ParentId);
                    if (parent != null)
                    {
                        parent.ChildItems.Remove(item);
                    }
                }
                else
                {
                    _items.Remove(item);
                }
                return;
            }
            throw new ArgumentException("invalid id");
        }

        /// <summary>
        /// Removes all items from the context menu
        /// </summary>
        [JavaScriptPluginMember, UsedImplicitly]
        public void RemoveAll()
        {
            Logger.Debug("Clearing context menu items");
            _items.Clear();
        }

        public void RaiseClicked(int id, CefContextMenuParams context)
        {
            if (OnClick == null)
            {
                return;
            }

            var item = FindItemById(id);
            if (item == null)
            {
                return;
            }

            if (item.Id != id)
            {
                return;
            }

            var clickInfo = new ContextMenuClickInfo
            {
                MenuItemId = id,
                ParentMenuItemId = item.ParentId,
                MediaType = context.MediaType.ToString(),
                LinkUrl = context.LinkUrl,
                SrcUrl = context.SourceUrl,
                PageUrl = context.PageUrl,
                FrameUrl = context.FrameUrl,
                SelectionText = context.SelectionText,
                Editable = context.IsEditable,
                WasChecked = item.Checked,
                Checked = item.Checked
            };

            if (item.Type == "radio" || item.Type == "checkbox")
            {
                item.Checked = !item.Checked;
                clickInfo.Checked = item.Checked;
            }

            OnClick(clickInfo);
        }

        public void Populate(ref CefMenuModel model, CefContextMenuParams context)
        {
            var menuModel = model;
            lock (Lock)
            {
                _items.ForEach(item => item.Populate(ref menuModel, context));
            }
        }

        private ContextMenuProperties FindItemById(int id)
        {
            ContextMenuProperties item = null;
            lock (_items)
            {
                VisitItems(_items, i =>
                {
                    if (i.Id == id)
                    {
                        item = i;
                        return true;
                    }
                    return false;
                });
            }
            return item;
        }

        private bool VisitItems(IEnumerable<ContextMenuProperties> items, Predicate<ContextMenuProperties> shouldStop)
        {
            return items != null && items.Select(item => VisitItem(item, shouldStop)).Any(done => done);
        }

        private bool VisitItem(ContextMenuProperties item, Predicate<ContextMenuProperties> shouldStop)
        {
            if (shouldStop(item))
            {
                return true;
            }
            if (item.ChildItems != null && item.ChildItems.Count > 0)
            {
                return VisitItems(item.ChildItems, shouldStop);
            }
            return false;
        }
    }

    public class ContextMenuProperties
    {
        public ContextMenuProperties()
        {
            Type = "normal";
            Id = -1;
            GroupId = -1;
            Title = string.Empty;
            Checked = false;
            ParentId = -1;
            Enabled = true;
        }

        /// <summary>
        /// ChildItems
        /// </summary>
        [JsonIgnore]
        public List<ContextMenuProperties> ChildItems { get; set; }

        /// <summary>
        ///  The type of menu item. Enum of "normal", "checkbox", "radio", or "separator". Defaults to 'normal' if not specified. 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///  The unique ID to assign to this item. Cannot be the same as another ID. Optional
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///  The unique ID to assign to this item. Cannot be the same as another ID. Optional
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        ///  Title. Optional. The text to be displayed in the item; this is required unless type is 'separator'. When the context is 'selection', you can use %s within the string to show the selected text. For example, if this parameter's value is "Translate '%s' to Pig Latin" and the user selects the word "cool", the context menu item for the selection is "Translate 'cool' to Pig Latin". 
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///  The initial state of a checkbox or radio item: true for selected and false for unselected. Only one radio item can be selected at a time in a given group of radio items. Optional. 
        /// </summary>
        public bool Checked { get; set; }

        /// <summary>
        ///  List of contexts this menu item will appear in. Defaults to ['page'] if not specified. Specifying ['all'] is equivalent to the combination of all other contexts except for 'launcher'. The 'launcher' context is only supported by apps and is used to add menu items to the context menu that appears when clicking on the app icon in the launcher/taskbar/dock/etc. Different platforms might put limitations on what is actually supported in a launcher context menu. 
        ///  Array of enum of "all", "page", "frame", "selection", "link", "editable", "image", "video", "audio". 
        ///  Optional. 
        /// </summary>
        public List<string> Contexts { get; set; }

        /// <summary>
        /// Id of the parent item (if any)
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        ///  Lets you restrict the item to apply only to documents whose URL matches one of the given patterns. (This applies to frames as well.) For details on the format of a pattern, see Match Patterns. 
        /// </summary>
        public List<string> DocumentUrlPatterns { get; set; }

        /// <summary>
        ///  Whether this context menu item is enabled or disabled. Defaults to true.
        /// </summary>
        public bool Enabled { get; set; }

        public void Populate(ref CefMenuModel model, CefContextMenuParams context)
        {
            if (ShouldPopulate(context))
            {
                if (ChildItems != null && ChildItems.Count > 0)
                {
                    var childMenu = model.AddSubMenu(Id, Title);
                    AddChildItems(ref childMenu, context);
                }
                else
                {
                    switch (Type)
                    {
                        case "radio":
                            model.AddRadioItem(Id, Title, GroupId <= 0 ? 1 : GroupId);
                            if (Checked)
                            {
                                model.SetChecked(Id, true);
                            }
                            break;
                        case "checkbox":
                            model.AddCheckItem(Id, Title);
                            if (Checked)
                            {
                                model.SetChecked(Id, true);
                            }
                            break;
                        case "separator":
                            model.AddSeparator();
                            break;
                        default:
                            model.AddItem(Id, Title);
                            break;
                    }
                }
            }
        }

        private void AddChildItems(ref CefMenuModel model, CefContextMenuParams context)
        {
            foreach (var item in ChildItems)
            {
                item.Populate(ref model, context);
            }
        }

        private bool ShouldPopulate(CefContextMenuParams context)
        {
            // First compare item contexts with current context
            var contexts = context.ContextMenuType;

            if (Contexts.Contains("all") ||
                ((contexts & CefContextMenuTypeFlags.Editable) == CefContextMenuTypeFlags.Editable &&
                 Contexts.Contains("editable")) ||
                ((contexts & CefContextMenuTypeFlags.Selection) == CefContextMenuTypeFlags.Selection &&
                 Contexts.Contains("selection")))
            {
                return true;
            }

            if (contexts == CefContextMenuTypeFlags.Page && Contexts.Contains("page"))
            {
                if (DocumentUrlPatterns == null || DocumentUrlPatterns.Count == 0 || MatchUrlPattern(DocumentUrlPatterns, context.PageUrl))
                {
                    return true;
                }
            }

            if ((contexts & CefContextMenuTypeFlags.Frame) == CefContextMenuTypeFlags.Frame &&
                Contexts.Contains("frame"))
            {
                if (DocumentUrlPatterns == null || DocumentUrlPatterns.Count == 0 || MatchUrlPattern(DocumentUrlPatterns, context.FrameUrl))
                {
                    return true;
                }
            }

            if ((contexts & CefContextMenuTypeFlags.Link) == CefContextMenuTypeFlags.Link &&
                Contexts.Contains("link"))
            {
                if (DocumentUrlPatterns == null || DocumentUrlPatterns.Count == 0 || MatchUrlPattern(DocumentUrlPatterns, context.LinkUrl))
                {
                    return true;
                }
            }

            return (contexts & CefContextMenuTypeFlags.Media) == CefContextMenuTypeFlags.Media &&
                   (Contexts.Contains("image") || Contexts.Contains("video") || Contexts.Contains("audio"));
        }

        private bool MatchUrlPattern(IEnumerable<string> patterns, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return true;
            }

            // TODO : Use the information at https://developer.chrome.com/apps/match_patterns to finish this
            return true;
        }
    }

    public class UpdateContextMenuProperties
    {
        public UpdateContextMenuProperties()
        {
            Type = "normal";
            Title = string.Empty;
            Checked = false;
            Enabled = true;
        }

        /// <summary>
        ///  The type of menu item. Enum of "normal", "checkbox", "radio", or "separator". Defaults to 'normal' if not specified. 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///  Title. Optional. The text to be displayed in the item; this is required unless type is 'separator'. When the context is 'selection', you can use %s within the string to show the selected text. For example, if this parameter's value is "Translate '%s' to Pig Latin" and the user selects the word "cool", the context menu item for the selection is "Translate 'cool' to Pig Latin". 
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///  The initial state of a checkbox or radio item: true for selected and false for unselected. Only one radio item can be selected at a time in a given group of radio items. Optional. 
        /// </summary>
        public bool Checked { get; set; }

        /// <summary>
        ///  Whether this context menu item is enabled or disabled. Defaults to true.
        /// </summary>
        public bool Enabled { get; set; }
    }

    public class ContextMenuClickInfo
    {
        /// <summary>
        ///  The ID of the menu item that was clicked. 
        /// </summary>
        public int MenuItemId { get; set; }

        /// <summary>
        ///  The parent ID, if any, for the item clicked. 
        /// </summary>
        public int ParentMenuItemId { get; set; }

        /// <summary>
        ///  One of 'image', 'video', or 'audio' if the context menu was activated on one of these types of elements. 
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        ///  If the element is a link, the URL it points to. 
        /// </summary>
        public string LinkUrl { get; set; }

        /// <summary>
        ///  Will be present for elements with a 'src' URL. 
        /// </summary>
        public string SrcUrl { get; set; }

        /// <summary>
        ///  The URL of the page where the menu item was clicked. This property is not set if the click occured in a context where there is no current page, such as in a launcher context menu. 
        /// </summary>
        public string PageUrl { get; set; }

        /// <summary>
        ///  The URL of the frame of the element where the context menu was clicked, if it was in a frame.
        /// </summary>
        public string FrameUrl { get; set; }

        /// <summary>
        ///  The text for the context selection, if any.
        /// </summary>
        public string SelectionText { get; set; }

        /// <summary>
        ///  A flag indicating whether the element is editable (text input, textarea, etc.). 
        /// </summary>
        public bool Editable { get; set; }

        /// <summary>
        ///  A flag indicating the state of a checkbox or radio item before it was clicked.
        /// </summary>
        public bool WasChecked { get; set; }

        /// <summary>
        ///  A flag indicating the state of a checkbox or radio item after it is clicked.
        /// </summary>
        public bool Checked { get; set; }
    }
}