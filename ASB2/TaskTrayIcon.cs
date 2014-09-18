//-----------------------------------------------------------------------
// <copyright file="TaskTrayIcon.cs" company="dc1394's software">
//     but this is originally adapted by ほげたん
//     cf. http://hogetan.blogspot.jp/2008/10/blog-post.html
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ASB2
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Forms;
    using MyLogic;

    /// <summary>
    /// タスクトレイにアイコンを表示するためのヘルパークラス
    /// <para> 
    /// コンストラクタで指定したウィンドウに接続して以下の動作を実現します。
    /// ・ウィンドウが最小化された場合にタスクトレイにバルーンチップを表示
    /// ・バルーンチップまたはアイコンクリックでウィンドウ再表示（通常表示）
    /// ・ウィンドウ非表示中はタスクトレイにアイコンを表示
    /// ・ウィンドウ表示中はタスクトレイアイコン非表示
    /// </para>
    /// このクラスを利用するためには以下のDLLを参照設定に追加する必要があります。
    ///  - System.Drawing
    ///  - System.Windows.Forms
    /// <para>
    /// NotifyIconクラスから派生させるのが簡単だが sealed クラスで派生できないためラップしています。
    /// </para>
    /// </summary>
    internal sealed class TaskTrayIcon : IDisposable
    {
        #region フィールド

        /// <summary>
        /// タスクトレイに表示するアイコン
        /// </summary>
        private NotifyIcon notifyIcon = new NotifyIcon();

        /// <summary>
        /// 接続しているウィンドウ
        /// </summary>
        private Window targetWindow;

        /// <summary>
        /// 接続しているウィンドウの表示状態
        /// </summary>
        private WindowState storedWindowState;

        #endregion フィールド

        #region 構築

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="target">ターゲットのウィンドウ</param>
        internal TaskTrayIcon(Window target)
        {
            this.BalloonTipTitle = "ASB2";
            this.BalloonTipText = "タスクトレイに常駐します";
            this.notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            this.notifyIcon.MouseClick += this.NotifyIcon_MouseClick;

            // 接続先ウィンドウ
            this.targetWindow = target;
            this.storedWindowState = this.targetWindow.WindowState;

            // ウィンドウに接続
            if (this.targetWindow != null)
            {
                this.targetWindow.Closing += new CancelEventHandler(this.Target_Closing);
                this.targetWindow.IsVisibleChanged += new DependencyPropertyChangedEventHandler(this.Target_IsVisibleChanged);
                this.targetWindow.StateChanged += new EventHandler(this.Target_StateChanged);
            }
        }

        #endregion 構築

        #region プロパティ

        /// <summary>
        /// アイコンを右クリックすると出現するメニュー
        /// </summary>
        internal ContextMenuStrip ContextMenuStrip
        {
            set
            {
                this.notifyIcon.ContextMenuStrip = value;
            }
        }

        /// <summary>
        /// タスクトレイに表示するアイコン
        /// </summary>
        internal Icon Icon
        {
            set
            {
                this.notifyIcon.Icon = value;
            }
        }

        /// <summary>
        /// 保存された設定情報のオブジェクト
        /// </summary>
        internal SaveDataManage.SaveData SaveData { private get; set; }

        /// <summary>
        /// アイコンのテキスト
        /// 改行文字を使って複数行のテキストを表示できます。
        /// </summary>
        internal string Text
        {
            set
            {
                this.notifyIcon.Text = value;
            }
        }

        /// <summary>
        /// バルーンチップに表示するテキスト
        /// 改行文字を使って複数行のテキストを表示できます。
        /// </summary>
        private string BalloonTipText
        {
            set
            {
                this.notifyIcon.BalloonTipText = value;
            }
        }

        /// <summary>
        /// バルーンチップを表示する時間（ミリ秒）
        /// </summary>
        private Int32 BalloonTipTimeout { get; set; }

        /// <summary>
        /// バルーンチップに表示するタイトル
        /// 文字列中に含まれる改行文字は無視します。
        /// </summary>
        private string BalloonTipTitle
        {
            set
            {
                this.notifyIcon.BalloonTipTitle = value;
            }
        }

        #endregion プロパティ

        #region 破棄

        /// <summary>
        /// Disposeメソッド
        /// </summary>
        public void Dispose()
        {
            if (this.notifyIcon != null)
            {
                this.notifyIcon.Dispose();
            }

            // ウィンドウから切断
            if (this.targetWindow != null)
            {
                this.targetWindow.Closing -= new CancelEventHandler(this.Target_Closing);
                this.targetWindow.StateChanged -= new EventHandler(this.Target_StateChanged);
                this.targetWindow.IsVisibleChanged -=
                    new DependencyPropertyChangedEventHandler(this.Target_IsVisibleChanged);
                this.targetWindow = null;
            }
        }

        #endregion 破棄

        #region メソッド

        /// <summary>
        /// ウィンドウを開く
        /// </summary>
        internal void OpenWindow()
        {
            switch (this.SaveData.Minimize)
            {
                case DefaultData.MinimizeType.TASKTRAY:
                    this.targetWindow.Show();
                    break;

                default:
                    this.notifyIcon.Visible = false;
                    break;
            }

            this.targetWindow.WindowState = this.storedWindowState;
        }

        #endregion メソッド

        #region イベントハンドラ

        /// <summary>
        /// 接続先ウィンドウの可視状態が変化した
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void Target_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.notifyIcon != null)
            {
                this.notifyIcon.Visible = !this.targetWindow.IsVisible;
            }
        }

        /// <summary>
        /// 接続先ウィンドウの表示状態が変化した
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void Target_StateChanged(object sender, EventArgs e)
        {
            switch (this.targetWindow.WindowState)
            {
                case WindowState.Minimized:
                    switch (this.SaveData.Minimize)
                    {
                         case DefaultData.MinimizeType.TASKBAR:
                             break;

                         case DefaultData.MinimizeType.TASKTRAY:
                             this.targetWindow.Hide();
                     
                             if (this.notifyIcon != null)
                             {
                                 this.notifyIcon.ShowBalloonTip(this.BalloonTipTimeout);
                             }

                             break;

                         case DefaultData.MinimizeType.BOTH:
                             this.notifyIcon.Visible = true;
                     
                             if (this.notifyIcon != null)
                             {
                                 this.notifyIcon.ShowBalloonTip(this.BalloonTipTimeout);
                             }
                     
                             break;

                         default:
                             Debug.Assert(false, "SaveData.Minimizeが異常！");
                             break;
                    }

                    break;
            
                case WindowState.Normal:
                    switch (this.SaveData.Minimize)
                    {
                        case DefaultData.MinimizeType.BOTH:
                            this.notifyIcon.Visible = false;
                            this.storedWindowState = this.targetWindow.WindowState;
                            break;

                        default:
                            break;
                    }

                    break;

                default:
                    this.storedWindowState = this.targetWindow.WindowState;
                    break;
            }
        }

        /// <summary>
        /// 接続先ウィンドウが閉じられた
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void Target_Closing(object sender, CancelEventArgs e)
        {
            this.notifyIcon.Dispose();
            this.notifyIcon = null;
        }

        /// <summary>
        /// タスクトレイでアイコンがクリックされた
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">NotifyIconのクリックイベントのハンドラ</param>
        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    switch (this.SaveData.Minimize)
                    {
                        case DefaultData.MinimizeType.TASKTRAY:
                            this.targetWindow.Show();
                            break;

                        default:
                            this.notifyIcon.Visible = false;
                            break;
                    }

                    this.targetWindow.WindowState = this.storedWindowState;

                    break;
            }
        }

        #endregion イベントハンドラ
    }
}
