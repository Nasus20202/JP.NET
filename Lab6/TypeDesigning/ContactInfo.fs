namespace Lab6.TypeDesigning

module ContactInfo =
    type ContactInfo =
        | EmailOnly of EmailAddress.T
        | PostOnly of string
        | EmailAndPost of EmailAddress.T * string
