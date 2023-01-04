using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Hdf5Fase2Test.Groups
{
    public class GroupFactory
    {
        public GroupFactory() { }

        /// <summary>
        ///     Create specified type of feature
        /// </summary>
        /// <param name="objectType">string value, type of feature</param>
        /// <returns>FeatureBase</returns>
        /// <exception cref="Exception"></exception>
        public GroupBase Create(string objectType)
        {
            var asm = Assembly.GetExecutingAssembly();
            ObjectHandle? handle = Activator.CreateInstance(asm.GetName().ToString(), $"Hdf5Fase2Test.Groups.{objectType}Group");
            if (handle != null)
            {
                var datasetInstance = handle.Unwrap() as GroupBase;

                if (datasetInstance != null)
                {
                    return datasetInstance;
                }
            }

            throw new Exception($"Couldn't create type '{objectType}'!");
        }
    }
}
