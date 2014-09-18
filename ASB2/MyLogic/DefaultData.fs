namespace MyLogic

module DefaultData =
    open System
    
    /// <summary>
    /// 最小化時の挙動を表す列挙型
    /// </summary>
    [<Serializable>]
    type MinimizeType =
         | TASKBAR   = 0
         | TASKTRAY  = 1
         | BOTH      = 2

    
    /// <summary>
    /// ASB2のデフォルト値を集めた構造体
    /// </summary>
    type DefaultDataDefinition =
        struct
            /// <summary>
            /// デフォルトのバッファサイズ（kiB）
            /// </summary>
            static member DEFAULTBUFSIZETEXT = "1024"

            /// <summary>
            // デフォルトでループしない
            /// </summary>
            static member DEFAULTLOOP = false

            /// <summary>
            /// ASB最小化時動作のデフォルト設定
            /// </summary>
            static member DEFAULTMINIMIZE = MinimizeType.BOTH

            /// <summary>
            // デフォルトで並列化しない
            /// </summary>
            static member DEFAULTPARALLEL = false
            
            /// <summary>
            // デフォルトの更新間隔（ミリ秒）
            /// </summary>
            static member DEFAULTTIMERINTERVALTEXT = "150"

            /// <summary>
            // デフォルトの一時データ保存ファイル名（フルパスを含む）
            /// </summary>
            static member DEFAULTTEMPFILENAMEFULLPATH = Environment.ExpandEnvironmentVariables("%TEMP%\\asb.temp")
            
            /// <summary>
            /// 一時ファイルのデフォルトサイズ(GiB)
            /// </summary>
            static member DEFAULTTEMPFILESIZETEXT = "3"
            
            /// <summary>
            // デフォルトでベリファイしない
            /// </summary>
            static member DEFAULTVERIFY = false
        end