//
// This file manually written from cef/include/internal/cef_types.h.
// C API name: cef_return_value_t.
//
namespace Xilium.CefGlue
{
    public enum CefReturnValue
    {
        ///
        // Cancel immediately.
        ///
        Cancel = 0,

        ///
        // Continue immediately.
        ///
        Continue,

        ///
        // Continue asynchronously (usually via a callback).
        ///
        ContinueAsync,
    }
}
