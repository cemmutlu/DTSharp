using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTSharp
{
    public abstract class FeatureValue
    {
    }

    public class DiscreteFeatureValue: FeatureValue
    {
        public object Value { get; set; }
    }

    public class ContiniousFeatureValue: FeatureValue
    {
        public IComparable From { get; set; }
        public IComparable To { get; set; }
    }
}
