namespace ASB2
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;

    using FileMemWork;
    using Microsoft.Win32;
    using MyLogic;

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        #region フィールド

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
        private FileIO wf = null;

        /// <summary>
        /// 対応するView
        /// </summary>
        private MainWindowViewModel mwd;

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

            SaveDataManage.SaveDataManage.XMLFILENAME = DefaultDirrectory + SaveDataManage.SaveDataManage.XMLFILENAME;
            ErrorDispose.ErrorLogFileName = DefaultDirrectory + ErrorDispose.ErrorLogFileName;

            if (File.Exists(ErrorDispose.ErrorLogFileName))
            {
                File.Delete(ErrorDispose.ErrorLogFileName);
            }

            try
            {
                sdm.dataRead();
            }
            catch (InvalidOperationException)
            {
                ErrorDispose.callError(@"データを保存したxmlファイルが壊れています。
xmlファイルを削除してデフォルトデータを読み込みます");
                File.Delete(SaveDataManage.SaveDataManage.XMLFILENAME);
            }
            
            mwd = new MainWindowViewModel(sdm.SaveData);

            if (File.Exists(sdm.SaveData.LastTmpFileNameFullPath))
            {
                if (MessageBox.Show(String.Format(
                    @"以前起動したときの一時ファイルと思われるファイルがあります。{0}削除しますか？
（「いいえ」を押すとアプリケーションを終了します）",
                    Environment.NewLine),
                    "情報", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    File.Delete(sdm.SaveData.LastTmpFileNameFullPath);
                }
                else
                {
                    this.Close();
                }
            }

            this.DataContext = mwd;

            tti = new TaskTrayIcon(this) { SaveData = sdm.SaveData };
            NotifyIcon_Setup();
        }

        #endregion 構築・破棄

        #region イベントハンドラ

        // 「開始」メニューのイベントハンドラ
        private void rightClick_開始(object sender, EventArgs e)
        {
            if (wf == null)
            {
                Run();
            }
        }

        // 「開く」メニューのイベントハンドラ
        private void rightClick_開く(object sender, EventArgs e)
        {
            tti.openWindow();
        }

        // 「停止」メニューのイベントハンドラ
        private void rightClick_停止(object sender, EventArgs e)
        {
            if (wf != null)
            {
                Stop();
            }
        }

        // 「終了」メニューのイベントハンドラ
        private void rightClick_終了(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainWindow_Loaded(object sender, EventArgs e)
        {
            // Iconを設定
            // 現在のコードを実行しているAssemblyを取得する
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            // 指定されたマニフェストリソース(この場合だとアイコンファイル)を読み込む
            this.Icon = BitmapFrame.Create(myAssembly.GetManifestResourceStream("ASB2.Images.app.ico"));    // ソフトウェアの左上のアイコンの指定

            tti.Icon = Properties.Resources.app;                                                            // タスクトレイのアイコンの指定

            // タイマーを作成する
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            sdm.dataSave();

            if (tti != null)
            {
                tti.Dispose();
            }

            if (wf != null)
            {
                wf.Dispose();
            }
        }

        private void MainWindowClosed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void ApplicationSettinged(object sender, ExecutedRoutedEventArgs e)
        {
            // Instantiate the dialog box
            SettingWindow ew = new SettingWindow(sdm.SaveData);

            // Configure the dialog box
            ew.Owner = this;

            // Open the dialog box modally
            ew.ShowDialog();
        }

        private void ApplicationVersioned(object sender, ExecutedRoutedEventArgs e)
        {
            // Instantiate the dialog box
            using (var vif = new VersionInformationForm())
            {
                // Open the dialog box modally
                vif.ShowDialog();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            // はじめのファイル名を指定する
            // はじめに「ファイル名」で表示される文字列を指定する
            sfd.FileName = Path.GetFileName(sdm.SaveData.LastTmpFileNameFullPath);
            // はじめに表示されるフォルダを指定する
            // 指定しない（空の文字列）の時は、現在のディレクトリが表示される
            sfd.InitialDirectory = Path.GetDirectoryName(sdm.SaveData.LastTmpFileNameFullPath);
            // [ファイルの種類]に表示される選択肢を指定する
            sfd.Filter = "一時ファイル(*.tmp;*.temp)|*.tmp;*.temp|すべてのファイル(*.*)|*.*";
            // タイトルを設定する
            sfd.Title = "一時ファイルのファイル名を指定：";

            // ダイアログを表示する
            if (sfd.ShowDialog().Value)
            {
                mwd.TmpFileNameFullPath = sfd.FileName;
                // フルパスを含むファイル名を保存
                sdm.SaveData.LastTmpFileNameFullPath = sfd.FileName;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            mwd.TmpFileNameFullPath = DefaultData.DefaultDataDefinition.DEFAULTTMPFILENAMEFULLPATH;
            mwd.TmpFileSizeText = DefaultData.DefaultDataDefinition.DEFAULTTMPFILESIZETEXT;
            mwd.IsLoop = DefaultData.DefaultDataDefinition.DEFAULTLOOP;
            mwd.IsVerify = DefaultData.DefaultDataDefinition.DEFAULTVERIFY;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (wf == null)
            {
                Run();
            }
            else
            {
                Stop();
            }
        }

        #endregion イベントハンドラ

        #region メソッド

        private void NotifyIcon_Setup()
        {
            tti.Text = "ASB2 待機中";

            // アイコンにコンテキストメニューを追加する
            var menuStrip = new System.Windows.Forms.ContextMenuStrip();
            var newcontitemary = new System.Windows.Forms.ToolStripMenuItem[4];
            for (Int32 i = 0; i < newcontitemary.Length; i++)
            {
                newcontitemary[i] = new System.Windows.Forms.ToolStripMenuItem();
            }
            newcontitemary[0].Text = "開始";
            newcontitemary[0].Click += new EventHandler(rightClick_開始);
            menuStrip.Items.Add(newcontitemary[0]);
            newcontitemary[1].Text = "停止";
            newcontitemary[1].Click += new EventHandler(rightClick_停止);
            menuStrip.Items.Add(newcontitemary[1]);
            newcontitemary[2].Text = "開く";
            newcontitemary[2].Click += new EventHandler(rightClick_開く);
            menuStrip.Items.Add(newcontitemary[2]);
            newcontitemary[3].Text = "終了";
            newcontitemary[3].Click += new EventHandler(rightClick_終了);
            menuStrip.Items.Add(newcontitemary[3]);

            tti.ContextMenuStrip = menuStrip;
        }

        private void Run()
        {
            Int64 tmpFileSize = Int64.Parse(mwd.TmpFileSizeText);
            tmpFileSize *= Giga;

            if (!DriveAndSpaceCheck(tmpFileSize))
                return;

            // 処理スタート
            PreparationAndStart(tmpFileSize);
        }

        private void Stop()
        {
            // タイトル変更
            this.Title = "ASB2 - キャンセル待機中...少々お待ちください";
            // WriteFileをGCの対象にする
            wf.Dispose();
            wf = null;
                        
            // プログレスバーを元に戻す
            progressBar1.Value = 0.0;
            // パーセンテージを0に戻す
            mwd.ProgressPercentText = "0.0%";

            tti.Text = "ASB2 待機中";
            NormalCancelClose();

            // タイトルを変更
            this.Title = "ASB2 - ユーザーの要求によりキャンセル（実行待機中）";
        }

        private bool DriveAndSpaceCheck(Int64 tmpFileSize)
        {
            DriveInfo drive = new DriveInfo(mwd.TmpFileNameFullPath[0].ToString());
            if (drive.IsReady)
            {
                if (drive.AvailableFreeSpace < tmpFileSize)
                {
                    ErrorDispose.callError("ドライブの空き容量が不足しています");
                    return false;
                }
            }
            else
            {
                ErrorDispose.callError(String.Format("ドライブ{0}は利用できません。", mwd.TmpFileNameFullPath[0]));
                return false;
            }

            return true;
        }

        private void PreparationAndStart(Int64 tmpFileSize)
        {
            wf = new FileMemWork.FileIO(Int32.Parse(sdm.SaveData.BufSizeText) * Kilo,
                                        mwd.IsVerify,
                                        sdm.SaveData.IsParallel)
                　{
                     IsLoop = mwd.IsLoop,
                     FileName = mwd.TmpFileNameFullPath,
                     FileSize = tmpFileSize,
                　};

            // プログレスバー設定
            progressBar1.Minimum = 0.0;
            progressBar1.Maximum = (Double)tmpFileSize;

            // 処理開始
            wf.fileIORun();
            
            // ストップウォッチスタート
            sw.Start();

            // タイトル等変更
            Change();

            // タイマースタート
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(Double.Parse(sdm.SaveData.TimerIntervalText));
            dispatcherTimer.Start();
            
            // ボタンのテキストを「停止」にする
            mwd.ButtonName = MainWindowViewModel.ButtonState.停止.ToString();
            
        }

        private void Change()
        {
            // 状態を「Write」にする
            mwd.RWstate = MainWindowViewModel.RWState.Write;
            // タイトル変更
            this.Title = "ASB2 - ディスクに書き込み中";
            // タスクトレイのアイコンを右クリックしたときのテキスト変更
            tti.Text = "ASB2 ディスクに書き込み中";
        }

        private void NormalCancelClose()
        {
            // タイマー停止
            dispatcherTimer.Stop();

            // ストップウォッチ停止
            sw.Reset();

            // 計測したデータをリセット
            aiswrite.Reset();
            aisread.Reset();

            // ボタンのテキストを「開始」にする
            mwd.ButtonName = MainWindowViewModel.ButtonState.開始.ToString();

            // タイトル変更
            this.Title = "ASB2 - 正常終了（実行待機中）";

            // タスクトレイのアイコンを右クリックしたときのテキスト変更
            tti.Text = "ASB2 待機中";

            // WriteFileオブジェクトをGCの対象にする
            if (wf != null)
            {
                wf.Dispose();
                wf = null;
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!wf.AllMemoryWrited && mwd.RWstate != MainWindowViewModel.RWState.Write)
            {
                Change();
            }
            else if (wf.AllMemoryWrited && mwd.IsVerify && mwd.RWstate != MainWindowViewModel.RWState.Read)
            {
                // 状態を「Read」にする
                mwd.RWstate = MainWindowViewModel.RWState.Read;
                // タイトル変更
                this.Title = "ASB2 - ファイルをベリファイ中";
                // タスクトレイのアイコンを右クリックしたときのテキスト変更
                tti.Text = "ASB2 ファイルをベリファイ中";
            }

            if (mwd.RWstate == MainWindowViewModel.RWState.Write)
            {
                // ウィンドウを更新
                UpdateWindowWrite((Double)wf.WroteSize, CalcWriteSpeed());
            }
            else
            {
                // ウィンドウを更新
                UpdateWindowRead((Double)wf.ReadSize, CalcReadSpeed());
            }

            if (wf.Tas.Status == TaskStatus.RanToCompletion)
            {
                NormalCancelClose();
            }
        }

        private Double CalcWriteSpeed()
        {
            // 書き込んだバイト数
            Double totalwrotebyte = (Double)wf.WroteSize;
            // 前の時間から書き込んだバイト数を算出
            Double wrotebyte = totalwrotebyte - beforeWroteByte;
            if (wrotebyte < 0.0)    // もし0未満なら…
            {
                wrotebyte = totalwrotebyte;
            }
            // 前に書き込んだバイト数に代入
            beforeWroteByte = totalwrotebyte;
            
            // 書き込み速度
            Double writebytespeed = wrotebyte / CalcLapTime();

            // 書き込み速度をリストに追加
            aiswrite.Addlist(writebytespeed);

            return writebytespeed;
        }

        private Double CalcReadSpeed()
        {
            // 書き込んだバイト数
            Double totalreadbyte = (Double)wf.ReadSize;
            // 前の時間から書き込んだバイト数を算出
            Double readbyte = totalreadbyte - beforeWroteByte;
            if (readbyte < 0.0)    // もし0未満なら…
            {
                readbyte = totalreadbyte;
            }
            // 前に書き込んだバイト数に代入
            beforeReadByte = totalreadbyte;

            // 書き込み速度
            Double readbytespeed = readbyte / CalcLapTime();

            // 書き込み速度をリストに追加
            aisread.Addlist(readbytespeed);

            return readbytespeed;
        }

        private Double CalcLapTime()
        {
            // ストップウォッチの経過時間
            Double elapsed = sw.Elapsed.TotalSeconds;
            // 前の時間からの経過時間を算出
            Double laptime = elapsed - beforeTime;
            if (laptime < 0.0) // もし0未満なら…
            {
                laptime = elapsed;
            }
            // 前の時間に代入
            beforeTime = elapsed;

            return laptime;
        }

        private void UpdateWindowWrite(Double wrotebyte, Double writebytespeed)
        {
            UpdateWindow();
            if (mwd.IsVerify)
            {
                progressBar1.Value = wrotebyte * 0.5;
                mwd.ProgressPercentText = (wrotebyte / progressBar1.Maximum * 0.5).ToString("0.0%");
            }
            else
            {
                progressBar1.Value = wrotebyte;
                mwd.ProgressPercentText = (wrotebyte / progressBar1.Maximum).ToString("0.0%");
            }
            mwd.SpeedText = String.Format(MainWindowViewModel.WRITESPEEDTEXT + "{0:0.00} MiB/s", writebytespeed / Mega);
            mwd.AverageSpeedText = String.Format(MainWindowViewModel.AVERAGEWRITESPEEDTEXT + "{0:0.00} MiB/s", aiswrite.Averagespeed() / Mega);
        }

        private void UpdateWindowRead(Double readbyte, Double readbytespeed)
        {
            UpdateWindow();
            progressBar1.Value = progressBar1.Maximum * 0.5 + readbyte * 0.5;
            mwd.ProgressPercentText = (readbyte / progressBar1.Maximum * 0.5 + 0.5).ToString("0.0%");
            mwd.SpeedText = String.Format(MainWindowViewModel.READSPEEDTEXT + "{0:0.00} MiB/s", readbytespeed / Mega);
            mwd.AverageSpeedText = String.Format(MainWindowViewModel.AVERAGEREADSPEEDTEXT + "{0:0.00} MiB/s", aisread.Averagespeed() / Mega);
        }

        private void UpdateWindow()
        {
            mwd.LoopNumText = FileIO.LoopNum.ToString() + MainWindowViewModel.LOOPTEXT;
            mwd.TotalWriteText = String.Format(MainWindowViewModel.TOTALWRITETEXT + "{0:N0} Bytes", FileIO.TotalWrote);
        }

        #endregion メソッド
    }
}
