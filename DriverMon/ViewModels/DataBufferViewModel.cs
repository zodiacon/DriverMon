using BufferManager;
using HexEditControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Zodiacon.WPF;

namespace DriverMon.ViewModels {
    sealed class DataBufferViewModel : DialogViewModelBase {
        IrpViewModelBase _vm;
        public DataBufferViewModel(Window dialog, IrpViewModelBase vm) : base(dialog) {
            _vm = vm;
        }

        public string Title => $"({_vm.Index}) IRP: 0x{_vm.Irp:X}, operation: {_vm.MajorCode}";

        IHexEdit _editor;
        public IHexEdit Editor {
            get => _editor;
            set {
                if (value != null) {
                    _editor = value;
                    _editor.AttachToBuffer(_vm.Data);
                }
            }
        }

        public bool Is1Byte {
            get => _editor == null || _editor.WordSize == 1;
            set {
                if (value)
                    _editor.WordSize = 1;
            }
        }

        public bool Is2Byte {
            get => _editor?.WordSize == 2;
            set {
                if (value)
                    _editor.WordSize = 2;
            }
        }

        public bool Is4Byte {
            get => _editor?.WordSize == 4;
            set {
                if (value)
                    _editor.WordSize = 4;
            }
        }

        public bool Is8Byte {
            get => _editor?.WordSize == 8;
            set {
                if (value)
                    _editor.WordSize = 8;
            }
        }

        public bool Is16Bytes {
            get => _editor == null || _editor.BytesPerLine == 16;
            set {
                if (value)
                    _editor.BytesPerLine = 16;
            }
        }

        public bool Is24Bytes {
            get => _editor?.BytesPerLine == 24;
            set {
                if (value)
                    _editor.BytesPerLine = 24;
            }
        }

        public bool Is32Bytes {
            get => _editor?.BytesPerLine == 32;
            set {
                if (value)
                    _editor.BytesPerLine = 32;
            }
        }

        public bool Is48Bytes {
            get => _editor?.BytesPerLine == 48;
            set {
                if (value)
                    _editor.BytesPerLine = 48;
            }
        }

        public bool Is64Bytes {
            get => _editor?.BytesPerLine == 64;
            set {
                if (value)
                    _editor.BytesPerLine = 64;
            }
        }
    }
}