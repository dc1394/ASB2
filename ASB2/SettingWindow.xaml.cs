using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

using MyLogic;


namespace ASB2
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        #region フィールド

        private SettingViewModel sd;
        private SaveDataManage.SaveData sdmsd;

        #endregion フィールド

        #region 構築・破棄

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="target"></param>
        internal SettingWindow(SaveDataManage.SaveData sdmsd)
        {
            InitializeComponent();
            this.sdmsd = sdmsd;
            sd = new SettingViewModel(sdmsd);
            this.DataContext = sd;
        }

        #endregion 構築・破棄

        #region イベントハンドラ

        private void MainWindow_Loaded(object sender, EventArgs e)
        {
            // Iconを設定
            // 現在のコードを実行しているAssemblyを取得する
            Assembly myAssembly = Assembly.GetExecutingAssembly();

            // 指定されたマニフェストリソース(この場合だとアイコンファイル)を読み込む
            this.Icon = BitmapFrame.Create(myAssembly.GetManifestResourceStream("ASB2.Images.app.ico"));    // ソフトウェアの左上のアイコンの指定
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            sdmsd.Minimize = sd.MyType;
            sdmsd.BufSizeText = sd.BufSizeText;
            sdmsd.TimerIntervalText = sd.TimerIntervalText;
            sdmsd.IsParallel = sd.IsParallel;
            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            sd.MyType = DefaultData.DefaultDataDefinition.DEFAULTMINIMIZE;
            sd.BufSizeText = DefaultData.DefaultDataDefinition.DEFAULTBUFSIZETEXT;
            sd.TimerIntervalText = DefaultData.DefaultDataDefinition.DEFAULTTIMERINTERVALTEXT;
            sd.IsParallel = DefaultData.DefaultDataDefinition.DEFAULTPARALLEL;
        }

        #endregion イベントハンドラ
    }
}
