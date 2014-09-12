namespace MyLogic

module ErrorDispose =
    open System
    open System.IO
    open System.Text
    open System.Windows
    open System.Collections

    let mutable ErrorLogFileName = "ASBError.log"

    let callError errMsg =
        MessageBox.Show(errMsg, "エラー",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error) |> ignore

    let GetNestedExceptionList (ex : Exception) =
        let current = ref ex
        let loop = ref true
        seq { while !loop do
                current := current.contents.InnerException
                if !current <> null then yield current
                elif !current = null then loop := false
            }

    let WriteExceptionDetail (builder : StringBuilder) (ex : Exception) =
        builder.AppendFormat("Message: {0}{1}", ex.Message, Environment.NewLine) |> ignore
        builder.AppendFormat("Type: {0}{1}", ex.GetType(), Environment.NewLine) |> ignore
        builder.AppendFormat("HelpLink: {0}{1}", ex.HelpLink, Environment.NewLine) |> ignore
        builder.AppendFormat("Source: {0}{1}", ex.Source, Environment.NewLine) |> ignore
        builder.AppendFormat("TargetSite: {0}{1}", ex.TargetSite, Environment.NewLine) |> ignore
        builder.AppendFormat("Data: {0}", Environment.NewLine) |> ignore
        for de in ex.Data |> Seq.cast<DictionaryEntry> do
            builder.AppendFormat("\t{0} : {1}{2}", de.Key, de.Value, Environment.NewLine) |> ignore
        done
        builder.AppendFormat("StackTrace: {0}{1}", ex.StackTrace, Environment.NewLine) |> ignore
    
    let ToFullDisplayString (ex : Exception) =
        let displayText = new StringBuilder()
        for inner in (GetNestedExceptionList ex) do
            displayText.AppendFormat("**** 内部例外 始点 **** {0}", Environment.NewLine) |> ignore
            WriteExceptionDetail displayText !inner
            displayText.AppendFormat("**** 内部例外 終点 **** {0}{0}", Environment.NewLine) |> ignore

        displayText.ToString()

    let WriteLog (ex : Exception) =
        try
            using (new StreamWriter(ErrorLogFileName, true, Text.Encoding.GetEncoding("Shift_JIS"))) (fun writer ->
                   writer.Write(ex.ToString() + Environment.NewLine + ToFullDisplayString(ex)))
        with
        | _ -> callError "ログファイルを保存できませんでした。"

