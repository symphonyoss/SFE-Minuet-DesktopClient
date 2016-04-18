//
// This file manually written from cef/include/internal/cef_types.h.
// C API name: cef_dom_document_type_t.
//
namespace Xilium.CefGlue
{
    using System;

    /// <summary>
    /// Cef referer policy
    /// </summary>
    public enum CefReferrerPolicy
    {
        ///
        /// Always send the complete Referrer value.
        ///
        Always,

        ///
        /// Use the default policy. This is REFERRER_POLICY_ORIGIN_WHEN_CROSS_ORIGIN
        /// when the `--reduced-referrer-granularity` command-line flag is specified
        /// and REFERRER_POLICY_NO_REFERRER_WHEN_DOWNGRADE otherwise.
        ///
        ///
        Default,

        ///
        /// When navigating from HTTPS to HTTP do not send the Referrer value.
        /// Otherwise, send the complete Referrer value.
        ///
        NoReferrerWhenDowngrade,

        ///
        /// Never send the Referrer value.
        ///
        Never,

        ///
        /// Only send the origin component of the Referrer value.
        ///
        Origin,

        ///
        /// When navigating cross-origin only send the origin component of the Referrer
        /// value. Otherwise, send the complete Referrer value.
        ///
        OriginWhenCrossOigin,
    }
}