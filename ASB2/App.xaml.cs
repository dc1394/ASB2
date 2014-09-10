using System;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

using MyLogic;

namespace ASB2
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        #region フィールド

        private static Mutex mutex = null;

        #endregion フィールド

        #region イベントハンドラ

        private void OnStartup(object sender, StartupEventArgs e)
        {
            // 名前を指定してMutexを生成
            mutex = new Mutex(false, "ASB2 by dc1394");

            // 二重起動をチェック
            if (!mutex.WaitOne(0, false))
            {
                // 二重起動の場合はエラーを表示して終了
                MessageBox.Show("すでに起動されています。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Mutexを破棄
                mutex.Close();
                mutex = null;

                // 起動を中止してプログラムを終了
                this.Shutdown();
            }

            // WPF UIスレッドでの未処理例外
            this.DispatcherUnhandledException +=
                new DispatcherUnhandledExceptionEventHandler(Application_DispatcherUnhandledException);
        }

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

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // 例外が処理されたことを指示
            e.Handled = true;
            ReportUnhandledException(e.Exception);
        }

        #endregion イベントハンドラ

        #region メソッド

        private void ReportUnhandledException(Exception ex)
        {
            ErrorDispose.callError(ex.Message +
                String.Format("{0}未処理の例外です。{0}{1}にログを保存しました。{0}アプリケーションを終了します。",
                              Environment.NewLine, ErrorDispose.ErrorLogFileName));
            ErrorDispose.WriteLog(ex);
            this.Shutdown();
        }

        #endregion メソッド
    }
}
