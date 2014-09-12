namespace MyLogic

module AverageIOSpeed =
    open System

    [<Sealed>]
    type AverageIOSpeed() =
        let SPEEDRATIO = 3.5
        let MAXLISTSIZE = 1000
        let mutable speedlist = List.empty

        member public this.Addlist (x : Double) =
            speedlist <- x :: speedlist
            let averagespeed = this.Averagespeed()
            speedlist <- List.filter (fun x -> x < averagespeed * SPEEDRATIO) speedlist

            if speedlist.Length >= MAXLISTSIZE then 
                speedlist <- [this.Averagespeed()]

        member public this.Averagespeed() =
            if speedlist.IsEmpty then 0.0
            else List.average(speedlist)

        member public this.Reset() =
            speedlist <- List.empty