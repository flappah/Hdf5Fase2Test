using HDF5CSharp.DataTypes;

namespace Hdf5Fase2Test.Types
{
    public struct SurfaceCurrentInstance
    {
        [Hdf5EntryName("surfaceCurrentSpeed")]
        public double speed;

        [Hdf5EntryName("surfaceCurrentDirection")]
        public double direction;

        /// <summary>
        ///     
        /// </summary>
        /// <param name="height"></param>
        /// <param name="trend"></param>
        public SurfaceCurrentInstance(double speed, double direction)
        {
            this.speed = speed;
            this.direction = direction;
        }
    }
}
