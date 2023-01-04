using HDF5CSharp.DataTypes;
using System.Runtime.InteropServices;

namespace Hdf5Fase2Test.Types
{
    public struct UncertaintyInstance
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 35)]
        [Hdf5EntryName("name")]
        public string name;
        [Hdf5EntryName("value")]
        public double value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public UncertaintyInstance(string name, double value)
        {
            this.name = name;
            this.value = value;
        }
    }
}
