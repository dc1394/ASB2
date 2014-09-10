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
        /// バッファのサイズの文字列
        /// </summary>
        private String bufSizeText;

        /// <summary>
        /// 並列化をサポートしているかどうか
        /// </summary>
        private Boolean isParallel;

        /// <summary>
        /// 並列化を有効にしているかどうか
        /// </summary>
        private Boolean isParallelEnabled;

        /// <summary>
        /// 最小化のときの状態
        /// </summary>
        private DefaultData.MinimizeType myType;

        /// <summary>
        /// タイマの更新間隔
        /// </summary>
        private String timerIntervalText;
        
        #endregion フィールド

        #region 構築・破棄

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sd">セーブデータ</param>
        internal SettingViewModel(SaveDataManage.SaveData sd)
        {
            this.bufSizeText = sd.BufSizeText;

            this.myType = sd.Minimize;

            this.isParallel = sd.IsParallel;

            this.isParallelEnabled = sd.IsVerify;

            this.timerIntervalText = sd.TimerIntervalText;
            
        }

        #endregion 構築・破棄

        #region プロパティ

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

        public String DefaultBufSize
        {
            get { return DefaultData.DefaultDataDefinition.DEFAULTBUFSIZETEXT; }
        }
          
        public String BufSizeText
        {
            get
            {
                return this.bufSizeText;
            }

            set
            {
                this.SetProperty(ref this.bufSizeText, value);
            }
        }

        public String DefaultTimerInterval
        {
            get { return DefaultData.DefaultDataDefinition.DEFAULTTIMERINTERVALTEXT; }
        }

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
        
        public Boolean IsParallel
        {
            get { return isParallel; }
            set
            {
                this.SetProperty(ref this.isParallel, value);
            }
        }

        public Boolean IsParallelEnabled
        {
            get { return isParallelEnabled; }
        }

		#endregion プロパティ

    }
}
