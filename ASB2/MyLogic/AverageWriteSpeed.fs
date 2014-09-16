namespace MyLogic

module AverageIOSpeed =
    open System

    /// <summary>
    /// 読み書き速度の平均を求めるクラス
    /// </summary>
    [<Sealed>]
    type AverageIOSpeed() =
        /// <summary>
        /// 異常値を判別するための閾値
        /// </summary>
        let SPEEDRATIO = 3.5
        
        /// <summary>
        /// 平均を求めるときのサンプル数
        /// </summary>
        let MAXLISTSIZE = 1000
        
        /// <summary>
        /// 測定値を格納するList
        /// </summary>
        let mutable speedlist = List.empty

        /// <summary>
        /// 測定値をListに収納する
        /// </summary>
        /// <param name="x">測定値</param>
        member public this.Addlist (x : Double) =
            speedlist <- x :: speedlist
            let averagespeed = this.Averagespeed()
            speedlist <- List.filter (fun x -> x < averagespeed * SPEEDRATIO) speedlist

            if speedlist.Length >= MAXLISTSIZE then 
                speedlist <- speedlist.Tail @ [this.Averagespeed()]

        /// <summary>
        /// 平均速度を求める
        /// </summary>
        member public this.Averagespeed() =
            if speedlist.IsEmpty then 0.0
            else List.average(speedlist)

        /// <summary>
        /// 測定値の格納されたListを空にする
        /// </summary>
        member public this.Reset() =
            speedlist <- List.empty