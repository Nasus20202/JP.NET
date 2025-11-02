open System
open System.IO
open System.Reflection
open LibrarySystem
open LibrarySystem.CurrencyExchange
open LibrarySystem.LibraryFunctions

let getAppDirectory() =
    let assembly = Assembly.GetExecutingAssembly()
    Path.GetDirectoryName(assembly.Location)

let printMenu() =
    printfn "\n========================================="
    printfn "    LIBRARY SYSTEM - MENU"
    printfn "========================================="
    printfn "1. Display patron information"
    printfn "2. Add fine to patron account"
    printfn "3. Check fines against deposit"
    printfn "4. Promote patron to Premium"
    printfn "5. Display loan history"
    printfn "6. Display all patrons"
    printfn "7. Fetch currency rates"
    printfn "0. Exit"
    printfn "========================================="
    printf "Choose option: "

let readInt() =
    match Int32.TryParse(Console.ReadLine()) with
    | (true, value) -> Some value
    | _ -> None

let readDecimal() =
    match Decimal.TryParse(Console.ReadLine()) with
    | (true, value) -> Some value
    | _ -> None

let displayPatronInfo (library: Library) =
    printf "Enter patron ID: "
    match readInt() with
    | Some patronId ->
        match library.GetPatronInfo(patronId) with
        | Some (firstName, lastName, email) ->
            printfn "\n--- Patron Information ---"
            printfn "First Name: %s" firstName
            printfn "Last Name: %s" lastName
            match email with
            | Some e -> printfn "Email: %s" e
            | None -> printfn "Email: [none]"
        | None ->
            printfn "No contact details found for patron %d" patronId
    | None ->
        printfn "Invalid number"

let addFineToPatron (library: Library) =
    printf "Enter patron ID: "
    match readInt() with
    | Some patronId ->
        printf "Enter fine amount (PLN): "
        match readDecimal() with
        | Some amount ->
            let fine = amount * 1.0M<PLN>
            if library.AddFineToPatron patronId fine then
                printfn "Fine added successfully"
            else
                printfn "Failed to add fine"
        | None ->
            printfn "Invalid amount"
    | None ->
        printfn "Invalid number"

let checkFinesAgainstDeposit (library: Library) (usdToPlnRate: decimal<PLN/USD>) =
    printf "Enter patron ID: "
    match readInt() with
    | Some patronId ->
        printfn "\nChecking fines against deposit..."
        library.CheckFinesAgainstDeposit patronId usdToPlnRate |> ignore
    | None ->
        printfn "Invalid number"

let promotePatron (library: Library) =
    printf "Enter patron ID: "
    match readInt() with
    | Some patronId ->
        printf "Minimum number of borrowed books: "
        match readInt() with
        | Some minBooks ->
            printf "Minimum number of membership days: "
            match readInt() with
            | Some minDays ->
                library.PromoteToPremium patronId minBooks minDays |> ignore
            | None ->
                printfn "Invalid number of days"
        | None ->
            printfn "Invalid number of books"
    | None ->
        printfn "Invalid number"

let displayLoanHistory (patronId: int) =
    match getLoanHistory patronId with
    | Some history ->
        printfn "\n--- Loan history for patron %d ---" patronId
        printfn "Number of loans: %d" history.Loans.Length
        history.Loans 
        |> List.iteri (fun i (isbn, returned) ->
            let status = if returned then "[RETURNED]" else "[BORROWED]"
            printfn "%d. ISBN: %s %s" (i+1) isbn status
        )
    | None ->
        printfn "No loan history found for patron %d" patronId

let displayAllPatrons (library: Library) =
    printfn "\n--- List of patrons ---"
    for i in 1..10 do
        match library.GetPatron(i) with
        | Some patron ->
            let statusStr = 
                match patron.Status with
                | Premium -> "Premium"
                | Standard -> "Standard"
            
            printfn "\nPatron ID: %d" patron.PatronId
            printfn "  Status: %s" statusStr
            printfn "  Deposit: %.2f USD" (patron.Deposit / 1.0M<USD>)
            printfn "  Fines balance: %.2f PLN" (patron.FinesBalance / 1.0M<PLN>)
            printfn "  Join date: %s" (patron.JoinDate.ToString("yyyy-MM-dd"))
            
            match patron.ContactDetails with
            | Some details ->
                printfn "  Name: %s %s" details.FirstName details.LastName
                match details.Email with
                | Some email -> printfn "  Email: %s" email
                | None -> printfn "  Email: [none]"
            | None ->
                printfn "  [No contact details]"
        | None -> ()

let displayCurrencyRates (usdToPlnRate: decimal<PLN/USD>) (eurToPlnRate: decimal<PLN/EUR>) =
    printfn "\n--- Current exchange rates ---"
    printfn "1 USD = %.4f PLN" (usdToPlnRate / 1.0M<PLN/USD>)
    printfn "1 EUR = %.4f PLN" (eurToPlnRate / 1.0M<PLN/EUR>)

[<EntryPoint>]
let main argv =
    printfn "========================================="
    printfn "  LIBRARY SYSTEM - Lab 7"
    printfn "========================================="
    
    let appDir = getAppDirectory()
    Directory.SetCurrentDirectory(appDir)
    printfn "Working directory: %s" appDir
    
    let library = Library()
    library.LoadPatrons("readers.json")
    
    printfn "\nFetching currency rates..."
    let mutable usdToPlnRate = getUsdToPlnRate()
    let mutable eurToPlnRate = getEurToPlnRate()
    printfn "USD/PLN: %.4f" (usdToPlnRate / 1.0M<PLN/USD>)
    printfn "EUR/PLN: %.4f" (eurToPlnRate / 1.0M<PLN/EUR>)
    
    let mutable running = true
    while running do
        printMenu()
        match Console.ReadLine() with
        | "1" -> displayPatronInfo library
        | "2" -> addFineToPatron library
        | "3" -> checkFinesAgainstDeposit library usdToPlnRate
        | "4" -> promotePatron library
        | "5" -> 
            printf "Enter patron ID: "
            match readInt() with
            | Some patronId -> displayLoanHistory patronId
            | None -> printfn "Invalid number"
        | "6" -> displayAllPatrons library
        | "7" -> 
            printfn "\nRefreshing currency rates..."
            usdToPlnRate <- getUsdToPlnRate()
            eurToPlnRate <- getEurToPlnRate()
            displayCurrencyRates usdToPlnRate eurToPlnRate
        | "0" -> 
            printfn "\nGoodbye!"
            running <- false
        | _ -> printfn "Invalid option. Please try again."
    
    0
