using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.ConfigModels
{
    public static class SlugId
    {
        public static string New()
        {
            return Guid.NewGuid().ToByteArray().ToBase36String();
        }
    }
}
