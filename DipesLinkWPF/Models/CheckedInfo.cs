using SharedProgram.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DipesLink.Models
{
    public class CheckedInfo
    {
        public ObservableCollection<CheckedResultModel>? list { get; set; }
        public string[]? columnNames { get; set; }

        public List<string[]>? RawList { get; set; }
        public List<PODModel>? PodFormat { get; set; }
        public JobOverview CurrentJob { get; internal set; }
    }
}
