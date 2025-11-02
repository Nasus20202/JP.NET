#load "String50.fs"
#load "WrappedString.fs"
#load "EmailAddress.fs"
#load "ContactInfo.fs"

open Lab6.TypeDesigning

let validString50 = String50.create "Hello World"
let invalidString50 = String50.create (String.replicate 51 "a")

do match invalidString50 with
    | Some sbyte -> printfn "valid"
    | None -> printfn "invalid"

do validString50
    |> Option.map String50.value
    |> Option.map (fun s -> s.ToUpper())
    |> Option.iter (printfn "%s")

do validString50
    |> Option.map String50.value
    |> Option.map (fun s -> s.ToUpper())
    |> Option.map (printfn "%s")
    |> ignore

do match validString50 with
    | Some s -> printfn "%s" ((String50.value s).ToUpper())
    | None -> ()

do 
    let result =
        match validString50 with
        | Some s -> (String50.value s).ToUpper()
        | None -> ""
    printfn "%s" result


let s50 =  WrappedString.string50 "abc" |> Option.get
let bad = WrappedString.string50 null
let s100 = WrappedString.string100 "abc" |> Option.get

do
    printfn "s50 is %A" s50
    printfn "bad is %A" bad
    printfn "s100 is %A" s100
    printfn "s50 is equal to s100 using module equals? %b" (WrappedString.equals s50 s100)
    printfn "s50 is equal to s100 using Object.Equals? %b" (s50.Equals s100)
    // printfn "s50 is equal to s100? %b" (s50 = s100)

let address1 = EmailAddress.create "x@example.com"
let address2 = EmailAddress.create "example.com"

do
    printfn "address1: %A" address1
    printfn "address2: %A" address2

let success (EmailAddress.EmailAddress s) = printfn "success creating email %s" s
let failure msg = printfn "error creating email: %s" msg
let createEmailAddress = EmailAddress.createWithCont success failure

let address3 = createEmailAddress "example@com"
printfn "address3: %A" address3
let address4 = createEmailAddress "x@example.com"
printfn "address4: %A" address4

let contact1 = 
    match EmailAddress.create "x@example.com" with
    | Some email -> ContactInfo.EmailOnly email
    | None -> ContactInfo.PostOnly "No email provided"
printfn "contact1: %A" contact1
let contact2 = 
    match EmailAddress.create "example.com" with
    | Some email -> ContactInfo.EmailAndPost (email, "123 Main St")
    | None -> ContactInfo.PostOnly "123 Main St"
printfn "contact2: %A" contact2
let contact3 = 
    match EmailAddress.create "x@example.com" with
    | Some email -> ContactInfo.EmailAndPost (email, "456 Elm St")
    | None -> ContactInfo.PostOnly "456 Elm St"
printfn "contact3: %A" contact3