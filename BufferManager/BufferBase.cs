using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferManager {
	abstract class BufferBase : IBufferManager {
		UndoManager _commandManager = new UndoManager();

		protected BufferBase() {
		}

		protected UndoManager CommandManager => _commandManager;

		public int UndoLevel {
			get => _commandManager.UndoLevel;
			set => _commandManager.UndoLevel = value;
		}

		protected virtual void OnSizeChanged() {
			SizeChanged?.Invoke(this, EventArgs.Empty);
		}

		public virtual bool IsReadOnly => false;

		public bool CanUndo => _commandManager.CanUndo;

		public bool CanRedo => _commandManager.CanRedo;

		public abstract long Size { get; }

		public event EventHandler SizeChanged;

		public abstract void Delete(long offset, int count);

		public virtual void Dispose() { }

		public abstract byte[] GetBytes(long offset, ref int count);

		public abstract void Insert(long offset, byte[] data);

		public void Redo() {
			_commandManager.Redo();
		}

		public abstract void SetData(long offset, byte[] data);

		public void Undo() {
			_commandManager.Undo();
		}
	}
}
