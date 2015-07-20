using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Filters
{
    interface IFilterContext
    {
        string FilterColumn { get; set; }
    }
    class FilterContext : IFilterContext
    {
        public string FilterColumn { get; set; }
    }
}
