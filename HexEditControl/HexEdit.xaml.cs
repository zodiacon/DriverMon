using BufferManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HexEditControl {
	public partial class HexEdit : IHexEdit {
		static Func<byte[], int, string>[] _bitConverters = {
			(bytes, i) => bytes[i].ToString("X2"),
			(bytes, i) => BitConverter.ToUInt16(bytes, i).ToString("X4"),
			(bytes, i) => BitConverter.ToUInt32(bytes, i).ToString("X8"),
			(bytes, i) => BitConverter.ToUInt64(bytes, i).ToString("X16"),
		};
		static int[] _bitConverterIndex = { 0, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3 };

		IBufferManager _buffer;
		IBufferEditor _editor;

		[ThreadStatic]
		static StringBuilder _text;
		public HexEdit() {
			if (_text == null)
				_text = new StringBuilder(256);
			
			InitializeComponent();
			_caret.Visibility = Visibility.Collapsed;

			Loaded += HexEdit_Loaded;
			SizeChanged += HexEdit_SizeChanged;

		}

		private void HexEdit_SizeChanged(object sender, SizeChangedEventArgs e) {
			InvalidateVisual();
		}

		private void HexEdit_Loaded(object sender, RoutedEventArgs e) {
			SetValue(HexEditorProperty, this);
            Refresh();
		}

		public IBufferManager BufferManager => _buffer;

		public IBufferEditor Editor => _editor;

		private void OnScrollChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			InvalidateVisual();
		}

		private void OnMouseWheel(object sender, MouseWheelEventArgs e) {
			_verticalScroll.Value -= e.Delta;
			UpdateCaretPosition();
		}

		void InitBuffer() {
			_buffer.SizeChanged += _buffer_SizeChanged;
			_editor = new BufferEditor(_buffer);
			_editor.CaretOffset = 0;
			_editor.CaretMoved += _editor_CaretMoved;
			Refresh();
			ScrollToPosition(0);
			_caret.Visibility = IsReadOnly ? Visibility.Collapsed : Visibility.Visible;
			_editor.CaretOffset = 0;
			Focus();
		}

		private void UpdateCaretPosition() {
			var offset = _editor.CaretOffset;
			if (offset < _startOffset || offset > _endOffset) {
				_caret.Top = -100;
				return;
			}
			int line = (int)(offset - _startOffset) / BytesPerLine;
			_caret.Top = line * (FontSize + VerticalSpace) + 1;
			_caret.Left = 200;
		}

		private void _buffer_SizeChanged(object sender, EventArgs e) {
		}

		void ScrollToPosition(long offset) {
			var line = offset / BytesPerLine - 5;
			if (line < 0) line = 0;

			var y = line * (FontSize + VerticalSpace);
			_verticalScroll.Value = -y;
		}

		private void _editor_CaretMoved(object sender, EventArgs e) {
			UpdateCaretPosition();
		}

		public void LoadFile(string path) {
			var buffer = BufferManagerFactory.CreateFromFile(path);
			Release();
			_buffer = buffer;
			InitBuffer();
		}

		private void Refresh() {
			_horizontalScroll.ViewportSize = _buffer != null ? _canvas.ActualWidth : 0;
            _horizontalScroll.Maximum = _buffer != null ? _canvas.ActualWidth : 0;
			_verticalScroll.ViewportSize = _canvas.ActualHeight;
            if (_buffer != null) {
                _verticalScroll.Maximum = (_buffer.Size / BytesPerLine) * (FontSize + VerticalSpace) - _canvas.ActualHeight + VerticalSpace * 2 + FontSize;
            }
            else {
                _verticalScroll.Maximum = 0;
            }
			ScrollToPosition(_startOffset);
			InvalidateVisual();
		}

		private void Release() {
			_buffer?.Dispose();
			_editor = null;
			_buffer = null;
		}

		public void AttachToProcess(int pid) {
			var buffer = BufferManagerFactory.CreateFromProcess(pid);
			_buffer = buffer;
			InitBuffer();
		}

		public void CreateNew() {
			Release();
			_buffer = BufferManagerFactory.CreateInMemory();
			InitBuffer();
		}

		private void OnLeftMouseDown(object sender, MouseButtonEventArgs e) {
			// find the closet element and place the caret there
			var pos = e.GetPosition(_canvas);
			var diff = pos.Y - _verticalScroll.Value;
			int line = (int)(diff / (FontSize + VerticalSpace));
			if (_lines.TryGetValue(line, out var pt)) {
				_caret.Left = pos.X;
				_caret.Top = pt.Y;
			}
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
            if (BufferManager != null) {
                var oldValue = _verticalScroll.Value;
                Refresh();
                //_verticalScroll.Value = VerticalSpace + _startOffset * (FontSize + VerticalSpace) / BytesPerLine;
                _verticalScroll.Value = oldValue;
            }
        }

        public void AttachToBuffer(byte[] buffer, int startIndex = 0, int length = 0) {
            _buffer = BufferManagerFactory.CreateInMemory(buffer.Skip(startIndex).Take(length == 0 ? buffer.Length : length));
            InitBuffer();
        }
    }
}
