//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="dc1394's software">
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ASB2
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Threading;
    using MyLogic;

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        #region フィールド

        /// <summary>
        /// 「平均ベリファイ速度：」の固定文字列
        /// </summary>
        private static readonly String AverageReadSpeedText = "平均ベリファイ速度：";

        /// <summary>
        /// 「平均書き込み速度：」の固定文字列
        /// </summary>
        private static readonly String AverageWriteSpeedText = "平均書き込み速度：";

        /// <summary>
        /// 「ループ終了済」の固定文字列
        /// </summary>
        private static readonly String LoopText = "ループ終了済";

        /// <summary>
        /// 「ベリファイ速度：」の固定文字列
        /// </summary>
        private static readonly String ReadSpeedText = "ベリファイ速度：";

        /// <summary>
        /// 「総書き込みバイト：」の固定文字列
        /// </summary>
        private static readonly String TotalWriteText = "総書き込みバイト：";

        /// <summary>
        /// 「書き込み速度：」の固定文字列
        /// </summary>
        private static readonly String WriteSpeedText = "書き込み速度：";

        /// <summary>
        /// デフォルトの一時ファイル保存ディレクトリ
        /// </summary>
        private static readonly String DefaultDirrectory = Directory.GetCurrentDirectory() + "\\";

        /// <summary>
        /// 単位をギガ（2の30乗）に変換するための定数
        /// </summary>
        private static readonly Int64 Giga = Pow.pow(2L, 30U);

        /// <summary>
        /// 単位をキロ（2の10乗）に変換するための定数
        /// </summary>
        private static readonly Int32 Kilo = (Int32)Pow.pow(2L, 10U);

        /// <summary>
        /// 単位をメガ（2の20乗）に変換するための定数
        /// </summary>
        private static readonly Double Mega = (Double)Pow.pow(2L, 20U);
        
        /// <summary>
        /// 読み込みの平均速度を計算するためのオブジェクト
        /// </summary>
        private AverageIOSpeed.AverageIOSpeed aisread = new AverageIOSpeed.AverageIOSpeed();

        /// <summary>
        /// 書き込みの平均速度を計算するためのオブジェクト
        /// </summary>
        private AverageIOSpeed.AverageIOSpeed aiswrite = new AverageIOSpeed.AverageIOSpeed();

        /// <summary>
        /// 直前に計測した読み込み済みバイト
        /// </summary>
        private Double beforeReadByte = 0.0;

        /// <summary>
        /// ストップウォッチで計測した、直前の経過時間
        /// </summary>
        private Double beforeTime = 0.0;

        /// <summary>
        /// 直前に計測した書き込み済みバイト
        /// </summary>
        private Double beforeWroteByte = 0.0;

        /// <summary>
        /// タイマーオブジェクト
        /// </summary>
        private DispatcherTimer dispatcherTimer = null;

        /// <summary>
        /// 一時ファイル読み書きオブジェクト
        /// </summary>
        private FileIO fio = null;

        /// <summary>
        /// 対応するView
        /// </summary>
        private MainWindowViewModel mwvm;

        /// <summary>
        /// 現在の状態が書き込みモードなのか読み込みモードなのかを示す列挙型変数
        /// </summary>
        private 書込ベリファイ列挙型 書込ベリファイ状態;

        /// <summary>
        /// タスクトレイアイコンオブジェクト
        /// </summary>
        private TaskTrayIcon tti = null;

        /// <summary>
        /// 時間計測用のストップウォッチオブジェクト
        /// </summary>
        private Stopwatch sw = new Stopwatch();

        /// <summary>
        /// 設定情報保存クラスのオブジェクト
        /// </summary>
        private SaveDataManage.SaveDataManage sdm = new SaveDataManage.SaveDataManage();

        #endregion フィールド

        #region 構築

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal MainWindow()
        {
            this.InitializeComponent();
        }

        #endregion 構築

        #region 列挙型

        /// <summary>
        /// 「開始・停止」ボタンのContentの列挙型
        /// </summary>
        private enum ButtonState
        {
            /// <summary>
            /// 開始している状態のとき
            /// </summary>
            開始,

            /// <summary>
            /// 停止している状態のとき
            /// </summary>
            停止
        }

        /// <summary>
        /// 現在の「ウィンドウの」状態が書き込みモードなのか
        /// ベリファイモードなのかを示す列挙型
        /// </summary>
        private enum 書込ベリファイ列挙型
        {
            /// <summary>
            /// 書き込みモード
            /// </summary>
            書込,

            /// <summary>
            /// ベリファイモード
            /// </summary>
            ベリファイ
        }

        #endregion 列挙型

        #region メソッド
        
        /// <summary>
        /// ラップタイムを計算する
        /// </summary>
        /// <returns>ラップタイム</returns>
        private Double CalcLapTime()
        {
            // ストップウォッチの経過時間
            var elapsed = this.sw.Elapsed.TotalSeconds;

            // 前の時間からの経過時間を算出
            var laptime = elapsed - this.beforeTime;
            if (laptime < 0.0)
            {
                // もし0未満なら…
                laptime = elapsed;
            }

            // 前の時間に代入
            this.beforeTime = elapsed;

            return laptime;
        }
        
        /// <summary>
        /// 読み書きの速度（単位はbyte/sec）を計算する
        /// </summary>
        /// <param name="ais">平均読み書き速度計算オブジェクトへの参照</param>
        /// <param name="beforeBytes">前回読み書きされたバイト数</param>
        /// <param name="totalbytes">読み書きされたバイト数の合計</param>
        /// <returns>読み書き速度（単位はbyte/sec）</returns>
        private Double CalcSpeed(ref AverageIOSpeed.AverageIOSpeed ais, ref Double beforeBytes, Double totalbytes)
        {
            // 前の時間から書き込んだバイト数を算出
            var bytes = totalbytes - beforeBytes;
            if (bytes < 0.0)
            {
                // もし0未満なら…
                bytes = totalbytes;
            }

            // 前に書き込んだバイト数に代入
            beforeBytes = totalbytes;

            // 読み書き速度
            var bytespeed = bytes / this.CalcLapTime();

            // 読み書き速度をリストに追加
            ais.Addlist(bytespeed);

            return bytespeed;
        }

        /// <summary>
        /// 指定されたドライブが利用可能で、かつそのドライブに引数で
        /// 指定された値以上の利用可能な空きバイト数が存在するかを調べる
        /// </summary>
        /// <param name="necessarySpace">最低限必要な空きバイト数</param>
        /// <returns>
        /// 指定されたドライブが利用可能で、かつそのドライブの利用可能な
        /// 空きバイト数が十分ならばtrue、そうでないならfalse
        /// </returns>
        private bool DriveAndSpaceCheck(Int64 necessarySpace)
        {
            var drive = new DriveInfo(this.mwvm.TempFileNameFullPath[0].ToString());
            if (drive.IsReady && drive.AvailableFreeSpace >= necessarySpace)
            {
                return true;
            }
            else if (drive.IsReady)
            {
                MyError.CallErrorMessageBox("ドライブの空き容量が不足しています");
                return false;
            }
            else
            {
                MyError.CallErrorMessageBox($"ドライブ{this.mwvm.TempFileNameFullPath[0]}は利用できません。");
                return false;
            }
        }

        /// <summary>
        /// 読み書き処理の終了を行う
        /// それに伴い、UI要素を変更する
        /// </summary>
        /// <param name="titleString">変更後のウィンドウタイトル</param>
        private void DoWriteEnd(String titleString)
        {
            // タイマー停止
            this.dispatcherTimer.Stop();

            // 計測したデータをリセット
            this.aiswrite.Reset();
            this.aisread.Reset();

            // プログレスバーを元に戻す
            this.ProgressBar.Value = 0.0;

            // パーセンテージを0に戻す
            this.ProgressPercentTextBox.Text = "0.0%";

            // 書き込み速度のテキストを0.00 MiB/sに戻す
            this.SpeedTextBlock.Text = MainWindow.WriteSpeedText;

            // ストップウォッチ停止
            this.sw.Reset();

            // タイトル変更
            this.Title = titleString;

            this.TaskTrayIconTextTo待機中();

            // ボタンのテキストを「開始」にする
            this.開始停止Button.Content = MainWindow.ButtonState.開始;

            // WriteFileオブジェクトをGCの対象にする
            this.fio.Dispose();
            this.fio = null;
        }

        /// <summary>
        /// ウィンドウのUI要素を、「書き込みモード」から「ベリファイモード」に変更する
        /// </summary>
        private void From書込toベリファイ()
        {
            // タイトル変更
            this.Title = "ASB2 - 一時ファイルをベリファイ中";

            // タスクトレイのアイコンを右クリックしたときのテキスト変更
            this.tti.Text = "ASB2 一時ファイルをベリファイ中";

            // 状態を「ベリファイモード」にする
            this.書込ベリファイ状態 = MainWindow.書込ベリファイ列挙型.ベリファイ;
        }

        /// <summary>
        /// タスクトレイアイコンを設定する
        /// </summary>
        private void NotifyIcon_Setup()
        {
            this.TaskTrayIconTextTo待機中();

            // アイコンにコンテキストメニューを追加する
            var newcontitemary = new ToolStripMenuItem[4]
            {
                new ToolStripMenuItem("開始", null, new EventHandler(this.RightClick_開始)),
                new ToolStripMenuItem("停止", null, new EventHandler(this.RightClick_停止)),
                new ToolStripMenuItem("開く", null, new EventHandler(this.RightClick_開く)),
                new ToolStripMenuItem("終了", null, new EventHandler(this.RightClick_終了))
            };

            var menuStrip = new ContextMenuStrip();
            menuStrip.Items.AddRange(newcontitemary);

            this.tti.ContextMenuStrip = menuStrip;
        }

        /// <summary>
        /// 実際に一時ファイルの読み書き処理を行う
        /// </summary>
        /// <param name="tmpFileSize">一時ファイルのバイト数（単位はKiB）</param>
        private void PreparationAndStart(Int64 tmpFileSize)
        {
            // タイトル等変更
            this.To書込();

            // プログレスバー設定
            this.ProgressBar.Minimum = 0.0;
            this.ProgressBar.Maximum = (Double)tmpFileSize;

            // ストップウォッチスタート
            this.sw.Start();

            Int32.TryParse(this.sdm.SaveData.BufferSizeText, out Int32 bufsize);

            this.fio = new FileIO(
                bufsize * Kilo,
                this.sdm.SaveData.IsParallel,
                this.mwvm.IsVerify)
            {
                Filename = this.mwvm.TempFileNameFullPath,
                FileSize = tmpFileSize,
                IsLoop = this.mwvm.IsLoop
            };

            // 処理開始
            this.fio.FileIORun();

            Double.TryParse(this.sdm.SaveData.TimerIntervalText, out Double interval);

            // タイマースタート
            this.dispatcherTimer.Interval = TimeSpan.FromMilliseconds(interval);
            this.dispatcherTimer.Start();

            // ボタンのテキストを「停止」にする
            this.開始停止Button.Content = MainWindow.ButtonState.停止.ToString();
        }

        /// <summary>
        /// 処理を開始する
        /// </summary>
        private void Run()
        {
            Int64.TryParse(this.mwvm.TempFileSizeText, out Int64 tmpFileSize);
            tmpFileSize *= MainWindow.Giga;

            if (!this.DriveAndSpaceCheck(tmpFileSize))
            {
                return;
            }

            // 処理スタート
            this.PreparationAndStart(tmpFileSize);
        }

        /// <summary>
        /// 処理を停止する
        /// </summary>
        private void Stop()
        {
            // タイトル変更
            this.Title = "ASB2 - キャンセル待機中...少々お待ちください";

            // FileIOの処理をキャンセルする
            this.fio.Cts.Cancel();
        }

        /// <summary>
        /// タスクトレイのアイコンのテキストを「ASB2 待機中」に変更する
        /// </summary>
        private void TaskTrayIconTextTo待機中()
        {
            // タスクトレイのアイコンを右クリックしたときのテキスト変更
            this.tti.Text = "ASB2 待機中";
        }

        /// <summary>
        /// ウィンドウのUIを、「書き込みモード」に変更する
        /// </summary>
        private void To書込()
        {
            // タイトル変更
            this.Title = "ASB2 - 一時ファイルをディスクに書き込み中";

            // タスクトレイのアイコンを右クリックしたときのテキスト変更
            this.tti.Text = "ASB2 一時ファイルをディスクに書き込み中";

            // 状態を「書込モード」にする
            this.書込ベリファイ状態 = MainWindow.書込ベリファイ列挙型.書込;
        }

        /// <summary>
        /// 「ProgressBar」と「ProgressPercentTextBox」を更新する
        /// </summary>
        /// <param name="bytes">処理されたバイト</param>
        private void UpdateProgress(Double bytes)
        {
            if (this.mwvm.IsVerify)
            {
                bytes *= 0.5;
            }

            this.ProgressBar.Value = bytes;
            this.ProgressPercentTextBox.Text = (bytes / this.ProgressBar.Maximum).ToString("0.0%");
        }

        /// <summary>
        /// 書き込み・読み込みに関わる部分以外のウィンドウの要素を更新する
        /// </summary>
        private void UpdateWindow()
        {
            this.LoopNumTextBlock.Text = FileIO.LoopNum.ToString() + MainWindow.LoopText;
            this.TotalWriteTextBlock.Text = String.Format(MainWindow.TotalWriteText + "{0:N0} Bytes", FileIO.TotalWroteBytes);
        }

        /// <summary>
        /// ウィンドウのUI要素を更新する（ベリファイモード）
        /// </summary>
        /// <param name="readbyte">トータルで読み込まれたバイト</param>
        /// <param name="readbytespeed">読み込み速度（単位はbyte/sec）</param>
        private void UpdateWindowForベリファイ(Double readbyte, Double readbytespeed)
        {
            this.UpdateWindow();

            this.UpdateProgress(this.ProgressBar.Maximum + readbyte);

            this.SpeedTextBlock.Text = String.Format(MainWindow.ReadSpeedText + "{0:0.00} MiB/s", readbytespeed / MainWindow.Mega);
            this.AverageSpeedTextBlock.Text = String.Format(
                MainWindow.AverageReadSpeedText + "{0:0.00} MiB/s",
                this.aisread.Averagespeed() / MainWindow.Mega);
        }

        /// <summary>
        /// ウィンドウのUI要素を更新する（書き込みモード）
        /// </summary>
        /// <param name="writebytespeed">書き込み速度（単位はbyte/sec）</param>
        /// <param name="wrotebyte">トータルで書き込まれたバイト</param>
        private void UpdateWindowFor書込(Double writebytespeed, Double wrotebyte)
        {
            this.UpdateWindow();

            this.UpdateProgress(wrotebyte);
            
            this.SpeedTextBlock.Text = String.Format(MainWindow.WriteSpeedText + "{0:0.00} MiB/s", writebytespeed / Mega);
            this.AverageSpeedTextBlock.Text = String.Format(
                MainWindow.AverageWriteSpeedText + "{0:0.00} MiB/s",
                this.aiswrite.Averagespeed() / Mega);
        }

        #endregion メソッド

        #region イベントハンドラ

        /// <summary>
        /// [設定] - [設定]をクリックしたときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void ApplicationSettinged(object sender, ExecutedRoutedEventArgs e)
        {
            // Instantiate the dialog box
            var ew = new SettingWindow(this.sdm.SaveData)
            {
                Owner = this
            };

            // Open the dialog box modally
            ew.ShowDialog();
        }

        /// <summary>
        /// [ヘルプ] - [バージョン情報]をクリックしたときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void ApplicationVersioned(object sender, ExecutedRoutedEventArgs e)
        {
            // Instantiate the dialog box
            var ew = new VersionInformationWindow()
            {
                Owner = this
            };

            // Open the dialog box modally
            ew.ShowDialog();
        }

        /// <summary>
        /// タイマーのコールバックメソッド
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            switch ((FileIO.動作状態)this.fio.IsNow)
            {
                case FileIO.動作状態.待機中:
                    break;

                case FileIO.動作状態.書込中:
                    switch (this.書込ベリファイ状態)
                    {
                        case MainWindow.書込ベリファイ列挙型.書込:
                            // ウィンドウを更新
                            this.UpdateWindowFor書込(
                                this.CalcSpeed(ref this.aiswrite, ref this.beforeWroteByte, (Double)this.fio.WroteBytes),
                                (Double)this.fio.WroteBytes);
                            break;

                        case MainWindow.書込ベリファイ列挙型.ベリファイ:
                            this.To書込();
                            break;

                        default:
                            Debug.Assert(false, "書込ベリファイ状態があり得ない値になっている！");
                            break;
                    }

                    break;

                case FileIO.動作状態.ベリファイ中:
                    switch (this.書込ベリファイ状態)
                    {
                        case MainWindow.書込ベリファイ列挙型.書込:
                            this.From書込toベリファイ();
                            break;

                        case MainWindow.書込ベリファイ列挙型.ベリファイ:
                            // ウィンドウを更新
                            this.UpdateWindowForベリファイ(
                                (Double)this.fio.ReadBytes,
                                this.CalcSpeed(ref this.aisread, ref this.beforeReadByte, (Double)this.fio.ReadBytes));
                            break;

                        default:
                            Debug.Assert(false, "書込ベリファイ状態があり得ない値になっている！");
                            break;
                    }

                    break;

                case FileIO.動作状態.終了待機中:
                    switch ((FileIO.終了状態)this.fio.ReturnCode)
                    {
                        case FileIO.終了状態.正常終了:
                            this.DoWriteEnd("ASB2 - 正常終了（実行待機中）");
                            break;

                        case FileIO.終了状態.キャンセル終了:
                            this.DoWriteEnd("ASB2 - ユーザーの要求によりキャンセル（実行待機中）");
                            break;

                        case FileIO.終了状態.異常終了:
                            this.DoWriteEnd("ASB2 - IOエラーにより終了（実行待機中）");
                            break;

                        default:
                            Debug.Assert(false, "FileIO.ErrorCodeがありえない値になっている！");
                            break;
                    }

                    break;

                default:
                    Debug.Assert(false, "FileIO.IsNowがありえない値になっている！");
                    break;
            }
        }

        /// <summary>
        /// [ファイル] - [終了]をクリックしたときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void MainWindowClosed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// ウィンドウが閉じる場合に呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // タイマーストップ
            this.dispatcherTimer.Stop();

            if (this.fio != null)
            {
                this.fio.Cts.Cancel();

                this.fio.Dispose();
                
                this.fio = null;
            }

            this.sdm.dataSave();

            if (this.tti != null)
            {
                this.tti.Dispose();

                this.tti = null;
            }
        }

        /// <summary>
        /// ウィンドウを開くときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void MainWindow_Loaded(object sender, EventArgs e)
        {
            SaveDataManage.SaveDataManage.XMLFILENAME = MainWindow.DefaultDirrectory + SaveDataManage.SaveDataManage.XMLFILENAME;

            this.sdm.dataRead();

            this.DataContext = this.mwvm = new MainWindowViewModel(this.sdm.SaveData);

            if (File.Exists(this.sdm.SaveData.TempFilenameFullPath))
            {
                var message = $"以前起動したときの一時ファイルと思われるファイルがあります。{Environment.NewLine}削除しますか？";
                
                if (System.Windows.MessageBox.Show(
                    message,
                    "情報",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    File.Delete(this.sdm.SaveData.TempFilenameFullPath);
                }
            }

            // タイマーを作成する
            this.dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            this.dispatcherTimer.Tick += new EventHandler(this.DispatcherTimer_Tick);

            this.tti = new TaskTrayIcon(this) { SaveData = this.sdm.SaveData };
            this.NotifyIcon_Setup();

            // Iconを設定
            this.tti.Icon = Properties.Resources.app;               // タスクトレイのアイコンの指定
                        
            this.LoopNumTextBlock.Text = "0" + MainWindow.LoopText;
        }

        /// <summary>
        /// タスクトレイアイコンを右クリックして、[開始]を選んだときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void RightClick_開始(object sender, EventArgs e)
        {
            if (this.fio == null)
            {
                this.Run();
            }
        }

        /// <summary>
        /// タスクトレイアイコンを右クリックして、[開く]を選んだときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void RightClick_開く(object sender, EventArgs e)
        {
            this.tti.OpenWindow();
        }

        /// <summary>
        /// タスクトレイアイコンを右クリックして、[停止]を選んだときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void RightClick_停止(object sender, EventArgs e)
        {
            if (this.fio != null)
            {
                this.Stop();
            }
        }

        /// <summary>
        /// タスクトレイアイコンを右クリックして、[修了]を選んだときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void RightClick_終了(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 「デフォルトに戻す」ボタンを押したときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void デフォルトに戻すButton_Click(object sender, RoutedEventArgs e)
        {
            this.mwvm.TempFileNameFullPath = DefaultData.DefaultDataDefinition.DEFAULTTEMPFILENAMEFULLPATH;
            this.mwvm.TempFileSizeText = DefaultData.DefaultDataDefinition.DEFAULTTEMPFILESIZETEXT;
            this.mwvm.IsLoop = DefaultData.DefaultDataDefinition.DEFAULTLOOP;
            this.mwvm.IsVerify = DefaultData.DefaultDataDefinition.DEFAULTVERIFY;
        }

        /// <summary>
        /// 「開始・停止」ボタンを押したときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void 開始停止Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.fio == null)
            {
                this.Run();
            }
            else
            {
                this.Stop();
            }
        }
        
        /// <summary>
        /// 「参照」ボタンを押したときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void 参照Button_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new Microsoft.Win32.SaveFileDialog()
            {
                // はじめのファイル名を指定する
                // はじめに「ファイル名」で表示される文字列を指定する
                FileName = Path.GetFileName(this.sdm.SaveData.TempFilenameFullPath),

                // [ファイルの種類]に表示される選択肢を指定する
                Filter = "一時ファイル(*.tmp;*.temp)|*.tmp;*.temp|すべてのファイル(*.*)|*.*",

                // はじめに表示されるフォルダを指定する
                // 指定しない（空の文字列）の時は、現在のディレクトリが表示される
                InitialDirectory = Path.GetDirectoryName(this.sdm.SaveData.TempFilenameFullPath),

                // タイトルを設定する
                Title = "一時ファイルのファイル名を指定："
            };

            // ダイアログを表示する
            if (sfd.ShowDialog().Value)
            {
                // フルパスを含むファイル名を保存
                this.mwvm.TempFileNameFullPath =
                    this.sdm.SaveData.TempFilenameFullPath = sfd.FileName;
            }
        }

        #endregion イベントハンドラ
    }
}
