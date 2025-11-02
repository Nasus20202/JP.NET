Imports System
Imports Lab6.Stocks

Module Program
    Async Function GetStocks() As Task
        Dim tickers = {"DT", "DDOG", "MSFT", "ORCL", "EBAY", "GOOG", "AAPL", "C"}
        Dim days = 365

        Dim analyzers = Await StockAnalyzer.GetAnalyzersParallelTask(tickers, days)

        For Each analyzer In analyzers
            Console.WriteLine($"StdDev = {analyzer.StdDev,8:F4} | Return = {analyzer.Return,8:F4}")
        Next
    End Function

    Sub Main()
        GetStocks().GetAwaiter().GetResult()
    End Sub
End Module
