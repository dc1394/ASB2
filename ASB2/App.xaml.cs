//-----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="dc1394's software">
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ASB2
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;
    using MyLogic;

    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App
    {
        #region フィールド

        /// <summary>
        /// エラーメッセージその1
        /// </summary>
        private static readonly String ErrorMessage1 = @"{0}未処理の例外です。{0}{1}にログを保存しました。{0}
おそらくプログラムのバグです。";

        /// <summary>
        /// エラーメッセージその2
        /// </summary>
        private static readonly String ErrorMessage2 = @"{0}@dc1394が責任を持って修正しますので、
ログファイル: {1}を添付の上、Twitterの@dc1394宛にご連絡下さい。";

        /// <summary>
        /// エラーメッセージその3
        /// </summary>
        private static readonly String ErrorMessage3 = @"{0}アプリケーションを終了します。";

        /// <summary>
        /// 二重起動確認用のMutex
        /// </summary>
        private static Mutex mutex;

        #endregion フィールド

        #region メソッド

        /// <summary>
        /// 例外からメッセージボックスに表示するエラー文字列を生成する
        /// </summary>
        /// <param name="ex">受け取る例外</param>
        /// <returns>メッセージボックスに表示するエラー文字列</returns>
        private String CreateErrorMessage(Exception ex)
        {
            return ex.Message +
                   String.Format(
                        App.ErrorMessage1,
                        Environment.NewLine,
                        Path.GetFullPath(MyError.ErrorLogFilename)) +
                   String.Format(
                        App.ErrorMessage2,
                        Environment.NewLine,
                        Path.GetFullPath(MyError.ErrorLogFilename)) +
                   String.Format(ErrorMessage3, Environment.NewLine);
        }

        /// <summary>
        /// 例外を受け取り、例外の内容からエラー文字列を生成
        /// そのエラー文字列をメッセージボックスで表示する
        /// さらに、その例外の内容をログに書き込む
        /// アプリケーションを強制終了する
        /// </summary>
        /// <param name="ex">受け取る例外</param>
        private void ReportUnhandledException(Exception ex)
        {
            // 例外の内容からエラー文字列を生成
            // 生成したエラー文字列の内容をメッセージボックスで表示
            MyError.CallErrorMessageBox(this.CreateErrorMessage(ex));        

            // 例外の内容をログに書き込む
            MyError.WriteLog(ex);

            this.Shutdown();
        }

        #endregion メソッド

        #region イベントハンドラ

        /// <summary>
        /// 未処理の例外を受け取り、処理する
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // 例外が処理されたことを指示
            e.Handled = true;
            this.ReportUnhandledException(e.Exception);
        }

        /// <summary>
        /// アプリケーションを開始したときに呼び出される
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            // 名前を指定してMutexを生成
            mutex = new Mutex(false, "ASB2 by @dc1394");

            // 二重起動をチェック
            if (!mutex.WaitOne(0, false))
            {
                // 二重起動の場合はエラーを表示して終了
                MessageBox.Show(
                    "すでに起動されています。",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                // Mutexを破棄
                mutex.Close();
                mutex = null;

                // 起動を中止してプログラムを終了
                this.Shutdown();
            }

            // WPF UIスレッドでの未処理例外
            this.DispatcherUnhandledException +=
                this.Application_DispatcherUnhandledException;
        }

        /// <summary>
        /// アプリケーションを終了したときに呼び出される
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void OnExit(object sender, ExitEventArgs e)
        {
            if (mutex != null)
            {
                // Mutexを解放
                mutex.ReleaseMutex();

                // Mutexを破棄
                mutex.Close();
                mutex = null;
            }
        }

        #endregion イベントハンドラ
    }
}
