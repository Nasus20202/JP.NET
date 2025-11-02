#r "nuget: XPlot.Plotly"
#r "nuget: FSharp.Collections.ParallelSeq"
open System.Net.Http
open XPlot.Plotly
open FSharp.Collections.ParallelSeq

let loadPricesAsync(ticker: string) = async {
    let stooqTicker = $"{ticker}.us"

    let dataStart = System.DateTime(2019, 1, 1).ToString("yyyyMMdd")
    let dataEnd = System.DateTime.Now.ToString("yyyyMMdd")

    let url = sprintf "https://stooq.com/q/d/l/?s=%s&d1=%s&d2=%s&i=d" stooqTicker dataStart dataEnd

    let client = new HttpClient()
    let! data = Async.AwaitTask (client.GetStringAsync(url))

    printf "Retrieved %s stock price:\n%s" ticker data

    let prices = 
        data.Split([|'\r'; '\n'|], System.StringSplitOptions.RemoveEmptyEntries)
        |> PSeq.ofArray
        |> PSeq.skip 1
        |> PSeq.map (fun line -> line.Split(','))
        |> PSeq.filter (fun values -> values.Length = 6)
        |> PSeq.map (fun values -> values.[0], System.Double.Parse(values.[4]))
        |> PSeq.toList
    return prices
}

let requests = 
    [
        loadPricesAsync "DT" // Good
        loadPricesAsync "DDOG" // Bad
    ]

let parallelRequests = Async.Parallel requests
let results = Async.RunSynchronously parallelRequests
results |> Array.iter (fun data -> 
    data
    |> Chart.Line
    |> Chart.Show
)
