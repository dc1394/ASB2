//-----------------------------------------------------------------------
// <copyright file="SettingWindow.xaml.cs" company="dc1394's software">
//     Copyright © 2014-2018 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ASB2
{
    using System;
    using System.Windows;
    using MyLogic;
    
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        #region フィールド

        /// <summary>
        /// 保存された設定情報のオブジェクト
        /// </summary>
        private readonly SaveDataManage.SaveData sd;

        /// <summary>
        /// SettingWindowに対応するView
        /// </summary>
        private SettingViewModel svm;

        #endregion フィールド

        #region 構築

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sd">保存された設定情報のオブジェクト</param>
        internal SettingWindow(SaveDataManage.SaveData sd)
        {
            this.InitializeComponent();

            this.sd = sd;
        }

        #endregion 構築

        #region イベントハンドラ

        /// <summary>
        /// 「キャンセル」ボタンをクリックしたとき呼び出される
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 「デフォルトに戻す」ボタンをクリックしたとき呼び出される
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void Default_Button_Click(object sender, RoutedEventArgs e)
        {
            this.BufferSizeTextBox.Text = DefaultData.DefaultDataDefinition.DEFAULTBUFSIZETEXT;
            
            this.svm.MyType = DefaultData.DefaultDataDefinition.DEFAULTMINIMIZE;

            this.TimerIntervalTextBox.Text = DefaultData.DefaultDataDefinition.DEFAULTTIMERINTERVALTEXT;

            if (this.sd.IsVerify)
            {
                this.IsParallelCheckBox.IsChecked = DefaultData.DefaultDataDefinition.DEFAULTPARALLEL;
            }
        }

        /// <summary>
        /// 「OK」ボタンをクリックしたとき呼び出される
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            this.sd.BufferSizeText = this.BufferSizeTextBox.Text;

            this.sd.IsParallel = this.IsParallelCheckBox.IsChecked ?? false;

            this.sd.Minimize = this.svm.MyType;

            this.sd.TimerIntervalText = this.TimerIntervalTextBox.Text;

            this.Close();
        }

        /// <summary>
        /// ウィンドウがロードされるとき呼び出される
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SettingWindow_Loaded(object sender, EventArgs e)
        {
            this.DataContext = this.svm = new SettingViewModel(this.sd);

            this.BufferSizeTextBox.Text = this.sd.BufferSizeText;

            this.IsParallelCheckBox.IsChecked = this.sd.IsParallel;

            this.sd.Minimize = this.svm.MyType;

            this.TimerIntervalTextBox.Text = this.sd.TimerIntervalText;
        }

        #endregion イベントハンドラ
    }
}
