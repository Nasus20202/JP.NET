namespace LibrarySystem

module LibraryFunctions =
    
    val getLoanHistory : int -> LoanHistory option
    
    val isPatronForLongerThan : Patron -> int -> bool
    
    val addFine : Patron -> decimal<PLN> -> Patron

type Library =
    new : unit -> Library
    
    member LoadPatrons : string -> unit
    
    member GetPatron : int -> Patron option
    
    member UpdatePatron : Patron -> unit
    
    member PromoteToPremium : int -> int -> int -> bool
    
    member GetPatronInfo : int -> (string * string * string option) option
    
    member AddFineToPatron : int -> decimal<PLN> -> bool
    
    member CheckFinesAgainstDeposit : int -> decimal<PLN/USD> -> bool

