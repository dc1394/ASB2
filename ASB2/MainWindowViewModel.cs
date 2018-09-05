//-----------------------------------------------------------------------
// <copyright file="MainWindowViewModel.cs" company="dc1394's software">
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ASB2
{
    using System;
    using MyLogic;

    /// <summary>
    /// MainWindowに対応するView
    /// </summary>
    internal sealed class MainWindowViewModel : ASB2.BindableBase
    {
        #region フィールド

        /// <summary>
        /// ループするかどうかを示すフラグ
        /// </summary>
        private Boolean isLoop;

        /// <summary>
        /// ベリファイするかどうかを示すフラグ
        /// </summary>
        private Boolean isVerify;

        /// <summary>
        /// ファイルに保存される設定情報データのオブジェクト
        /// </summary>
        private SaveDataManage.SaveData sd;

        /// <summary>
        /// ドライブに書き出す一時ファイル名のフルパス
        /// </summary>
        private String tempFilenameFullPath;

        /// <summary>
        /// ドライブに書き出す一時ファイルのバイト数
        /// </summary>
        private String tempFileSizeText;

        #endregion フィールド

        #region 構築

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sd">ファイルに保存される設定情報データのオブジェクト</param>
        internal MainWindowViewModel(SaveDataManage.SaveData sd)
        {
            this.isLoop = sd.IsLoop;

            this.isVerify = sd.IsVerify;

            this.sd = sd;

            this.tempFilenameFullPath = sd.TempFilenameFullPath;
            
            this.tempFileSizeText = sd.TempFileSizeText;
        }

        #endregion 構築

        #region プロパティ

        /// <summary>
        /// ループするかどうかを示すフラグ
        /// </summary>
        public Boolean IsLoop
        {
            get => this.isLoop;

            set
            {
                this.SetProperty(ref this.isLoop, value);
                this.sd.IsLoop = value;
            }
        }

        /// <summary>
        /// ベリファイするかどうかを示すフラグ
        /// </summary>
        public Boolean IsVerify
        {
            get => this.isVerify;

            set
            {
                this.SetProperty(ref this.isVerify, value);
                this.sd.IsVerify = value;
            }
        }

        /// <summary>
        /// ドライブに書き出す一時ファイル名のフルパス
        /// </summary>
        public String TempFilenameFullPath
        {
            get => this.tempFilenameFullPath;

            set
            {
                this.SetProperty(ref this.tempFilenameFullPath, value);
                this.sd.TempFilenameFullPath = value;
            }
        }

        /// <summary>
        /// ドライブに書き出す一時ファイルのバイト数
        /// </summary>
        public String TempFileSizeText
        {
            get => this.tempFileSizeText;

            set
            {
                this.SetProperty(ref this.tempFileSizeText, value);
                this.sd.TempFileSizeText = value;
            }
        }

        #endregion プロパティ
    }
}
