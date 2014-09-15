//-----------------------------------------------------------------------
// <copyright file="ErrorCheck.cs" company="dc1394's software">
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ASB2
{
    using System;

    /// <summary>
    /// エラーに関するクラス
    /// </summary>
    internal static class ErrorCheck
    {
        /// <summary>
        /// メインスレッド以外で投げられた例外をメインスレッドで投げ返す
        /// </summary>
        /// <param name="arg">例外</param>
        /// <returns>なし</returns>
        internal static Object ThrowMainThreadException(Object arg)
        {
            var ex = arg as Exception;
            ErrorCheck.WriteAndThrow<SystemException>(ex.Message, ex);

            return null;
        }

        /// <summary>
        /// 与えられたエラーメッセージをログファイルに書き込み、
        /// 与えられたエラーメッセージで与えられた型の例外を投げる
        /// </summary>
        /// <typeparam name="T">例外の型</typeparam>
        /// <param name="errmsg">エラーメッセージ</param>
        /// <param name="innerException">内部例外</param>
        internal static void WriteAndThrow<T>(String errmsg, Exception innerException) where T : Exception
        {
            // エラーログを書き込む
            MyErrorLog.ErrorLog.Log.Error(errmsg);
            var ctor = typeof(T).GetConstructor(new Type[] { typeof(String), typeof(Exception) });

            throw ctor.Invoke(new Object[] { errmsg, innerException }) as T;
        }
    }
}
