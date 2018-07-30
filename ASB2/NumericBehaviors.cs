//-----------------------------------------------------------------------
// <copyright file="NumericBehaviors.cs" company="dc1394's software">
//     but this is originally adapted by Nine Works
//     cf. http://nine-works.blog.ocn.ne.jp/blog/2011/03/post_7f40.html
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ASB2
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// TextBox 添付ビヘイビア
    /// </summary>
    internal static class NumericBehaviors
    {
        #region フィールド

        /// <summary>
        /// テキストボックスのデフォルトの数値
        /// テキストボックスに対する依存プロパティ
        /// </summary>
        public static readonly DependencyProperty DefaultProperty =
            DependencyProperty.RegisterAttached(
                "Default",
                typeof(Int32),
                typeof(NumericBehaviors));

        /// <summary>
        /// True なら入力を数字のみに制限する
        /// テキストボックスに対する依存プロパティ
        /// </summary>
        public static readonly DependencyProperty IsNumericProperty =
            DependencyProperty.RegisterAttached(
                "IsNumeric",
                typeof(bool),
                typeof(NumericBehaviors),
                new UIPropertyMetadata(false, IsNumericChanged));

        /// <summary>
        /// テキストボックスの最大値を設定する
        /// テキストボックスに対する依存プロパティ
        /// </summary>
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.RegisterAttached(
                "Maximum",
                typeof(Int32),
                typeof(NumericBehaviors));

        /// <summary>
        /// テキストボックスの最小値を設定する
        /// テキストボックスに対する依存プロパティ
        /// </summary>
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.RegisterAttached(
                "Minimum",
                typeof(Int32),
                typeof(NumericBehaviors));

        #endregion フィールド

        #region メソッド

        /// <summary>
        /// 指定されたテキストボックスのデフォルトの数値を取得する
        /// </summary>
        /// <param name="obj">テキストボックスに対する依存プロパティ</param>
        /// <returns>指定されたテキストボックスのデフォルトの数値</returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static Int32 GetDefault(DependencyObject obj)
        {
            return (Int32)obj.GetValue(DefaultProperty);
        }

        /// <summary>
        /// 指定されたテキストボックスへ入力できるのが数値のみかどうかを取得する
        /// </summary>
        /// <param name="obj">テキストボックスに対する依存プロパティ</param>
        /// <returns>指定されたテキストボックスへ入力できるのが数値のみかどうか</returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static Boolean GetIsNumeric(DependencyObject obj)
        {
            return (Boolean)obj.GetValue(NumericBehaviors.IsNumericProperty);
        }

        /// <summary>
        /// 指定されたテキストボックスの最大値を取得する
        /// </summary>
        /// <param name="obj">テキストボックスに対する依存プロパティ</param>
        /// <returns>指定されたテキストボックスの最大値</returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static Int32 GetMaximum(DependencyObject obj)
        {
            return (Int32)obj.GetValue(NumericBehaviors.MaximumProperty);
        }

        /// <summary>
        /// 指定されたテキストボックスの最小値を取得する
        /// </summary>
        /// <param name="obj">テキストボックスに対する依存プロパティ</param>
        /// <returns>指定されたテキストボックスの最小値</returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static Int32 GetMinimum(DependencyObject obj)
        {
            return (Int32)obj.GetValue(NumericBehaviors.MinimumProperty);
        }

        /// <summary>
        /// 指定されたテキストボックスのデフォルトの数値を設定する
        /// </summary>
        /// <param name="obj">テキストボックスに対する依存プロパティ</param>
        /// <param name="value">指定されたテキストボックスの数値の初期値</param>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetDefault(DependencyObject obj, Int32 value)
        {
            obj.SetValue(DefaultProperty, value);
        }

        /// <summary>
        /// 指定されたテキストボックスへ入力できるのが数値のみかどうかを設定する
        /// </summary>
        /// <param name="obj">テキストボックスに対する依存プロパティ</param>
        /// <param name="value">指定されたテキストボックスへ入力できるのが数値のみかどうか</param>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetIsNumeric(DependencyObject obj, Boolean value)
        {
            obj.SetValue(IsNumericProperty, value);
        }

        /// <summary>
        /// 指定されたテキストボックスの最大値を設定する
        /// </summary>
        /// <param name="obj">テキストボックスに対する依存プロパティ</param>
        /// <param name="value">指定されたテキストボックスの最大値</param>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetMaximum(DependencyObject obj, Int32 value)
        {
            obj.SetValue(NumericBehaviors.MaximumProperty, value);
        }

        /// <summary>
        /// 指定されたテキストボックスの最小値を設定する
        /// </summary>
        /// <param name="obj">テキストボックスに対する依存プロパティ</param>
        /// <param name="value">指定されたテキストボックスの最小値</param>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetMinimum(DependencyObject obj, Int32 value)
        {
            obj.SetValue(NumericBehaviors.MinimumProperty, value);
        }

        #endregion メソッド

        #region イベントハンドラ

        /// <summary>
        /// IsNumericの値により、入力制限の設定・解除を行う
        /// </summary>
        /// <param name="sender">テキストボックス</param>
        /// <param name="e">The parameter is not used.</param>
        private static void IsNumericChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.KeyDown -= OnKeyDown;
                textBox.LostFocus -= OnLostFocus;
                DataObject.RemovePastingHandler(textBox, TextBoxPastingEventHandler);

                if ((Boolean)e.NewValue)
                {
                    textBox.KeyDown += OnKeyDown;
                    textBox.LostFocus += OnLostFocus;
                    DataObject.AddPastingHandler(textBox, TextBoxPastingEventHandler);
                }
            }
        }

        /// <summary>
        /// テキストボックスへの入力を、数値のみに制限する
        /// </summary>
        /// <param name="sender">入力制限を行うテキストボックス</param>
        /// <param name="e">入力されたキー</param>
        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
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

        /// <summary>
        /// 入力値をチェックし、必要があれば補正する
        /// </summary>
        /// <param name="sender">入力制限を行うテキストボックス</param>
        /// <param name="e">The parameter is not used.</param>
        private static void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    Int32.TryParse(textBox.Text, out Int32 val);

                    var max = NumericBehaviors.GetMaximum(textBox);
                    if (val < NumericBehaviors.GetMinimum(textBox))
                    {
                        Int32 i = NumericBehaviors.GetDefault(textBox);
                        textBox.Text = i.ToString();
                    }
                    else if (val > max)
                    {
                        textBox.Text = max.ToString();
                    }
                }
                else
                {
                    textBox.Text = NumericBehaviors.GetDefault(textBox).ToString();
                }
            }
        }

        /// <summary>
        /// テキストボックスへの入力を、数値とマイナスのみに制限する
        /// クリップボード経由の貼り付けを監視
        /// </summary>
        /// <param name="sender">入力制限を行うテキストボックス</param>
        /// <param name="e">ペーストされた値</param>
        private static void TextBoxPastingEventHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is TextBox textBox &&
                Int32.TryParse(e.DataObject.GetData(typeof(string)) as string, out Int32 val) &&
                val >= NumericBehaviors.GetMinimum(textBox))
            {
                var max = NumericBehaviors.GetMaximum(textBox);
                if (val > max)
                {
                    textBox.Text = max.ToString();
                }

                textBox.Text = val.ToString();
            }

            e.CancelCommand();
            e.Handled = true;
        }

        #endregion イベントハンドラ
    }
}
