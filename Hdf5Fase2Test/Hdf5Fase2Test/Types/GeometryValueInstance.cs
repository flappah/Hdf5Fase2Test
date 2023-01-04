using HDF5CSharp.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hdf5Fase2Test.Types
{
    public struct GeometryValueInstance
    {
        [Hdf5EntryName("longitude")]
        public double longitude;
        [Hdf5EntryName("latitude")]
        public double latitude;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public GeometryValueInstance(double longitude, double latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
        }
    }
}
