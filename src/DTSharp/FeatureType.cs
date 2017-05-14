using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTSharp
{
    /// <summary>
    /// Type of feature value
    /// </summary>
    public enum FeatureType
    {
        /// <summary>
        /// Represents features like age group, job, location, etc.
        /// </summary>
        Discrete,
        /// <summary>
        /// Represents features like age,height,probability etc.
        /// </summary>
        Continious
    }
}
