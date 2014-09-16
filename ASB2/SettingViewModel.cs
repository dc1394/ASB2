//-----------------------------------------------------------------------
// <copyright file="SettingViewModel.cs" company="dc1394's software">
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ASB2
{
    using MyLogic;
    using System;
    using System.ComponentModel;

    /// <summary>
    /// SettingWindowに対応するView
    /// </summary>
    internal sealed class SettingViewModel : MyViewModelBase.BindableBase
    {

        #region フィールド

        /// <summary>
        /// バッファサイズの文字列
        /// </summary>
        private String bufferSizeText;

        /// <summary>
        /// 並列化を有効にするかどうか
        /// </summary>
        private Boolean isParallel;

        /// <summary>
        /// 最小化のときの状態
        /// </summary>
        private DefaultData.MinimizeType myType;

        /// <summary>
        /// タイマの更新間隔
        /// </summary>
        private String timerIntervalText;

        #endregion フィールド

        #region 構築

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sd">セーブデータ</param>
        internal SettingViewModel(SaveDataManage.SaveData sd)
        {
            this.bufferSizeText = sd.BufSizeText;

            this.myType = sd.Minimize;

            this.isParallel = sd.IsParallel;

            this.timerIntervalText = sd.TimerIntervalText;
        }

        #endregion 構築

        #region プロパティ

        public String DefaultBufSize
        {
            get { return DefaultData.DefaultDataDefinition.DEFAULTBUFSIZETEXT; }
        }

        /// <summary>
        /// バッファサイズの文字列
        /// </summary>
        public String BufferSizeText
        {
            get
            {
                return this.bufferSizeText;
            }

            set
            {
                this.SetProperty(ref this.bufferSizeText, value);
            }
        }

        public String DefaultTimerInterval
        {
            get { return DefaultData.DefaultDataDefinition.DEFAULTTIMERINTERVALTEXT; }
        }

        /// <summary>
        /// タイマの更新間隔
        /// </summary>
        public String TimerIntervalText
        {
            get
            {
                return timerIntervalText;
            }

            set
            {
                this.SetProperty(ref this.timerIntervalText, value);
            }
        }

        /// <summary>
        /// 最小化のときの状態
        /// </summary>
        public DefaultData.MinimizeType MyType
        {
            get
            {
                return this.myType;
            }

            set
            {
                this.SetProperty(ref this.myType, value);
            }
        }

        #endregion プロパティ

    }
}
