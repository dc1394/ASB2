using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using MyLogic;

namespace ASB2
{
    /// <summary>
    /// TextBox 添付ビヘイビア
    /// </summary>
    internal static class NumericBehaviors
    {
        // 最小値
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.RegisterAttached(
                "Minimum",
                typeof(Int32),
                typeof(NumericBehaviors));

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static Int32 GetMinimum(DependencyObject obj)
        {
            return (Int32)obj.GetValue(MinimumProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetMinimum(DependencyObject obj, Int32 value)
        {
            obj.SetValue(MinimumProperty, value);
        }

        // 最大値
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.RegisterAttached(
                "Maximum",
                typeof(Int32),
                typeof(NumericBehaviors));

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static Int32 GetMaximum(DependencyObject obj)
        {
            return (Int32)obj.GetValue(MaximumProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetMaximum(DependencyObject obj, Int32 value)
        {
            obj.SetValue(MaximumProperty, value);
        }

        // デフォルト
        public static readonly DependencyProperty DefaultProperty =
            DependencyProperty.RegisterAttached(
                "Default",
                typeof(Int32),
                typeof(NumericBehaviors));

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static Int32 GetDefault(DependencyObject obj)
        {
            return (Int32)obj.GetValue(DefaultProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetDefault(DependencyObject obj, Int32 value)
        {
            obj.SetValue(DefaultProperty, value);
        }

        // True なら入力を数字のみに制限
        public static readonly DependencyProperty IsNumericProperty =
            DependencyProperty.RegisterAttached(
                "IsNumeric",
                typeof(bool),
                typeof(NumericBehaviors),
                new UIPropertyMetadata(false, IsNumericChanged));

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static bool GetIsNumeric(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsNumericProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetIsNumeric(DependencyObject obj, bool value)
        {
            obj.SetValue(IsNumericProperty, value);
        }

        // IsNumericの値により、入力制限の設定・解除を行う
        private static void IsNumericChanged
            (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.KeyDown -= OnKeyDown;
                textBox.LostFocus -= OnLostFocus;
                DataObject.RemovePastingHandler(textBox, TextBoxPastingEventHandler);

                var newIsNumeric = (bool)e.NewValue;
                if (newIsNumeric)
                {
                    textBox.KeyDown += OnKeyDown;
                    textBox.LostFocus += OnLostFocus;
                    DataObject.AddPastingHandler(textBox, TextBoxPastingEventHandler);
                }
            }
        }

        // 入力制限（数値入力モード）
        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
           var textBox = sender as TextBox;
            if (textBox != null)
            {
                if ((Key.D0 <= e.Key && e.Key <= Key.D9) ||
                    (Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9) ||
                    (Key.Delete == e.Key) || (Key.Back == e.Key) ||
                    (Key.Tab == e.Key))
                {
                    e.Handled = false;
                }
                else
                {
                    // ここで止める
                    e.Handled = true;
                }
            }
        }

        // 入力値をチェックし、必要があれば補正する
        private static void OnLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    Int32 val = Int32.Parse(textBox.Text);
                    Int32 max = GetMaximum(textBox);
                    if (val < GetMinimum(textBox))
                    {
                        Int32 i = GetDefault(textBox);
                        textBox.Text = i.ToString();
                    }
                    else if (val > max)
                    {
                        textBox.Text = max.ToString();
                    }
                }
                else
                {
                    textBox.Text = GetDefault(textBox).ToString();
                }
            }
        }

        // クリップボード経由の貼り付けチェック
        private static void TextBoxPastingEventHandler(object sender, DataObjectPastingEventArgs e)
        {
            TextBox textBox = (sender as TextBox);

            Int32 val;
            if (textBox != null &&
                Int32.TryParse(e.DataObject.GetData(typeof(string)) as string, out val) &&
                val >= GetMinimum(textBox))
            {
                Int32 max = GetMaximum(textBox);
                if (val > max)
                {
                    textBox.Text = max.ToString();
                }
                textBox.Text = val.ToString();
            }

            e.CancelCommand();
            e.Handled = true;
        }
    }
}
