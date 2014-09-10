// 参考ソースコード：http://hogetan.blogspot.jp/2008/10/blog-post.html
// Arranged by dc1394

using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;

using MyLogic;

namespace ASB2
{
    /// <summary>
    /// タスクトレイにアイコンを表示するためのヘルパークラス
    /// 
    /// コンストラクタで指定したウィンドウに接続して以下の動作を実現します。
    /// ・ウィンドウが最小化された場合にタスクトレイにバルーンチップを表示
    /// ・バルーンチップまたはアイコンクリックでウィンドウ再表示（通常表示）
    /// ・ウィンドウ非表示中はタスクトレイにアイコンを表示
    /// ・ウィンドウ表示中はタスクトレイアイコン非表示
    /// 
    /// このクラスを利用するためには以下のDLLを参照設定に追加する必要があります。
    ///  - System.Drawing
    ///  - System.Windows.Forms
    /// 
    /// NotifyIconクラスから派生させるのが簡単だが sealed クラスで派生できないためラップしています。
    /// </summary>
    internal sealed class TaskTrayIcon : IDisposable
    {
        #region フィールド

        /// <summary>
        /// タスクトレイに表示するアイコン
        /// </summary>
        private NotifyIcon m_NotifyIcon = new NotifyIcon();

        /// <summary>
        /// 接続しているウィンドウ
        /// </summary>
        private Window m_TargetWindow;

        /// <summary>
        /// 接続しているウィンドウの表示状態
        /// </summary>
        private WindowState m_StoredWindowState;

        #endregion フィールド

        #region 構築・破棄

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="target"></param>
        internal TaskTrayIcon(Window target)
        {
            BalloonTipTitle = "ASB2";
            BalloonTipText = "タスクトレイに常駐します";
            m_NotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            m_NotifyIcon.MouseClick += m_notifyIcon_MouseClick;

            // 接続先ウィンドウ
            m_TargetWindow = target;
            m_StoredWindowState = m_TargetWindow.WindowState;

            // ウィンドウに接続
            if (m_TargetWindow != null)
            {
                m_TargetWindow.Closing += new System.ComponentModel.CancelEventHandler(target_Closing);
                m_TargetWindow.StateChanged += new EventHandler(target_StateChanged);
                m_TargetWindow.IsVisibleChanged += new DependencyPropertyChangedEventHandler(target_IsVisibleChanged);
            }
        }

        #region IDisposable メンバ

        public void Dispose()
        {
            if (m_NotifyIcon != null)
            {
                m_NotifyIcon.Dispose();
            }

            // ウィンドウから切断
            if (m_TargetWindow != null)
            {
                m_TargetWindow.Closing -= new System.ComponentModel.CancelEventHandler(target_Closing);
                m_TargetWindow.StateChanged -= new EventHandler(target_StateChanged);
                m_TargetWindow.IsVisibleChanged -= new DependencyPropertyChangedEventHandler(target_IsVisibleChanged);
                m_TargetWindow = null;
            }
        }

        #endregion

        #endregion 構築・破棄

        #region プロパティ

        /// <summary>
        /// アイコンのテキスト
        /// 改行文字を使って複数行のテキストを表示できます。
        /// </summary>
        internal string Text
        {
            set
            {
                m_NotifyIcon.Text = value;
            }
        }

        /// <summary>
        /// タスクトレイに表示するアイコン
        /// </summary>
        internal Icon Icon
        {
            set
            {
                m_NotifyIcon.Icon = value;
            }
        }

        /// <summary>
        /// バルーンチップに表示するタイトル
        /// 文字列中に含まれる改行文字は無視します。
        /// </summary>
        internal string BalloonTipTitle
        {
            set
            {
                m_NotifyIcon.BalloonTipTitle = value;
            }
        }

        /// <summary>
        /// バルーンチップに表示するテキスト
        /// 改行文字を使って複数行のテキストを表示できます。
        /// </summary>
        internal string BalloonTipText
        {
            set
            {
                m_NotifyIcon.BalloonTipText = value;
            }
        }

        /// <summary>
        /// アイコンを右クリックすると出現するメニュー
        /// </summary>
        public ContextMenuStrip ContextMenuStrip
        {
            set
            {
                m_NotifyIcon.ContextMenuStrip = value;
            }
        }

        /// <summary>
        /// バルーンチップを表示する時間（ミリ秒）
        /// </summary>
        internal Int32 BalloonTipTimeout
        { private get; set; }

        internal SaveDataManage.SaveData SaveData
        { private get; set; }

        #endregion プロパティ

        #region イベントハンドラ

        /// <summary>
        /// 接続先ウィンドウの可視状態が変化した
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void target_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (m_NotifyIcon != null)
            {
                m_NotifyIcon.Visible = !m_TargetWindow.IsVisible;
            }
        }

        /// <summary>
        /// 接続先ウィンドウの表示状態が変化した
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void target_StateChanged(object sender, EventArgs e)
        {
            if (m_TargetWindow.WindowState == WindowState.Minimized)
            {
                switch (SaveData.Minimize)
                {
                    case DefaultData.MinimizeType.TASKBAR:
                        break;
                    case DefaultData.MinimizeType.TASKTRAY:
                        m_TargetWindow.Hide();
                        if (m_NotifyIcon != null)
                        {
                            m_NotifyIcon.ShowBalloonTip(BalloonTipTimeout);
                        }
                        break;
                    case DefaultData.MinimizeType.BOTH:
                        m_NotifyIcon.Visible = true;
                        if (m_NotifyIcon != null)
                        {
                            m_NotifyIcon.ShowBalloonTip(BalloonTipTimeout);
                        }
                        break;
                    default:
                        Debug.Assert(false, "何かがおかしい！！");
                        break;
                }
            }
            else if (m_TargetWindow.WindowState == WindowState.Normal && SaveData.Minimize == DefaultData.MinimizeType.BOTH)
            {
                m_NotifyIcon.Visible = false;
                m_StoredWindowState = m_TargetWindow.WindowState;
            }
            else
            {
                m_StoredWindowState = m_TargetWindow.WindowState;
            }
        }

        /// <summary>
        /// 接続先ウィンドウが閉じられた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void target_Closing(object sender, CancelEventArgs e)
        {
            m_NotifyIcon.Dispose();
            m_NotifyIcon = null;
        }

        /// <summary>
        /// タスクトレイでアイコンがクリックされた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // NotifyIconのクリックイベントのハンドラ
        private void m_notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (SaveData.Minimize == DefaultData.MinimizeType.TASKTRAY)
                {
                    m_TargetWindow.Show();
                }
                else
                {
                    m_NotifyIcon.Visible = false;
                }
                m_TargetWindow.WindowState = m_StoredWindowState;
            }
        }

        #endregion イベントハンドラ

        #region メソッド

        internal void openWindow()
        {
            if (SaveData.Minimize == DefaultData.MinimizeType.TASKTRAY)
            {
                m_TargetWindow.Show();
            }
            else
            {
                m_NotifyIcon.Visible = false;
            }
            m_TargetWindow.WindowState = m_StoredWindowState;
        }

        #endregion メソッド
    }
}
