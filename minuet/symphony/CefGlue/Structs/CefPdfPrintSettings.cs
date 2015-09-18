namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;

    /// <summary>
    /// Class representing PDF print settings.
    /// </summary>
    public sealed unsafe partial class CefPdfPrintSettings
    {
        public CefPdfPrintSettings()
        { }

        ///
        /// Page title to display in the header. Only used if |header_footer_enabled|
        /// is set to true (1).
        ///
        public string HeaderFooterTitle { get; set; }

        ///
        /// URL to display in the footer. Only used if |header_footer_enabled| is set
        /// to true (1).
        ///
        public string HeaderFooterUrl { get; set; }

        ///
        /// Output page size in microns. If either of these values is less than or
        /// equal to zero then the default paper size (A4) will be used.
        ///
        public int PageWidth { get; set; }
        public int PageHeight { get; set; }

        ///
        /// Margins in millimeters. Only used if |margin_type| is set to
        /// PDF_PRINT_MARGIN_CUSTOM.
        ///
        public double MarginTop { get; set; }
        public double MarginRight { get; set; }
        public double MarginBottom { get; set; }
        public double MarginLeft { get; set; }

        ///
        /// Margin type.
        ///
        public CefPdfPrintMarginType MarginType { get; set; }

        ///
        /// Set to true to print headers and footers or false (0) to not print
        /// headers and footers.
        ///
        public bool HeaderFooterEnabled { get; set; }

        ///
        /// Set to true to print the selection only or false (0) to print all.
        ///
        public bool SelectionOnly { get; set; }

        ///
        /// Set to true for landscape mode or false (0) for portrait mode.
        ///
        public bool Landscape { get; set; }

        ///
        /// Set to true to print background graphics or false (0) to not print
        /// background graphics.
        ///
        public bool BackgroundsEnabled { get; set; }

        internal static CefPdfPrintSettings FromNative(cef_pdf_print_settings_t* ptr)
        {
            return new CefPdfPrintSettings
                {
                    HeaderFooterTitle = cef_string_t.ToString(&ptr->header_footer_title),
                    HeaderFooterUrl = cef_string_t.ToString(&ptr->header_footer_url),
                    PageWidth = ptr->page_width,
                    PageHeight = ptr->page_height,
                    MarginTop = ptr->margin_top,
                    MarginRight = ptr->margin_right,
                    MarginBottom = ptr->margin_bottom,
                    MarginLeft = ptr->margin_left,
                    MarginType = ptr->margin_type,
                    HeaderFooterEnabled = ptr->header_footer_enabled != 0,
                    SelectionOnly = ptr->selection_only != 0,
                    Landscape = ptr->landscape != 0,
                    BackgroundsEnabled = ptr->backgrounds_enabled != 0,
                };
        }

        internal cef_pdf_print_settings_t* ToNative()
        {
            var ptr = cef_pdf_print_settings_t.Alloc();

            cef_string_t.Copy(HeaderFooterTitle, &ptr->header_footer_title);
            cef_string_t.Copy(HeaderFooterUrl, &ptr->header_footer_url);
            ptr->page_width = PageWidth;
            ptr->page_height = PageHeight;
            ptr->margin_top = MarginTop;
            ptr->margin_right = MarginRight;
            ptr->margin_bottom = MarginBottom;
            ptr->margin_left = MarginLeft;
            ptr->margin_type = MarginType;
            ptr->header_footer_enabled = HeaderFooterEnabled ? 1 : 0;
            ptr->selection_only = SelectionOnly ? 1 : 0;
            ptr->landscape = Landscape ? 1 : 0;
            ptr->backgrounds_enabled = BackgroundsEnabled ? 1 : 0;

            return ptr;
        }

        internal static void Free(cef_pdf_print_settings_t* ptr)
        {
            cef_pdf_print_settings_t.Free((cef_pdf_print_settings_t*)ptr);
        }    
    }
}
