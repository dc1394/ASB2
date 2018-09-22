//-----------------------------------------------------------------------
// <copyright file="VersionInformationWindow.xaml.cs" company="dc1394's software">
//     Copyright © 2014-2018 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ASB2
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Navigation;
    using MyLogic;

    /// <summary>
    /// VersionInformationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class VersionInformationWindow
    {
        #region 構築

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal VersionInformationWindow()
        {
            this.InitializeComponent();
        }

        #endregion 構築

        #region イベントハンドラ

        /// <summary>
        /// Twitterの私のページに対するハイパーリンクをクリックしたとき呼び出される
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        /// <summary>
        /// 「readme.txtを見る」ボタンをクリックしたとき呼び出される
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void OpenTextFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("readme.txt");
            }
            catch (Win32Exception)
            {
                MyError.CallErrorMessageBox($"カレントフォルダにreadme.txtが見つかりません。{Environment.NewLine}readme.txtを削除しないで下さい。");
            }
        }

        #endregion イベントハンドラ
    }
}
