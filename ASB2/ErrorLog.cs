//-----------------------------------------------------------------------
// <copyright file="ErrorLog.cs" company="dc1394's software">
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MyErrorLog
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Windows;

    /// <summary>
    /// エラー処理の関数をまとめたクラス
    /// </summary>
    internal static class ErrorLog
    {
        #region フィールド

        /// <summary>
        /// log4netインスタンス（ログ記録用）
        /// </summary>
        internal static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// ログを記録するファイル名
        /// </summary>
        internal static readonly String ErrorLogFilename = "Errorlog-file.txt";

        #endregion フィールド

        /// <summary>
        /// エラーメッセージボックスを表示する
        /// </summary>
        /// <param name="errMsg">エラーメッセージ</param>
        internal static void CallErrorMessageBox(this String errMsg)
        {
            MessageBox.Show(
                errMsg,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        /// <summary>
        /// 例外をエラーとしてファイルに記録する
        /// </summary>
        /// <param name="ex">例外</param>
        internal static void WriteLog(this Exception ex)
        {
            ErrorLog.Log.Error(ex.ToString() + Environment.NewLine + ex.ToFullDisplayString());
        }

        /// <summary>
        /// 例外の詳細をStringBuilderに追加する
        /// </summary>
        /// <param name="ex">例外</param>
        /// <param name="builder">StringBuilder</param>
        private static void AddExceptionDetail(this Exception ex, StringBuilder builder)
        {
            builder.AppendFormat("Message: {0}{1}", ex.Message, Environment.NewLine);
            builder.AppendFormat("Type: {0}{1}", ex.GetType(), Environment.NewLine);
            builder.AppendFormat("HelpLink: {0}{1}", ex.HelpLink, Environment.NewLine);
            builder.AppendFormat("Source: {0}{1}", ex.Source, Environment.NewLine);
            builder.AppendFormat("TargetSite: {0}{1}", ex.TargetSite, Environment.NewLine);
            builder.AppendFormat("Data: {0}", Environment.NewLine);
            ex.Data
              .OfType<DictionaryEntry>()
              .ToList()
              .ForEach(
              c =>
                {
                    builder.AppendFormat("\t{0} : {1}{2}", c.Key, c.Value, Environment.NewLine);
                });

            builder.AppendFormat("StackTrace: {0}{1}", ex.StackTrace, Environment.NewLine);
        }

        /// <summary>
        /// ネストされた例外を列挙子にいれて返す
        /// </summary>
        /// <param name="ex">例外</param>
        /// <returns>例外が入った列挙子</returns>
        private static IEnumerable<Exception> GetNestedExceptionList(this Exception ex)
        {
            var current = ex;
            do
            {
                current = current.InnerException;
                if (current != null)
                {
                    yield return current;
                }
            }
            while (current != null);
        }

        /// <summary>
        /// 例外の詳細をStringBuilderに追加して、それをStringに変換して返す
        /// </summary>
        /// <param name="ex">例外</param>
        /// <returns>例外の詳細の文字列</returns>
        private static String ToFullDisplayString(this Exception ex)
        {
            var displayText = new StringBuilder();
            ex.AddExceptionDetail(displayText);
            ex.GetNestedExceptionList()
              .ToList()
              .ForEach(c =>
                {
                    displayText.AppendFormat("**** 内部例外 始点 **** {0}", Environment.NewLine);

                    c.AddExceptionDetail(displayText);

                    displayText.AppendFormat("**** 内部例外 終点 **** {0}{0}", Environment.NewLine);
                });

            return displayText.ToString();
        }
    }
}
