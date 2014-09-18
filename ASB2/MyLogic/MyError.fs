namespace MyLogic

module MyError =
    open System
    open System.Collections
    open System.IO
    open System.Reflection
    open System.Text
    open System.Windows
    open log4net
    
    /// <summary>
    /// 例外の詳細をStringBuilderに追加する
    /// </summary>
    /// <param name="ex">例外</param>
    /// <param name="builder">StringBuilder</param>
    let AddExceptionDetail (ex : Exception) (builder : StringBuilder) =
        builder.AppendFormat("Message: {0}{1}", ex.Message, Environment.NewLine) |> ignore
        builder.AppendFormat("Type: {0}{1}", ex.GetType(), Environment.NewLine) |> ignore
        builder.AppendFormat("HelpLink: {0}{1}", ex.HelpLink, Environment.NewLine) |> ignore
        builder.AppendFormat("Source: {0}{1}", ex.Source, Environment.NewLine) |> ignore
        builder.AppendFormat("TargetSite: {0}{1}", ex.TargetSite, Environment.NewLine) |> ignore
        builder.AppendFormat("Data: {0}", Environment.NewLine) |> ignore
        ex.Data |> Seq.cast<DictionaryEntry>
                |> Seq.iter (fun x -> builder.AppendFormat("\t{0} : {1}{2}", x.Key, x.Value, Environment.NewLine)
                                      |> ignore)


        builder.AppendFormat("StackTrace: {0}{1}", ex.StackTrace, Environment.NewLine) |> ignore

    /// <summary>
    /// エラーメッセージボックスを表示する
    /// </summary>
    /// <param name="errMsg">エラーメッセージ</param>
    let CallErrorMessageBox errMsg =
        MessageBox.Show(
            errMsg,
            "エラー",
            MessageBoxButton.OK,
            MessageBoxImage.Error) |> ignore
    
    /// <summary>
    /// ログを記録するファイル名
    /// </summary>
    let ErrorLogFilename = "Errorlog-file.txt"

    /// <summary>
    /// ネストされた例外を列挙子にいれて返す
    /// </summary>
    /// <param name="ex">例外</param>
    /// <returns>例外が入った列挙子</returns>
    let GetNestedExceptionList (ex : Exception) =
        let current = ref ex
        let loop = ref true
        seq { while !loop do
                current := current.contents.InnerException
                if !current <> null then yield current
                elif !current = null then loop := false
            }
    
    /// <summary>
    /// log4netインスタンス（ログ記録用）
    /// </summary>
    let Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

    /// <summary>
    /// 例外の詳細をStringBuilderに追加して、それをStringに変換して返す
    /// </summary>
    /// <param name="ex">例外</param>
    /// <returns>例外の詳細の文字列</returns>
    let ToFullDisplayString ex =
        let displayText = new StringBuilder()
        for inner in (GetNestedExceptionList ex) do
            displayText.AppendFormat("**** 内部例外 始点 **** {0}", Environment.NewLine) |> ignore
            AddExceptionDetail !inner displayText 
            displayText.AppendFormat("**** 内部例外 終点 **** {0}{0}", Environment.NewLine) |> ignore

        displayText.ToString()

    /// <summary>
    /// 与えられたエラーメッセージをログファイルに書き込み、
    /// 与えられたエラーメッセージで与えられた型の例外を投げる
    /// </summary>
    /// <typeparam name="T">例外の型</typeparam>
    /// <param name="errmsg">エラーメッセージ</param>
    let WriteAndThrow<'T> errmsg innerException =
        Log.Error(errmsg);
        let ctor = typeof<'T>.GetConstructor([| typeof<String>; typeof<Exception>; |])

        raise (ctor.Invoke([| errmsg; innerException |]) :?> Exception)

    /// <summary>
    /// 例外をエラーとしてファイルに記録する
    /// </summary>
    /// <param name="ex">例外</param>
    let WriteLog (ex : Exception) =
        Log.Error(ex.ToString() + Environment.NewLine + ToFullDisplayString ex)
