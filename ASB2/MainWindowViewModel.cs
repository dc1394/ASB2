using System;
using System.IO;
using System.ComponentModel;

using MyLogic;

namespace ASB2
{
    internal sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        #region 列挙型

        internal enum ButtonState
        {
            開始,
            停止
        }

        internal enum RWState
        {
            Write,
            Read
        }

        #endregion 列挙型

        #region フィールド

        public const String LOOPTEXT = "ループ終了済";
        public const String TOTALWRITETEXT = "総書き込みバイト：";
        public const String WRITESPEEDTEXT = "書き込み速度：";
        public const String READSPEEDTEXT = "ベリファイ速度：";
        public const String AVERAGEWRITESPEEDTEXT = "平均書き込み速度：";
        public const String AVERAGEREADSPEEDTEXT = "平均ベリファイ速度：";

        private String tmpFileNameFullPath;
        private String buttonName = ButtonState.開始.ToString();
        private String tmpFileSizeText;
        private Boolean isLoop;
        private Boolean isVerify;
        private String progressPercentText = "";
        private String loopNumText = "0" + MainWindowViewModel.LOOPTEXT;
        private String totalWriteText = MainWindowViewModel.TOTALWRITETEXT;
        private String writeSpeedText = MainWindowViewModel.WRITESPEEDTEXT;
        private String readSpeedText = MainWindowViewModel.READSPEEDTEXT;
        private String averageWriteSpeedText = MainWindowViewModel.AVERAGEWRITESPEEDTEXT;
        private String averageReadSpeedText = MainWindowViewModel.AVERAGEREADSPEEDTEXT;

        SaveDataManage.SaveData sd;

        #endregion フィールド

        #region 構築・破棄

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="target"></param>
        internal MainWindowViewModel(SaveDataManage.SaveData sd)
        {
            this.sd = sd;
            tmpFileNameFullPath = sd.LastTmpFileNameFullPath;
            tmpFileSizeText = sd.TmpFileSizeText;
            isLoop = sd.IsLoop;
            isVerify = sd.IsVerify;
        }

        #endregion 構築・破棄

        #region プロパティ

        public String TmpFileNameFullPath
        {
            get { return tmpFileNameFullPath; }
            set
            {
                tmpFileNameFullPath = value;
                sd.LastTmpFileNameFullPath = value;
                OnPropertyChanged("TmpFileNameFullPath");
            }
        }
                
        public String ButtonName
        {
            get { return buttonName; }
            set
            {
                buttonName = value;
                OnPropertyChanged("ButtonName");
            }
        }
                
        public String TmpFileSizeText
        {
            get { return tmpFileSizeText; }
            set
            {
                tmpFileSizeText = value;
                sd.TmpFileSizeText = value;
                OnPropertyChanged("TmpFileSizeText");
            }
        }
        
        public Boolean IsLoop
        {
            get { return isLoop; }
            set
            {
                isLoop = value;
                sd.IsLoop = value;
                OnPropertyChanged("IsLoop");
            }
        }
                
        public Boolean IsVerify
        {
            get { return isVerify; }
            set
            {
                isVerify = value;
                sd.IsVerify = value;
                OnPropertyChanged("IsVerify");
            }
        }

        public String ProgressPercentText
        {
            get { return progressPercentText; }
            set
            {
                progressPercentText = value;
                OnPropertyChanged("ProgressPercentText");
            }
        }
        
        public String LoopNumText
        {
            get { return loopNumText; }
            set
            {
                loopNumText = value;
                OnPropertyChanged("LoopNumText");
            }
        }
        
        public String TotalWriteText
        {
            get { return totalWriteText; }
            set
            {
                totalWriteText = value;
                OnPropertyChanged("totalWriteText");
            }
        }
        
        public String SpeedText
        {
            get
            {
                if (RWstate == RWState.Write)
                {
                    return writeSpeedText;
                }
                else
                {
                    return readSpeedText;
                }
            }
            set
            {
                if (RWstate == RWState.Write)
                {
                    writeSpeedText = value;
                }
                else
                {
                    readSpeedText = value;
                }
                OnPropertyChanged("SpeedText");
            }
        }
        
        public String AverageSpeedText
        {
            get
            {
                if (RWstate == RWState.Write)
                {
                    return averageWriteSpeedText;
                }
                else
                {
                    return averageReadSpeedText;
                }
            }
            set
            {
                if (RWstate == RWState.Write)
                {
                    averageWriteSpeedText = value;
                }
                else
                {
                    averageReadSpeedText = value;
                }
                OnPropertyChanged("AverageSpeedText");
            }
        }

        public RWState RWstate { get; set; }

        #endregion プロパティ

        #region INotifyPropertyChanged メンバー

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
