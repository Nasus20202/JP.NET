namespace LibrarySystem

open System

[<Measure>] type USD
[<Measure>] type PLN
[<Measure>] type EUR

type ContactMethod =
    | PhoneOnly of phone: string
    | PostalAddressOnly of address: string
    | PhoneAndPostalAddress of phone: string * address: string

type ContactDetails = {
    FirstName: string
    LastName: string
    DateOfBirth: DateTime
    LibraryCardNumber: string
    Email: string option
    ContactMethod: ContactMethod
}

type AccountStatus =
    | Standard
    | Premium

type Patron = {
    PatronId: int
    ContactDetails: ContactDetails option
    Deposit: decimal<USD>
    JoinDate: DateTime
    Status: AccountStatus
    FinesBalance: decimal<PLN>
}

type Loan = string * bool

type LoanHistory = {
    PatronId: int
    Loans: Loan list
}

module JsonTypes =
    open System.Text.Json.Serialization
    
    type ContactMethodJson =
        | PhoneOnly = 0
        | PostalAddressOnly = 1
        | PhoneAndPostalAddress = 2
    
    type ContactMethodDto = {
        Type: ContactMethodJson
        Phone: string option
        PostalAddress: string option
    }
    
    type ContactDetailsDto = {
        FirstName: string
        LastName: string
        DateOfBirth: string
        LibraryCardNumber: string
        Email: string option
        ContactMethod: ContactMethodDto
    }
    
    type PatronDto = {
        PatronId: int
        ContactDetails: ContactDetailsDto option
        Deposit: decimal
        JoinDate: string
        Status: string
        FinesBalance: decimal
    }
    
    type LoanDto = {
        ISBN: string
        IsReturned: bool
    }
    
    type LoanHistoryDto = {
        PatronId: int
        Loans: LoanDto list
    }

module Conversions =
    open JsonTypes
    
    let contactMethodFromDto (dto: ContactMethodDto) : ContactMethod =
        match dto.Type with
        | ContactMethodJson.PhoneOnly -> 
            PhoneOnly (dto.Phone |> Option.defaultValue "")
        | ContactMethodJson.PostalAddressOnly -> 
            PostalAddressOnly (dto.PostalAddress |> Option.defaultValue "")
        | ContactMethodJson.PhoneAndPostalAddress -> 
            let phone = dto.Phone |> Option.defaultValue ""
            let address = dto.PostalAddress |> Option.defaultValue ""
            PhoneAndPostalAddress (phone, address)
        | _ -> failwith "Unknown contact method type"
    
    let contactMethodToDto (cm: ContactMethod) : ContactMethodDto =
        match cm with
        | PhoneOnly phone -> 
            { Type = ContactMethodJson.PhoneOnly; Phone = Some phone; PostalAddress = None }
        | PostalAddressOnly address -> 
            { Type = ContactMethodJson.PostalAddressOnly; Phone = None; PostalAddress = Some address }
        | PhoneAndPostalAddress (phone, address) -> 
            { Type = ContactMethodJson.PhoneAndPostalAddress; Phone = Some phone; PostalAddress = Some address }
    
    let contactDetailsFromDto (dto: ContactDetailsDto) : ContactDetails =
        {
            FirstName = dto.FirstName
            LastName = dto.LastName
            DateOfBirth = DateTime.Parse(dto.DateOfBirth)
            LibraryCardNumber = dto.LibraryCardNumber
            Email = dto.Email
            ContactMethod = contactMethodFromDto dto.ContactMethod
        }
    
    let contactDetailsToDto (details: ContactDetails) : ContactDetailsDto =
        {
            FirstName = details.FirstName
            LastName = details.LastName
            DateOfBirth = details.DateOfBirth.ToString("yyyy-MM-dd")
            LibraryCardNumber = details.LibraryCardNumber
            Email = details.Email
            ContactMethod = contactMethodToDto details.ContactMethod
        }
    
    let patronFromDto (dto: PatronDto) : Patron =
        let status = 
            match dto.Status.ToLower() with
            | "premium" -> Premium
            | _ -> Standard
        
        {
            PatronId = dto.PatronId
            ContactDetails = dto.ContactDetails |> Option.map contactDetailsFromDto
            Deposit = dto.Deposit * 1.0M<USD>
            JoinDate = DateTime.Parse(dto.JoinDate)
            Status = status
            FinesBalance = dto.FinesBalance * 1.0M<PLN>
        }
    
    let patronToDto (patron: Patron) : PatronDto =
        let statusStr = 
            match patron.Status with
            | Premium -> "Premium"
            | Standard -> "Standard"
        
        {
            PatronId = patron.PatronId
            ContactDetails = patron.ContactDetails |> Option.map contactDetailsToDto
            Deposit = patron.Deposit / 1.0M<USD>
            JoinDate = patron.JoinDate.ToString("yyyy-MM-dd")
            Status = statusStr
            FinesBalance = patron.FinesBalance / 1.0M<PLN>
        }
    
    let loanFromDto (dto: LoanDto) : Loan =
        (dto.ISBN, dto.IsReturned)
    
    let loanToDto ((isbn, returned): Loan) : LoanDto =
        { ISBN = isbn; IsReturned = returned }
    
    let loanHistoryFromDto (dto: LoanHistoryDto) : LoanHistory =
        {
            PatronId = dto.PatronId
            Loans = dto.Loans |> List.map loanFromDto
        }
    
    let loanHistoryToDto (history: LoanHistory) : LoanHistoryDto =
        {
            PatronId = history.PatronId
            Loans = history.Loans |> List.map loanToDto
        }
