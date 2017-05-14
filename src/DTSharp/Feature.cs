using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DTSharp
{
    public abstract class Feature
    {
        public string Name { get; set; }
        public FeatureType Type { get; set; }
        public abstract Expression Selector { get; }
        public abstract Type DataType { get; }

    }
    public class Feature<TEntity, TFeature> : Feature
    {
        public Expression<Func<TEntity, TFeature>> GenericSelector { get; set; }
        public override Expression Selector { get => GenericSelector; }
        public override Type DataType { get => typeof(TFeature); }
    }
}
