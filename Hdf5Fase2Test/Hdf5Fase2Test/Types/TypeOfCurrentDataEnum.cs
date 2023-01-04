using HDF.PInvoke;
using System.Runtime.InteropServices;

namespace Hdf5Fase2Test.Types
{
    public class TypeOfCurrentDataEnum : CustomEnumBase
    {
        private Dictionary<byte, string> _values = new Dictionary<byte, string>()
        {
            { 1, "Historical observation" },
            { 2, "Real-time observation" },
            { 3, "Astronomical prediction" },
            { 4, "Analysis or hybrid method" },
            { 5, "Hydrodynamic model hindcast" },
            { 6, "hydrodynamic model forecast" }
        };

        private byte _current;

        public TypeOfCurrentDataEnum()
        {

        }

        public TypeOfCurrentDataEnum(byte current)
        {
            if (_values.ContainsKey(current))
            {
                this._current = current;
            }
            else
            {
                throw new InvalidDataException($"{current} is an invalid value!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="current"></param>
        public override void WriteToHdf5(long groupId, byte current)
        {
            if (_values.ContainsKey(current))
            {
                _current = current;
                WriteToHdf5(groupId);
            }
            else
            {
                throw new InvalidDataException($"{current} is an invalid value!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        public override void WriteToHdf5(long groupId)
        {
            long enum_id = H5T.enum_create(H5T.INTEL_U8);

            var currentHandle = GCHandle.Alloc(_current, GCHandleType.Pinned);
            var handles = new List<GCHandle>();
            foreach (KeyValuePair<byte, string> item in _values)
            {
                var handle = GCHandle.Alloc(item.Key, GCHandleType.Pinned);
                H5T.enum_insert(enum_id, item.Value, handle.AddrOfPinnedObject());
                handles.Add(handle);
            }

            try
            {
                var space_id = H5S.create(H5S.class_t.SCALAR);
                var acpl = H5P.create(H5P.ATTRIBUTE_CREATE);
                H5P.set_char_encoding(acpl, H5T.cset_t.UTF8);

                var attr_id = H5A.create(groupId, "typeOfCurrentData", enum_id, space_id, acpl, H5P.DEFAULT);
                H5A.write(attr_id, enum_id, currentHandle.AddrOfPinnedObject());
                H5A.close(attr_id);
            }
            finally
            {
                foreach (GCHandle handle in handles)
                {
                    handle.Free();
                }
                currentHandle.Free();
            }
        }
    }
}
