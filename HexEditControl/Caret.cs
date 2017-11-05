using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace HexEditControl {
	public sealed class Caret : FrameworkElement {
		DispatcherTimer _timer;
		bool _hide;

		static Caret() {
			WidthProperty.OverrideMetadata(typeof(Caret), new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender));
		}

		public Caret() {
			Loaded += Caret_Loaded;
		}

		private void Caret_Loaded(object sender, RoutedEventArgs e) {
			_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(.5) };
			_timer.Tick += delegate {
				if (Visibility == Visibility.Visible) {
					_hide = !_hide;
					InvalidateVisual();
				}
				_timer.Interval = TimeSpan.FromMilliseconds(_hide ? 400 : 600);
			};
			_timer.Start();
		}

		public Brush Foreground {
			get { return (Brush)GetValue(ForegroundProperty); }
			set { SetValue(ForegroundProperty, value); }
		}

		public static readonly DependencyProperty ForegroundProperty =
			DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(Caret), 
				new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.AffectsRender));

		public double Left {
			get { return (double)GetValue(LeftProperty); }
			set { SetValue(LeftProperty, value); }
		}

		public static readonly DependencyProperty LeftProperty =
			DependencyProperty.Register(nameof(Left), typeof(double), typeof(Caret), 
				new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, (s, e) => ((Caret)s)._hide = false));


		public double Top {
			get { return (double)GetValue(TopProperty); }
			set { SetValue(TopProperty, value); }
		}

		public static readonly DependencyProperty TopProperty =
			DependencyProperty.Register(nameof(Top), typeof(double), typeof(Caret), 
				new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, (s, e) => ((Caret)s)._hide = false));

		protected override void OnRender(DrawingContext dc) {
			if (!_hide) {
				dc.DrawRectangle(Foreground, null, new Rect(Left, Top, Width, Height));
			}
		}
	}
}
