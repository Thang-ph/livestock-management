using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos
{
    public class ExternalLoginResult
    {
        public bool Succeeded { get; set; }
        public string Token { get; set; }
        public string Error { get; set; }
    }
}
