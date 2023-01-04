namespace Hdf5Fase2Test.Types
{
    public abstract class CustomEnumBase
    {
        public abstract void WriteToHdf5(long groupId, byte current);
        public abstract void WriteToHdf5(long groupId);
    }
}
