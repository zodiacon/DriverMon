using System;
using System.Windows;
using System.Windows.Input;

namespace HexEditControl {
	partial class HexEdit {
		public static RoutedCommand LineDown = new RoutedCommand(nameof(LineDown), typeof(HexEdit));
		public static RoutedCommand LineDownNoCaret = new RoutedCommand(nameof(LineDownNoCaret), typeof(HexEdit));
		public static RoutedCommand LineUp = new RoutedCommand(nameof(LineUp), typeof(HexEdit));
		public static RoutedCommand PageDown = new RoutedCommand(nameof(PageDown), typeof(HexEdit));
		public static RoutedCommand PageUp = new RoutedCommand(nameof(PageUp), typeof(HexEdit));
		public static RoutedCommand Home = new RoutedCommand(nameof(Home), typeof(HexEdit));
		public static RoutedCommand End = new RoutedCommand(nameof(End), typeof(HexEdit));

		static HexEdit() {
			LineDown.InputGestures.Add(new KeyGesture(Key.Down));
			LineUp.InputGestures.Add(new KeyGesture(Key.Up));
			PageDown.InputGestures.Add(new KeyGesture(Key.PageDown));
			PageUp.InputGestures.Add(new KeyGesture(Key.PageUp));
			Home.InputGestures.Add(new KeyGesture(Key.Home));
			End.InputGestures.Add(new KeyGesture(Key.End));

			LineDownNoCaret.InputGestures.Add(new KeyGesture(Key.Down, ModifierKeys.Control));

			CommandManager.RegisterClassCommandBinding(typeof(HexEdit), new CommandBinding(LineDown, (s, e) => ((HexEdit)s).ExecuteLineDown(e), (s, e) => ((HexEdit)s).CanExecuteLineDown(e)));
			CommandManager.RegisterClassCommandBinding(typeof(HexEdit), new CommandBinding(LineUp, (s, e) => ((HexEdit)s).ExecuteLineUp(e), (s, e) => ((HexEdit)s).CanExecuteLineDown(e)));
			CommandManager.RegisterClassCommandBinding(typeof(HexEdit), new CommandBinding(PageDown, (s, e) => ((HexEdit)s).ExecutePageDown(e), (s, e) => ((HexEdit)s).CanExecuteLineDown(e)));
			CommandManager.RegisterClassCommandBinding(typeof(HexEdit), new CommandBinding(PageUp, (s, e) => ((HexEdit)s).ExecutePageUp(e), (s, e) => ((HexEdit)s).CanExecuteLineDown(e)));
			CommandManager.RegisterClassCommandBinding(typeof(HexEdit), new CommandBinding(Home, (s, e) => ((HexEdit)s).ExecuteHome(e), (s, e) => ((HexEdit)s).CanExecuteLineDown(e)));
			CommandManager.RegisterClassCommandBinding(typeof(HexEdit), new CommandBinding(End, (s, e) => ((HexEdit)s).ExecuteEnd(e), (s, e) => ((HexEdit)s).CanExecuteLineDown(e)));

			CommandManager.RegisterClassCommandBinding(typeof(HexEdit), new CommandBinding(LineDownNoCaret, (s, e) => ((HexEdit)s).ExecuteLineDownNoCaret(e), (s, e) => ((HexEdit)s).CanExecuteLineDown(e)));
		}

		private void ExecuteLineDownNoCaret(ExecutedRoutedEventArgs e) {
			_verticalScroll.Value += FontSize + VerticalSpace;
		}

		private void ExecuteEnd(ExecutedRoutedEventArgs e) {
			_verticalScroll.Value = _verticalScroll.Maximum;
		}

		private void ExecuteHome(ExecutedRoutedEventArgs e) {
			_verticalScroll.Value = 0;
		}

		private void ExecutePageUp(ExecutedRoutedEventArgs e) {
			_verticalScroll.Value -= (FontSize + VerticalSpace) * 20;
		}

		private void ExecutePageDown(ExecutedRoutedEventArgs e) {
			_verticalScroll.Value += (FontSize + VerticalSpace) * 20;
		}

		private void CanExecuteLineDown(CanExecuteRoutedEventArgs e) {
			e.CanExecute = BufferManager != null;
		}

		private void ExecuteLineDown(ExecutedRoutedEventArgs e) {
			_editor.CaretOffset += BytesPerLine;
			if (_editor.CaretOffset > _endOffset - 10 * BytesPerLine) {
				_verticalScroll.Value += FontSize + VerticalSpace;
				if (_verticalScroll.Value == _verticalScroll.Maximum)
					UpdateCaretPosition();
			}
			else {
				UpdateCaretPosition();
			}
		}

		private void ExecuteLineUp(ExecutedRoutedEventArgs e) {
			_verticalScroll.Value -= FontSize + VerticalSpace;
		}

	}
}