using System;
using System.Windows;

namespace HexEditControl {
	partial class HexEdit {

		public int BytesPerLine {
			get { return (int)GetValue(BytesPerLineProperty); }
			set { SetValue(BytesPerLineProperty, value); }
		}

		public static readonly DependencyProperty BytesPerLineProperty =
			DependencyProperty.Register(nameof(BytesPerLine), typeof(int), typeof(HexEdit), 
				new FrameworkPropertyMetadata(16, FrameworkPropertyMetadataOptions.AffectsRender, (s, e) => ((HexEdit)s).Refresh()));


		public double VerticalSpace {
			get { return (double)GetValue(VerticalSpaceProperty); }
			set { SetValue(VerticalSpaceProperty, value); }
		}

		public static readonly DependencyProperty VerticalSpaceProperty =
			DependencyProperty.Register(nameof(VerticalSpace), typeof(double), typeof(HexEdit), 
				new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

		public int WordSize {
			get { return (int)GetValue(WordSizeProperty); }
			set { SetValue(WordSizeProperty, value); }
		}

		public static readonly DependencyProperty WordSizeProperty =
			DependencyProperty.Register(nameof(WordSize), typeof(int), typeof(HexEdit), 
				new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender));

		public IHexEdit HexEditor {
			get { return (IHexEdit)GetValue(HexEditorProperty); }
			set { throw new InvalidOperationException(); }
		}

		public static readonly DependencyProperty HexEditorProperty =
			DependencyProperty.Register(nameof(HexEditor), typeof(IHexEdit), typeof(HexEdit), 
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(HexEdit), 
                new PropertyMetadata(true));

    }
}
