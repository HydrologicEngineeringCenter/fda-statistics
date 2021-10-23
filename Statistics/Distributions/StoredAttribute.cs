using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Distributions
{
    [System.AttributeUsage(AttributeTargets.Property,AllowMultiple = false, Inherited = false)]
    class StoredAttribute : System.Attribute
    {
        public string Name;
        public Type type;
    }
}
