using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DipesLink.Models
{
    public class PrintingInfo
    {
        public ObservableCollection<ExpandoObject>? list { get; set; }
        public string[]? columnNames { get; set; }
    }
}
