using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;

using MyLogic;

namespace ASB2
{
    public partial class VersionInformationForm : Form
    {
        #region 構築・破棄

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="target"></param>
        internal VersionInformationForm()
        {
            InitializeComponent();
            
            label1.Text += String.Format(@"{0}{0}Aomin SSD Breaker 2 Created by @dc1394
icon @aominpoko", Environment.NewLine);
        }

        #endregion 構築・破棄

        #region メソッド

        private void button1_Click(Object sender, EventArgs e)
        {
            try
            {
                Process.Start("readme.txt");
            }
            catch (Win32Exception)
            {
                MyError.CallErrorMessageBox(@"カレントフォルダにreadme.txtが見つかりません。
更新履歴.txtを削除しないで下さい。");
            }
        }

        #endregion メソッド
    }
}
