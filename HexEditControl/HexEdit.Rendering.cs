using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace HexEditControl {
	partial class HexEdit {
		protected override void OnRender(DrawingContext dc) {
			if (!IsLoaded)
				return;

			dc.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));
			DrawBuffer(dc);
		}

		long _startOffset, _endOffset;
		Dictionary<int, Point> _lines = new Dictionary<int, Point>(50);     // line to point of first character
		Point _startPoint;

		Dictionary<long, Brush> _highlites = new Dictionary<long, Brush>(128);

		void DrawBuffer(DrawingContext dc) {
            if (_buffer == null)
                return;

			var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
			var lineHeight = FontSize + VerticalSpace;
			var y = -_verticalScroll.Value;
			Debug.WriteLine($"y={y}");
			var viewport = _verticalScroll.ViewportSize;

            int bytesPerLine = BytesPerLine;
			long start = (long)(bytesPerLine * (0 - VerticalSpace - y) / lineHeight);

			if (start < 0)
				start = 0;
			else
				start = start / bytesPerLine * bytesPerLine;


			var maxWidth = 0.0;

			long end = start + bytesPerLine * ((long)(viewport / lineHeight + 1));
			if (end > _buffer.Size)
				end = _buffer.Size;

			_startOffset = start;
			_endOffset = end;

			if (true) {
				int addressDigits = 8;
				if (_buffer.Size < 1 << 16)
					addressDigits = 4;
				else if (_buffer.Size >= 1L << 32)
					addressDigits = 16;

				var formatter = "X" + addressDigits.ToString();

				double bytesWidth = 0;

				_lines.Clear();
				int line = 0;
				for (long i = start; i < end; i += bytesPerLine) {
					var pos = 2 + (i / bytesPerLine) * lineHeight + y;
					var text = new FormattedText(i.ToString(formatter) + ": ", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Brushes.Blue, 1);
					if (text.Width > maxWidth)
						maxWidth = text.Width;
					dc.DrawText(text, new Point(2, pos));

					int count = bytesPerLine;
					var bytes = _buffer.GetBytes(i, ref count);

					Point pt = new Point(10 + text.Width, pos);
					if (line == 0)
						_startPoint = pt;

					var width = DrawDataLine(dc, pt.X, pt.Y, i, bytes, bytesPerLine, typeface);
					_lines[line++] = pt;

					if (width > bytesWidth)
						bytesWidth = width;

					if (true) {
						DrawCharacterLine(dc, 20 + text.Width + bytesWidth, pos, bytes, bytesPerLine, typeface);
					}
				}
			}

			UpdateCaretPosition();

		}

		private double DrawDataLine(DrawingContext dc, double x, double y, long offset, byte[] bytes, int bytesPerLine, Typeface typeface) {
			var wordSize = WordSize;
			var converter = _bitConverters[_bitConverterIndex[wordSize]];

			_text.Clear();
			var xstart = x;
			for (int i = 0; i < bytes.Length; i += wordSize) {
				if (wordSize > 1 && i + wordSize > bytes.Length) {
					wordSize = 1;
					converter = _bitConverters[0];
				}
				_text.Append(converter(bytes, i)).Append(" ");
			}
			var text = new FormattedText(_text.ToString(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground, 1);
//			text.SetForegroundBrush(Brushes.Red, WordSize, WordSize);
			dc.DrawText(text, new Point(xstart, y));

			return text.Width;
		}

		private double DrawCharacterLine(DrawingContext dc, double x, double y, byte[] bytes, int bytesPerLine, Typeface typeface) {
			string value = bytes.Aggregate(_text.Clear(), (sb, b) => sb.Append(char.IsControl((char)b) ? '.' : (char)b)).ToString();
			var text = new FormattedText(value, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground, 1);
			dc.DrawText(text, new Point(x, y));
			return text.Width;
		}

	}
}
