namespace LibrarySystem

open System
open System.IO
open System.Text.Json

module LibraryFunctions =
    
    let getLoanHistory (patronId: int) : LoanHistory option =
        let jsonFile = sprintf "loans_%d.json" patronId
        let xmlFile = sprintf "loans_%d.xml" patronId
        
        try
            if File.Exists(jsonFile) then
                let json = File.ReadAllText(jsonFile)
                let dto = JsonSerializer.Deserialize<JsonTypes.LoanHistoryDto>(json)
                Some (Conversions.loanHistoryFromDto dto)
            elif File.Exists(xmlFile) then
                None
            else
                None
        with
        | ex -> 
            printfn "Error loading loan history: %s" ex.Message
            None
    
    let isPatronForLongerThan (patron: Patron) (days: int) : bool =
        let today = DateTime.Now
        let difference = today - patron.JoinDate
        difference.Days > days
    
    let addFine (patron: Patron) (amount: decimal<PLN>) : Patron =
        { patron with FinesBalance = patron.FinesBalance + amount }

type Library() =
    let mutable patrons: Map<int, Patron> = Map.empty
    
    member this.LoadPatrons(filePath: string) =
        try
            let json = File.ReadAllText(filePath)
            let dtos = JsonSerializer.Deserialize<JsonTypes.PatronDto list>(json)
            patrons <- 
                dtos 
                |> List.map Conversions.patronFromDto
                |> List.map (fun p -> (p.PatronId, p))
                |> Map.ofList
            printfn "Loaded %d patrons" patrons.Count
        with
        | ex -> printfn "Error loading patrons: %s" ex.Message
    
    member this.GetPatron(patronId: int) : Patron option =
        Map.tryFind patronId patrons
    
    member this.UpdatePatron(patron: Patron) =
        patrons <- Map.add patron.PatronId patron patrons
    
    member this.PromoteToPremium(patronId: int) (minBooks: int) (minDays: int) : bool =
        match this.GetPatron(patronId) with
        | Some patron when patron.Status = Standard ->
            let history = LibraryFunctions.getLoanHistory patronId
            let bookCount = 
                match history with
                | Some h -> h.Loans.Length
                | None -> 0
            
            let hasEnoughBooks = bookCount > minBooks
            let memberLongEnough = LibraryFunctions.isPatronForLongerThan patron minDays
            
            if hasEnoughBooks && memberLongEnough then
                let promotedPatron = { patron with Status = Premium }
                this.UpdatePatron(promotedPatron)
                printfn "Patron %d promoted to Premium!" patronId
                true
            else
                printfn "Patron %d does not meet promotion criteria" patronId
                printfn "  Books borrowed: %d (needs > %d)" bookCount minBooks
                printfn "  Member for %d days (needs > %d)" (DateTime.Now - patron.JoinDate).Days minDays
                false
        | Some patron ->
            printfn "Patron %d is already Premium" patronId
            false
        | None ->
            printfn "Patron %d not found" patronId
            false
    
    member this.GetPatronInfo(patronId: int) : (string * string * string option) option =
        match this.GetPatron(patronId) with
        | Some patron ->
            match patron.ContactDetails with
            | Some details -> Some (details.FirstName, details.LastName, details.Email)
            | None -> None
        | None -> None
    
    member this.AddFineToPatron(patronId: int) (amount: decimal<PLN>) : bool =
        match this.GetPatron(patronId) with
        | Some patron ->
            let updated = LibraryFunctions.addFine patron amount
            this.UpdatePatron(updated)
            printfn "Added %.2f PLN fine to patron %d" (amount / 1.0M<PLN>) patronId
            true
        | None ->
            printfn "Patron %d not found" patronId
            false
    
    member this.CheckFinesAgainstDeposit(patronId: int) (usdToPlnRate: decimal<PLN/USD>) : bool =
        match this.GetPatron(patronId) with
        | Some patron ->
            let depositInPln = patron.Deposit * usdToPlnRate
            let exceeded = patron.FinesBalance > depositInPln
            
            printfn "Patron %d:" patronId
            printfn "  Deposit: %.2f USD (%.2f PLN)" (patron.Deposit / 1.0M<USD>) (depositInPln / 1.0M<PLN>)
            printfn "  Fines: %.2f PLN" (patron.FinesBalance / 1.0M<PLN>)
            printfn "  Status: %s" (if exceeded then "EXCEEDED" else "OK")
            
            not exceeded
        | None ->
            printfn "Patron %d not found" patronId
            false
