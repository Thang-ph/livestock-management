using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.ConfigModels
{
    public class ResponseModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class ResponseListModel<T>
    {
        public int Total { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}
