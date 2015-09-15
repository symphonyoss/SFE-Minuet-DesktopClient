//
// This file manually written from cef/include/internal/cef_types.h.
//
namespace Xilium.CefGlue.Interop
{
    using System;
    using System.Runtime.InteropServices;

    ///
    /// Structure representing PDF print settings.
    ///
    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    internal unsafe struct cef_pdf_print_settings_t 
    {
        ///
        /// Page title to display in the header. Only used if |header_footer_enabled|
        /// is set to true (1).
        ///
        public cef_string_t header_footer_title;

        ///
        /// URL to display in the footer. Only used if |header_footer_enabled| is set
        /// to true (1).
        ///
        public cef_string_t header_footer_url;

        ///
        /// Output page size in microns. If either of these values is less than or
        /// equal to zero then the default paper size (A4) will be used.
        ///
        public int page_width;
        public int page_height;

        ///
        /// Margins in millimeters. Only used if |margin_type| is set to
        /// PDF_PRINT_MARGIN_CUSTOM.
        ///
        internal double margin_top;
        internal double margin_right;
        public double margin_bottom;
        public double margin_left;

        ///
        /// Margin type.
        ///
        public CefPdfPrintMarginType margin_type;

        ///
        /// Set to true (1) to print headers and footers or false (0) to not print
        /// headers and footers.
        ///
        public int header_footer_enabled;

        ///
        /// Set to true (1) to print the selection only or false (0) to print all.
        ///
        public int selection_only;

        ///
        /// Set to true (1) for landscape mode or false (0) for portrait mode.
        ///
        public int landscape;

        ///
        /// Set to true (1) to print background graphics or false (0) to not print
        /// background graphics.
        ///
        public int backgrounds_enabled;

        #region Alloc & Free
        private static int _sizeof;

        static cef_pdf_print_settings_t()
        {
            _sizeof = Marshal.SizeOf(typeof(cef_pdf_print_settings_t));
        }

        public static cef_pdf_print_settings_t* Alloc()
        {
            var ptr = (cef_pdf_print_settings_t*)Marshal.AllocHGlobal(_sizeof);
            *ptr = new cef_pdf_print_settings_t();
            return ptr;
        }

        public static void Free(cef_pdf_print_settings_t* ptr)
        {
            Marshal.FreeHGlobal((IntPtr)ptr);
        }
        #endregion
    }
}
