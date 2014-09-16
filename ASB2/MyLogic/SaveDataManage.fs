namespace MyLogic

module SaveDataManage =
    open System
    open System.IO
    open System.Runtime.Serialization.Formatters.Binary

    //open DefaultData

    [<Serializable>]
    [<Sealed>]
    type SaveData() =
        let mutable minimize        = DefaultDataDefinition.DEFAULTMINIMIZE
        let mutable bufSizeText     = DefaultDataDefinition.DEFAULTBUFSIZETEXT
        let mutable tmpFileSizeText = DefaultDataDefinition.DEFAULTTMPFILESIZETEXT
        let mutable timerIntervalText = DefaultDataDefinition.DEFAULTTIMERINTERVALTEXT
        let mutable isParallel      = DefaultDataDefinition.DEFAULTPARALLEL
        let mutable isLoop          = DefaultDataDefinition.DEFAULTLOOP
        let mutable isVerify        = DefaultDataDefinition.DEFAULTVERIFY
        let mutable lastTmpFileNameFullPath = DefaultDataDefinition.DEFAULTTMPFILENAMEFULLPATH

        member public this.Minimize
            with get() = minimize;
            and set(value) = minimize <- value
                         
        member public this.BufSizeText
            with get() = bufSizeText
            and set(value) = bufSizeText <- value

        member public this.TmpFileSizeText
            with get() = tmpFileSizeText
            and set(value) = tmpFileSizeText <- value
        
        member public this.TimerIntervalText
            with get() = timerIntervalText
            and set(value) = timerIntervalText <- value

        member public this.IsParallel
            with get() = isParallel
            and set(value) = isParallel <- value

        member public this.IsLoop
            with get() = isLoop
            and set(value) = isLoop <- value

        member public this.IsVerify
            with get() = isVerify
            and set(value) = isVerify <- value

        member public this.LastTmpFileNameFullPath
            with get() = lastTmpFileNameFullPath
            and set(value) = lastTmpFileNameFullPath <- value

    [<Sealed>]
    type SaveDataManage() =
        let mutable sd = new SaveData()

        // asbデータ保存ファイル名
        static member val XMLFILENAME = "asbdata.dat" with get, set

        member public this.SaveData
            with get() = sd
            and set(value) = sd <- value

        /// <summary>
        /// シリアライズされたデータを読み込みし、メモリに保存する
        /// </summary>
        member public this.dataRead() =
            try
                using (new FileStream(SaveDataManage.XMLFILENAME,
                                      FileMode.Open,
                                      FileAccess.Read)) (fun fs ->
                                                         // 逆シリアル化する
                                                         sd <- (new BinaryFormatter()).Deserialize(fs) :?> SaveData)
            with
                | :? FileNotFoundException -> ()
                | _ -> reraise ()

        /// <summary>
        /// データをシリアライズし、ファイルに保存する
        /// </summary>
        member public this.dataSave() =
            try
                using (new FileStream(SaveDataManage.XMLFILENAME,
                                      FileMode.Create,
                                      FileAccess.Write)) (fun fs ->
                                                          //シリアル化して書き込む
                                                          (new BinaryFormatter()).Serialize(fs, sd))
            with
                | e -> CallErrorMessageBox (e.Message + String.Format("{0}データファイルの書き込みに失敗しました。{0}{1}にログを保存しました。",
                                                             Environment.NewLine, ErrorLogFileName))
                       WriteLog e