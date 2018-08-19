using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        public unsafe static IEnumerable<string> GetDriversFromObjectManager(string folder) {
            var objAttributes = new ObjectAttributes();
            var directoryName = new UnicodeString();
            string name = folder;
            var buffer = Marshal.AllocCoTaskMem(1 << 16);
            var info = (ObjectDirectoryInformation*)buffer.ToPointer();
            var drivers = new List<string>(128);
            fixed (char* sname = name) {
                directoryName.Init(sname, name.Length);
                objAttributes.Init(&directoryName, ObjectAttributesFlags.CaseInsensitive);
                if (0 == NtDll.NtOpenDirectoryObject(out var hDirectory, DirectoryAccessMask.Query | DirectoryAccessMask.Traverse, ref objAttributes)) {
                    var first = true;
                    int index = 0;
                    var status = NtDll.NtQueryDirectoryObject(hDirectory, info, 1 << 16, false, first, ref index, out var returned);
                    if (status == 0) {
                        for (int i = 0; i < index; i++) {
                            drivers.Add(new string(info[i].Name.Buffer));
                        }
                    }
                }
            }
            Marshal.FreeCoTaskMem(buffer);
            return drivers;
        }
    }
}
