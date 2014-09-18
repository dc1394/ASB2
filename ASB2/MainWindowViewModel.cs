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
    internal sealed class MainWindowViewModel : MyViewModelBase.BindableBase
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
        private String tmpFileSizeText;

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
            
            this.tmpFileSizeText = sd.TempFileSizeText;
        }

        #endregion 構築

        #region プロパティ

        /// <summary>
        /// ループするかどうかを示すフラグ
        /// </summary>
        public Boolean IsLoop
        {
            get
            {
                return this.isLoop;
            }

            internal set
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
            get
            {
                return this.isVerify;
            }

            internal set
            {
                this.SetProperty(ref this.isVerify, value);
                this.sd.IsVerify = value;
            }
        }

        /// <summary>
        /// ドライブに書き出す一時ファイル名のフルパス
        /// </summary>
        public String TmpFileNameFullPath
        {
            get
            {
                return this.tempFilenameFullPath;
            }

            internal set
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
            get
            {
                return this.tmpFileSizeText;
            }

            internal set
            {
                this.SetProperty(ref this.tmpFileSizeText, value);
                this.sd.TempFileSizeText = value;
            }
        }

        #endregion プロパティ
    }
}
