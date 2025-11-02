namespace LibrarySystem

open System
open System.Net.Http
open System.Text.RegularExpressions

module CurrencyExchange =
    
    let private fetchRate (url: string) : decimal option =
        use client = new HttpClient()
        try
            let response = client.GetStringAsync(url).Result
            let lines = response.Split([|'\n'|], StringSplitOptions.RemoveEmptyEntries)
            if lines.Length >= 2 then
                let dataLine = lines.[1]
                let fields = dataLine.Split(',')
                if fields.Length >= 7 then
                    match Decimal.TryParse(fields.[6]) with
                    | (true, rate) -> Some rate
                    | _ -> None
                else
                    None
            else
                None
        with
        | ex -> 
            printfn "Error fetching rate: %s" ex.Message
            None
    
    let getUsdToPlnRate() : decimal<PLN/USD> =
        let url = "https://stooq.pl/q/l/?s=usdpln&f=sd2t2ohlcv&h&e=csv"
        match fetchRate url with
        | Some rate -> rate * 1.0M<PLN/USD>
        | None -> 
            printfn "Failed to fetch USD/PLN rate, using default: 4.0"
            4.0M<PLN/USD>
    
    let getEurToPlnRate() : decimal<PLN/EUR> =
        let url = "https://stooq.pl/q/l/?s=eurpln&f=sd2t2ohlcv&h&e=csv"
        match fetchRate url with
        | Some rate -> rate * 1.0M<PLN/EUR>
        | None -> 
            printfn "Failed to fetch EUR/PLN rate, using default: 4.3"
            4.3M<PLN/EUR>
    
    let getPlnToEurRate() : decimal<EUR/PLN> =
        let url = "https://stooq.pl/q/l/?s=plneur&f=sd2t2ohlcv&h&e=csv"
        match fetchRate url with
        | Some rate -> rate * 1.0M<EUR/PLN>
        | None -> 
            printfn "Failed to fetch PLN/EUR rate, using default: 0.23"
            0.23M<EUR/PLN>
    
    let convertUsdToPln (amount: decimal<USD>) (rate: decimal<PLN/USD>) : decimal<PLN> =
        amount * rate
    
    let convertPlnToEur (amount: decimal<PLN>) (rate: decimal<EUR/PLN>) : decimal<EUR> =
        amount * rate
    
    let convertEurToPln (amount: decimal<EUR>) (rate: decimal<PLN/EUR>) : decimal<PLN> =
        amount * rate
