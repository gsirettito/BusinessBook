using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace SiretT.Controls {
    [DefaultEvent("ValueChanged")]
    [Localizability(LocalizationCategory.Text)]
    [TemplatePart(Name = "PART_ContentHost", Type = typeof(FrameworkElement))]
    public class NumericBox : TextBox {
        private NumericType _type;
        private double _step;
        private int caret;
        private string _text;
        public NumericBox()
            : base() {
                _step = 1;
                base.Text = "0";
                this.AddCharacterCommand = new DelegateCommand(ExecuteAddCharacter);
                this.DeleteCharacterCommand = new DelegateCommand(ExecuteDeleteCharacter);
                NotifyChange = true;
        }

        public class ValueEventArgs : EventArgs {
            public ValueEventArgs(object value) {
                this.Value = value;
            }

            public object Value { get; set; }
        }

        protected void OnValueChanged(object value) {
            if(ValueChanged != null)
                ValueChanged(this, new ValueEventArgs(value));
        }

        public event EventHandler<EventArgs> ValueChanged;

        static readonly DependencyProperty valueProperty =
            DependencyProperty.Register(
            "Value", typeof(object),
            typeof(NumericBox),
            new PropertyMetadata((int)0, OnValueChanged));

        static readonly DependencyProperty commandProperty =
            DependencyProperty.Register(
            "Command", typeof(ICommand),
            typeof(NumericBox),
            new PropertyMetadata(null, OnValueChanged));

        static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            (obj as NumericBox).OnValueChanged(args);
        }

        public interface IDelegateCommand : ICommand {
            void RaiseCanExecuteChanged();
        }

        public class DelegateCommand : IDelegateCommand {
            Action<object> execute;
            Func<object, bool> canExecute;
            // Event required by ICommand
            public event EventHandler CanExecuteChanged;
            // Two constructors
            public DelegateCommand(Action<object> execute, Func<object, bool> canExecute) {
                this.execute = execute;
                this.canExecute = canExecute;
            }
            public DelegateCommand(Action<object> execute) {
                this.execute = execute;
                this.canExecute = this.AlwaysCanExecute;
            }
            // Methods required by ICommand
            public void Execute(object param) {
                execute(param);
            }
            public bool CanExecute(object param) {
                return canExecute(param);
            }
            // Method required by IDelegateCommand
            public void RaiseCanExecuteChanged() {
                if(CanExecuteChanged != null)
                    CanExecuteChanged(this, EventArgs.Empty);
            }
            // Default CanExecute method
            bool AlwaysCanExecute(object param) {
                return true;
            }
        }

        public IDelegateCommand AddCharacterCommand { protected set; get; }
        public IDelegateCommand DeleteCharacterCommand { protected set; get; }

        void ExecuteAddCharacter(object param) {
            if(param is NumericBox) {
                (param as NumericBox).Cressing();
                (param as NumericBox).SetContent((param as NumericBox).Value);
            }
        }
        void ExecuteDeleteCharacter(object param) {
            if(param is NumericBox) {
                (param as NumericBox).Decressing();
                (param as NumericBox).SetContent((param as NumericBox).Value);
            }
        }
        bool CanExecuteDeleteCharacter(object param) {
            if(param is NumericBox) {
                NumericBox numBox = (param as NumericBox);
                double _double;
                if(double.TryParse(numBox.TextBase, out _double)) {
                    int _int = _double >= int.MaxValue ? int.MaxValue : (int)_double;
                    byte _byte = _int >= byte.MaxValue ? byte.MaxValue : (byte)_int;

                    switch(NumType) {
                        case NumericType.ByteType: return _byte >= byte.MinValue;
                        case NumericType.DoubleType: return _double >= double.MinValue;
                        case NumericType.IntType: return _int >= int.MinValue;
                    }
                }
            }
            return false;
        }

        protected void OnValueChanged(DependencyPropertyChangedEventArgs args) {
            SetContent(args.NewValue);
            if(NotifyChange)
                OnValueChanged((object)args.NewValue);
        }

        public static DependencyProperty ValueProperty {
            get { return valueProperty; }
        }

        public static DependencyProperty CommandProperty {
            get { return commandProperty; }
        }

        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object Value {
            set {
                switch(_type) {
                    case NumericType.IntType: 
                        int _int;
                        if(!int.TryParse(value.ToString().Replace('.', ','), out _int))
                            _int = Convert.ToInt32(Value);
                        SetValue(ValueProperty, _int);
                        break;
                    case NumericType.DoubleType: SetValue(ValueProperty, Convert.ToDouble(value)); break;
                    case NumericType.ByteType:
                        byte _byte;
                        if(!byte.TryParse(value.ToString().Replace('.',','), out _byte))
                            _byte = Convert.ToByte(Value);
                        SetValue(ValueProperty, _byte);
                        break;
                }
            }
            get { return GetValue(ValueProperty); }
        }

        [Browsable(false)]
        public virtual string Text { get; set; }

        public string TextBase { get { return base.Text; } }

        public void SetContent(object content) { base.Text = content.ToString(); }

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e) {
            caret = CaretIndex;
            int vkey = System.Windows.Input.KeyInterop.VirtualKeyFromKey(e.Key);
            int key = (vkey > 47 && vkey < 106) || vkey == 190 || vkey == 188 ? vkey % 48 : -1;
            int comma = 46;
            int period = 44;
            if(e.Key == Key.Delete || e.Key == Key.Back)
                e.Handled = false;
            else if(e.Key == System.Windows.Input.Key.Up) {
                Cressing();
                base.Text = Value.ToString();
                base.Select(0, base.Text.Length);
            } else if(e.Key == System.Windows.Input.Key.Down) {
                Decressing();
                base.Text = Value.ToString();
                base.Select(0, base.Text.Length);
            } else
                if((key < 0 || key > 9) &&
                    (key != comma && key != period))
                    e.Handled = true;
                else {
                    e.Handled = true;
                    string __text = base.Text.Insert(caret, key.ToString());
                    double _double;
                    if(double.TryParse(__text, out _double)) {
                        int _int = _double > int.MaxValue ? int.MaxValue : (int)_double;
                        byte _byte = _int > byte.MaxValue ? byte.MaxValue : (byte)_int;

                        switch(NumType) {
                            case NumericType.ByteType: Value = _byte; break;
                            case NumericType.DoubleType: Value = _double; break;
                            case NumericType.IntType: Value = _int; break;
                        }
                        base.Text = Value.ToString();
                        if(_double > Convert.ToDouble(Value))
                            caret = base.Text.Length - 1;
                        CaretIndex = caret + 1;
                    }
                }
            //base.OnPreviewKeyDown(e);
        }

        protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e) {
            if(e.Delta > 0) 
                Cressing();
             else
                Decressing();
            base.Text = Value.ToString();
            SelectAll();
            base.OnMouseWheel(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e) {
            //double value;
            //if(string.IsNullOrEmpty(base.Text))
            //    value = 0;
            //else if(!double.TryParse(base.Text.Replace('.', ','), out value))
            //    value = Convert.ToDouble(Value);
            //Value = value;
        }

        public int Direction {
            private set;
            get;
        }

        public void Cressing() {
            Direction = 1;
            switch(NumType) {
                case NumericType.IntType:
                    int _int;
                    if(!int.TryParse(Value.ToString(), out _int))
                        _int = Convert.ToInt32(Value);
                    if(_int < int.MaxValue)
                        Value = _int + (int)Step;
                    else Value = int.MaxValue;
                    break;
                case NumericType.DoubleType:
                    double dvalue = Convert.ToDouble(Value);
                    if(dvalue < double.MaxValue)
                        Value = dvalue + Step;
                    else Value = double.MaxValue;
                    break;
                case NumericType.ByteType:
                    byte _byte;
                    if(!byte.TryParse(Value.ToString(), out _byte))
                        _byte = Convert.ToByte(Value);
                    if(_byte < byte.MaxValue)
                        Value = _byte + (byte)Step;
                    else Value = byte.MaxValue;
                    break;
            }
        }

        public void Decressing() {
            Direction = -1;
            switch(NumType) {
                case NumericType.IntType:
                    int _int;
                    if(!int.TryParse(Value.ToString(), out _int))
                        _int = Convert.ToInt32(Value);
                    if(_int > int.MinValue)
                        Value = _int - (int)Step;
                    else Value = int.MinValue;
                    break;
                case NumericType.DoubleType:
                    double dvalue = Convert.ToDouble(Value);
                    if(dvalue > double.MinValue)
                        Value = dvalue - Step;
                    else Value = double.MinValue;
                    break;
                case NumericType.ByteType:
                    byte _byte;
                    if(!byte.TryParse(Value.ToString(), out _byte))
                        _byte = Convert.ToByte(Value);
                    if(_byte > byte.MinValue)
                        Value = _byte - (byte)Step;
                    else Value = byte.MinValue;
                    break;
            }
        }

        public NumericType NumType {
            get { return _type; }
            set {
                _type = value;
                switch(value) {
                    case NumericType.IntType: Value = Convert.ToInt32(Value); break;
                    case NumericType.DoubleType: Value = Convert.ToDouble(Value); break;
                    case NumericType.ByteType: Value = Convert.ToByte(Value); break;
                }
            }
        }

        public double Step {
            get { return _step; }
            set {
                _step = value;
                switch(_type) {
                    case NumericType.IntType: _step = (int)_step; break;
                    case NumericType.DoubleType: _step = (double)_step; break;
                    case NumericType.ByteType: _step = (byte)_step; break;
                }
            }
        }

        public enum NumericType {
            DoubleType,
            IntType,
            ByteType
        }

        public bool NotifyChange { get; set; }
    }
}
