//-----------------------------------------------------------------------
// <copyright file="EnumBooleanConverter.cs" company="dc1394's software">
//     but this is originally adapted by ba
//     cf. http://frog.raindrop.jp/knowledge/archives/002200.html
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ASB2
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Enum値とBoolean値を相互変換するクラス
    /// </summary>
    internal sealed class EnumBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Enum値をBoolean値に変換する
        /// </summary>
        /// <param name="value">変換したいEnum値</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">バインドするEnum値の文字列表記</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>変換後のBoolean値</returns>
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            // バインドする列挙値の文字列表記がパラメータに渡されているか
            var paramString = parameter as String;
            if (paramString == null)
            {
                return DependencyProperty.UnsetValue;
            }

            // パラメータが Enum の値として正しいか
            if (!Enum.IsDefined(value.GetType(), paramString))
            {
                return DependencyProperty.UnsetValue;
            }

            // パラメータを Enum に変換し、値が一致すればtrueを返す
            return value.Equals(Enum.Parse(value.GetType(), paramString));
        }

        /// <summary>
        /// Boolean値をEnum値に変換する
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetType">変換したい型</param>
        /// <param name="parameter">バインドするEnum値の文字列表記</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>変換後のEnum値</returns>
        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            // バインドするEnum値の文字列表記がパラメータに渡されているか
            var paramString = parameter as String;
            if (paramString == null)
            {
                return DependencyProperty.UnsetValue;
            }

            // Enum型にパースして返す
            return Enum.Parse(targetType, paramString);
        }
    }
}
