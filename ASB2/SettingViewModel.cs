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
        /// 最小化のときの状態
        /// </summary>
        private DefaultData.MinimizeType myType;
        
        #endregion フィールド

        #region 構築・破棄

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sd">セーブデータ</param>
        internal SettingViewModel(SaveDataManage.SaveData sd)
        {
            this.myType = sd.Minimize;
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
        
		#endregion プロパティ

    }
}
