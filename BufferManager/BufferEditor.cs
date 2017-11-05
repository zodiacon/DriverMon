using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferManager {
	public class BufferEditor : IBufferEditor {
		readonly IBufferManager _bufferManager;
		EditMode _editMode;
		long _caretOffset, _startOffset;
		int _inputBase = 16;
		int _wordSize = 1;
		ulong _currentValue;
		int _currentDigit;
		List<byte> _numbers = new List<byte>(64);
		Stack<byte> _digitStack = new Stack<byte>();
		static byte[,] _digits;

		static BufferEditor() {
			_digits = new byte[9, 17];
			// how many digits in a number of a given base?

			// for base 16
			_digits[1, 16] = 2; _digits[2, 16] = 4; _digits[4, 16] = 8; _digits[8, 16] = 16;
		}

		public BufferEditor(IBufferManager bufferManager) {
			_bufferManager = bufferManager;
		}

		public ulong CurrentValue => _currentValue;

		public EditMode EditMode {
			get => _editMode;
			set {
				if (_editMode != value) {
					Flush();
					_editMode = value;
					Reset();
				}
			}
		}

		public void Flush() {
			// flush current numbers
			if (EditMode == EditMode.Insert) {
				_bufferManager.Insert(_startOffset, _numbers.ToArray());
			}
			else {
				_bufferManager.SetData(_startOffset, _numbers.ToArray());
			}
			Reset();
		}

		public long CaretOffset {
			get => _caretOffset;
			set {
				if (value >= _bufferManager.Size)
					value = _bufferManager.Size;
				if (_caretOffset != value) {
					_caretOffset = value;
					OnCaretMoved();
				}
			}
		}

		private void OnCaretMoved() {
			CaretMoved?.Invoke(this, EventArgs.Empty);
		}

		public int WordSize {
			get => _wordSize;
			set {
				if (_wordSize != value) {
					Flush();
					_wordSize = value;
					Reset();
				}
			}
		}

		public int InputBase {
			get => _inputBase;
			set {
				if (_inputBase != value) {
					Flush();
					_inputBase = value;
					Reset();
				}
			}
		}

		public event EventHandler CaretMoved;

		public void UpdateDigit(byte value) {
			_currentValue = _currentValue * (uint)InputBase + (uint)value;
			_digitStack.Push(value);
			if (++_currentDigit == _digits[WordSize, InputBase]) {
				if (_numbers.Count == 0)
					_startOffset = CaretOffset;
				// number end
				_numbers.AddRange(BitConverter.GetBytes(_currentValue).Take(WordSize));
				CaretOffset += WordSize;
				_currentDigit = 0;
				_currentValue = 0;
			}
		}

		public void Reset() {
			_currentValue = 0;
			_currentDigit = 0;
			_numbers.Clear();
			_digitStack.Clear();
		}

		public bool Backup() {
			if (_currentDigit == 0)
				return false;
			_currentDigit--;
			_currentValue = (_currentValue - _digitStack.Pop()) / (uint)InputBase;
			return true;
		}
	}
}
