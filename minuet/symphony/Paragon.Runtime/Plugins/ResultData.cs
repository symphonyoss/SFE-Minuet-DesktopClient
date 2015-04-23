using System.Collections.Generic;

namespace Paragon.Runtime.Plugins
{
    public class ResultData
    {
        public ResultDataType DataType { get; set; }
        public List<ResultItem> Items { get; set; }
        public int ErrorCode { get; set; }
        public string Error { get; set; }
    }
}