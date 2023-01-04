using Hdf5Fase2Test.Hdf5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Hdf5Fase2Test.Products
{
    public abstract class ProductBase : H5_ItemBase
    {
        public string[]? ValidFeatureTypes { get; set; }
        public object[]? Data { get; set; }
    }
}
