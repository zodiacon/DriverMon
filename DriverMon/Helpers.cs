using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DriverMon {
    static class Helpers {
        public static void Save<T>(Stream stream, T obj) where T : class {
            var writer = new DataContractSerializer(typeof(T));
            writer.WriteObject(stream, obj);
        }

        public static T Load<T>(Stream stream) where T : class {
            var reader = new DataContractSerializer(typeof(T));
            return reader.ReadObject(stream) as T;
        }
    }
}
