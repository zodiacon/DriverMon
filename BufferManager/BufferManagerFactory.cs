using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferManager {
	public static class BufferManagerFactory {
		public static IBufferManager CreateInMemory(IEnumerable<byte> data) {
			return new ByteBuffer(data);
		}

		public static IBufferManager CreateInMemory(int capacity = 0) {
			return new ByteBuffer(capacity);
		}

		public static IBufferManager CreateFromFile(string path) {
			return new MemoryMappedBuffer(path);
		}

		public static IBufferManager CreateFromProcess(int pid) {
			return new ProcessBuffer(pid);
		}
	}
}
