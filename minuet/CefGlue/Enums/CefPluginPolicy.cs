//
// This file manually written from cef/include/internal/cef_types.h.
// C API name: cef_dom_document_type_t.
//
namespace Xilium.CefGlue
{
    using System;

    /// <summary>
    /// Cef plugin policy
    /// </summary>
    public enum CefPluginPolicy
    {
        ///
        /// Allow the content.
        ///
        Allow  = 0,

        ///
        /// Allow important content and block unimportant content based on heuristics.
        /// The user can manually load blocked content.
        ///
        DetectImportant,

        ///
        /// Block the content. The user can manually load blocked content.
        ///
        Block,

        ///
        /// Disable the content. The user cannot load disabled content.
        ///
        Disable,
    }
}