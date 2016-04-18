//
// This file manually written from cef/include/internal/cef_types.h.
// C API name: cef_thread_id_t.
//
namespace Xilium.CefGlue
{
    /// <summary>
    /// Existing thread IDs.
    /// </summary>
    public enum CefJsonWriterOptions
    {
        ///
        // Default behavior.
        ///
        Default = 0,

        ///
        // This option instructs the writer that if a Binary value is encountered,
        // the value (and key if within a dictionary) will be omitted from the
        // output, and success will be returned. Otherwise, if a binary value is
        // encountered, failure will be returned.
        ///
        OmitBinaryValues = 1 << 0,

        ///
        // This option instructs the writer to write doubles that have no fractional
        // part as a normal integer (i.e., without using exponential notation
        // or appending a '.0') as long as the value is within the range of a
        // 64-bit int.
        ///
        OmitDoubleTypePreservation = 1 << 1,

        ///
        // Return a slightly nicer formatted json string (pads with whitespace to
        // help with readability).
        ///
        PrettyPrint = 1 << 2,

    }
}
