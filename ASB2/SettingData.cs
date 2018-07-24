using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MyLogic;

namespace ASB2
{
    internal sealed class SettingData : INotifyPropertyChanged
    {
        #region フィールド

        private DefaultData.MinimizeType myType;
        private String bufSizeText;
        private String timerIntervalText;
        private Boolean isParallel;
        private Boolean isParallelEnabled;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion フィールド

        #region 構築・破棄

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="target"></param>
        internal SettingData(SaveDataManage.SaveData sd)
        {
            this.myType = sd.Minimize;
            bufSizeText = sd.BufSizeText;
            timerIntervalText = sd.TimerIntervalText;
            isParallel = sd.IsParallel;
            isParallelEnabled = sd.IsVerify;
        }

        #endregion 構築・破棄

        #region プロパティ

        public DefaultData.MinimizeType MyType
        {
            get { return myType; }
            set
            {
                myType = value;
                PropertyChanged("MyType");
            }
        }

        public String DefaultBufSize
        {
            get { return DefaultData.DefaultDataDefinition.DEFAULTBUFSIZETEXT; }
        }
          
        public String BufSizeText
        {
            get { return bufSizeText; }
            set
            {
                bufSizeText = value;
                PropertyChanged("BufSizeText");
            }
        }

        public String DefaultTimerInterval
        {
            get { return DefaultData.DefaultDataDefinition.DEFAULTTIMERINTERVALTEXT; }
        }

        public String TimerIntervalText
        {
            get { return timerIntervalText; }
            set
            {
                timerIntervalText = value;
                PropertyChanged("TimerIntervalText");
            }
        }
        
        public Boolean IsParallel
        {
            get { return isParallel; }
            set
            {
                isParallel = value;
                PropertyChanged("IsParallel");
            }
        }

        public Boolean IsParallelEnabled
        {
            get { return isParallelEnabled; }
        }

		#endregion プロパティ

		#region INotifyPropertyChanged メンバー

        private void OnPropertyChanged(String name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion INotifyPropertyChanged メンバー
    }
}
