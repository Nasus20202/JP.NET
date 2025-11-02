#r "nuget: XPlot.Plotly"
open System.Net.Http
open XPlot.Plotly

let loadPrices(ticker: string) =
    let stooqTicker = $"{ticker}.us"

    let dataStart = System.DateTime(2019, 1, 1).ToString("yyyyMMdd")
    let dataEnd = System.DateTime.Now.ToString("yyyyMMdd")

    let url = sprintf "https://stooq.com/q/d/l/?s=%s&d1=%s&d2=%s&i=d" stooqTicker dataStart dataEnd

    let client = new HttpClient()
    let getDataAsync = async {
        let! data = Async.AwaitTask (client.GetStringAsync(url))
        return data
    }
    let data = Async.RunSynchronously getDataAsync

    printf "Retrieved %s stock price:\n%s" ticker data

    let prices = 
        data.Split([|'\r'; '\n'|], System.StringSplitOptions.RemoveEmptyEntries)
        |> Seq.skip 1
        |> Seq.map (fun line -> line.Split(','))
        |> Seq.filter (fun values -> values.Length = 6)
        |> Seq.map (fun values -> values.[0], System.Double.Parse(values.[4]))
        |> Seq.toList
    prices


// DT good, DDOG bad
["DT"; "DDOG"] |> Seq.iter(fun ticker -> 
    let prices = loadPrices ticker
    prices
    |> Chart.Line
    |> Chart.WithTitle (sprintf "Stock Prices for %s" ticker)
    |> Chart.Show
)
