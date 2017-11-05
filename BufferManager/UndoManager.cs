using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferManager {
	class UndoManager {
		List<ICommand> _undoList = new List<ICommand>();
		List<ICommand> _redoList = new List<ICommand>();

		public int UndoLevel { get; set; } = 16;

		public bool CanUndo => _undoList.Count > 0;
		public bool CanRedo => _redoList.Count > 0;

		public void Undo() {
			Debug.Assert(CanUndo);
			var cmd = _undoList[_undoList.Count - 1];
			cmd.Undo();
			_undoList.RemoveAt(_undoList.Count - 1);
			_redoList.Add(cmd);
		}

		public virtual void Redo() {
			Debug.Assert(CanRedo);
			var cmd = _redoList[_redoList.Count - 1];
			cmd.Execute();
			_redoList.RemoveAt(_redoList.Count - 1);
			_undoList.Add(cmd);
		}

		public void AddCommand(ICommand command, bool execute = true) {
			if (execute)
				command.Execute();
			_undoList.Add(command);
			_redoList.Clear();
			if (UndoLevel > 0 && _undoList.Count > UndoLevel)
				_undoList.RemoveAt(0);
		}

	}

}
