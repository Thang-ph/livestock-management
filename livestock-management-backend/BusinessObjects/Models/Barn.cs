using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Barn : BaseEntity
    {
        public string Name { get; set; }
        public string Address { get; set; }

        public virtual ICollection<Livestock> Livestocks { get; set; }
        public virtual ICollection<BatchImport> BatchImports { get; set; }
        public virtual ICollection<BatchExport> BatchExports { get; set; }
    }
}
