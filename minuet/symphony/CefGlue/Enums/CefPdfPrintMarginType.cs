//
// This file manually written from cef/include/internal/cef_types.h.
// C API name: cef_pdf_print_margin_type_t.
//
namespace Xilium.CefGlue
{
    ///
    /// Margin type for PDF printing.
    ///
    public enum CefPdfPrintMarginType
    {
        ///
        /// Default margins.
        ///
        PdfPrintMarginDefault,

        ///
        /// No margins.
        ///
        PdfPrintMarginNone,

        ///
        /// Minimum margins.
        ///
        PdfPrintMarginMinimum,

        ///
        /// Custom margins using the |margin_*| values from cef_pdf_print_settings_t.
        ///
        PdfPrintMarginCustom,
    }
}
