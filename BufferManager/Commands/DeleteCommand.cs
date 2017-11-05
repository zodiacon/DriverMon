using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferManager.Commands {
	sealed class DeleteCommand : InsertCommand {
		public DeleteCommand(IBufferOperations buffer, long offset, int count)
			: base(buffer, offset, ((IBufferManager)buffer).GetBytes(offset, ref count)) { }
		
		public override void Execute() {
			base.Undo();
		}

		public override void Undo() {
			base.Execute();
		}
	}
}
