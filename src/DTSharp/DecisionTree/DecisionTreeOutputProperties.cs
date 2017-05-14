using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTSharp.DecisionTree
{
    public class DecisionTreeOutputProperties: OutputProperties
    {
        public Dictionary<object,int> OutputDistribution { get; set; }
    }
}
