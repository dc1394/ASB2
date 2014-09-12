namespace MyLogic

module DefaultData =
    open System

    [<Serializable>]
    type MinimizeType =
         | TASKBAR   = 0
         | TASKTRAY  = 1
         | BOTH      = 2

    type DefaultDataDefinition =
        struct
            // asb最小化時動作のデフォルト設定
            static member DEFAULTMINIMIZE = MinimizeType.BOTH
            // デフォルトのバッファサイズ（kiB）
            static member DEFAULTBUFSIZETEXT = "1024"
            // 一時ファイルのデフォルトサイズ(GiB)
            static member DEFAULTTMPFILESIZETEXT = "3"
            // デフォルトの更新間隔（ミリ秒）
            static member DEFAULTTIMERINTERVALTEXT = "150"
             // デフォルトで並列化しない
            static member DEFAULTPARALLEL = false
            // デフォルトの一時データ保存ファイル名（フルパスを含む）
            static member DEFAULTTMPFILENAMEFULLPATH = Environment.ExpandEnvironmentVariables("%TEMP%\\asb.temp")
            // デフォルトでループしない
            static member DEFAULTLOOP = false
            // デフォルトでベリファイしない
            static member DEFAULTVERIFY = false
        end