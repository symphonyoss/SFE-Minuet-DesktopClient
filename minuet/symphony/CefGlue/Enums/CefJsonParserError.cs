//
// This file manually written from cef/include/internal/cef_types.h.
// C API name: cef_json_parser_error_t.
//
namespace Xilium.CefGlue
{
    using System;

    /// <summary>
    /// Error codes that can be returned from CefParseJSONAndReturnError.
    /// </summary>
    public enum CefJsonParserError
    {
        JsonNoError = 0,
        JsonInvalidEscape,
        JsonSyntaxError,
        JsonUnexpectedToken,
        JsonTrailingComma,
        JsonTooMuchNesting,
        JsonUnexpectedDataAfterRoot,
        JsonUnexpectedEncoding,
        JsonUnquotedDictionaryKey,
        JsonParseErrorCount
    }
}
