﻿module AngouriMath.Interactive.Graphs

open AngouriMath.FSharp.Compilation
open Plotly.NET
open AngouriMath.FSharp.Core

exception InvalidInput

let displayFunc (range : 'T seq) (func : obj) =
    let entity = parsed func
    let vars = entity.Vars |> List.ofSeq
    match vars with
    | theOnlyVar::[] ->
        let compiled = compiled1In<'T, double> theOnlyVar func
        let xData = List.ofSeq range
        let yData = List.map compiled xData
        Chart.Point (xData, yData)
    | _ -> raise InvalidInput
