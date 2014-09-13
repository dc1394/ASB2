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
        private String tmpFileNameFullPath;

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

            this.tmpFileNameFullPath = sd.LastTmpFileNameFullPath;
            
            this.tmpFileSizeText = sd.TmpFileSizeText;
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
            get
            {
                return this.isVerify;
            }

            set
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
                return this.tmpFileNameFullPath;
            }

            set
            {
                this.SetProperty(ref this.tmpFileNameFullPath, value);
                this.sd.LastTmpFileNameFullPath = value;
            }
        }

        /// <summary>
        /// ドライブに書き出す一時ファイルのバイト数
        /// </summary>
        public String TmpFileSizeText
        {
            get
            {
                return this.tmpFileSizeText;
            }

            set
            {
                this.SetProperty(ref this.tmpFileSizeText, value);
                this.sd.TmpFileSizeText = value;
            }
        }

        #endregion プロパティ
    }
}
