﻿//-----------------------------------------------------------------------
// <copyright file="SettingViewModel.cs" company="dc1394's software">
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ASB2
{
    using System;
    using System.ComponentModel;
    using MyLogic;

    /// <summary>
    /// SettingWindowに対応するView
    /// </summary>
    internal sealed class SettingViewModel : ASB2.BindableBase
    {
        #region フィールド

        /// <summary>
        /// メモリサイズの文字列
        /// </summary>
        private String bufferSizeText;

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
        /// <param name="sd">保存された設定情報のオブジェクト</param>
        internal SettingViewModel(SaveDataManage.SaveData sd)
        {
            this.bufferSizeText = sd.BufferSizeText;

            Int32.TryParse(DefaultData.DefaultDataDefinition.DEFAULTBUFSIZETEXT, out Int32 defaultBufferSize);

            this.DefaultBufferSize = defaultBufferSize;

            Int32.TryParse(DefaultData.DefaultDataDefinition.DEFAULTTIMERINTERVALTEXT, out Int32 defaultTimerInterval);

            this.DefaultTimerInterval = defaultTimerInterval;

            this.myType = sd.Minimize;

            this.timerIntervalText = sd.TimerIntervalText;
        }

        #endregion 構築

        #region プロパティ

        /// <summary>
        /// バッファサイズの文字列
        /// </summary>
        public String BufferSizeText
        {
            get => this.bufferSizeText;

            set => this.SetProperty(ref this.bufferSizeText, value);
        }

        /// <summary>
        /// デフォルトのバッファサイズ
        /// </summary>
        public Int32 DefaultBufferSize { get; }

        /// <summary>
        /// デフォルトのタイマの更新間隔
        /// </summary>
        public Int32 DefaultTimerInterval { get; }

        /// <summary>
        /// タイマの更新間隔
        /// </summary>
        public String TimerIntervalText
        {
            get => this.timerIntervalText;

            set => this.SetProperty(ref this.timerIntervalText, value);
        }

        /// <summary>
        /// 最小化のときの状態
        /// </summary>
        public DefaultData.MinimizeType MyType
        {
            get => this.myType;

            set => this.SetProperty(ref this.myType, value);
        }

        #endregion プロパティ
    }
}
