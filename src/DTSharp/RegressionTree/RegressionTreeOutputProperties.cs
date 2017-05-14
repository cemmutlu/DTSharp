using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTSharp
{
    public class RegressionTreeOutputProperties: OutputProperties
    {
        public double Mean { get; set; }
        public double Maximum { get; set; }
        public double Minimum { get; set; }
    }
}
