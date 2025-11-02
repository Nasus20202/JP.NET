#r "nuget: FSharp.Collections.ParallelSeq"
open FSharp.Collections.ParallelSeq

let data = [1..10]
let inline sqr x = x * x

let sumOfSquaresI nums =
    let mutable acc = 0
    for x in nums do
        acc <- acc + sqr x
    acc
printfn "Result of sumOfSquaresI: %d" (sumOfSquaresI data)

let rec sumOfSquaresF nums =
    match nums with
    | [] -> 0
    | h::t -> sqr h + sumOfSquaresF t
printfn "Result of sumOfSquaresF: %d" (sumOfSquaresF data)

let sumOfSquares nums =
    nums
    |> Seq.map(sqr)
    |> Seq.sum
printfn "Result of sumOfSquares: %d" (sumOfSquares data)

let sumOfSquaresP nums =
    nums
    |> PSeq.map(sqr)
    |> PSeq.sum
printfn "Result of sumOfSquaresP: %d" (sumOfSquaresP data)
