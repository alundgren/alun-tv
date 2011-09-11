module XmlParser

open System.Xml
open System.Xml.Linq

let internal children
    (accept : XElement -> bool)
    (root : XElement)
    =
    root.Descendants ()
    |> Seq.filter (accept)

let internal xmlNameFilter name = fun (e : XElement) -> e.Name.LocalName.ToLowerInvariant().Equals(name)

let internal get 
    (name : string)
    (xml : XElement) 
    =
    let attributeValue =
        xml.Attributes ()
        |> Seq.tryFind(fun e -> e.Name.LocalName.ToLowerInvariant().Equals(name))
        |> Option.bind (fun e -> Some(e.Value))
    if attributeValue.IsNone then
        xml.Descendants ()
        |> Seq.tryFind (fun e -> e.Name.LocalName.ToLowerInvariant().Equals(name))
        |> Option.bind (fun e -> Some(e.Value))
    else
        attributeValue

open Microsoft.FSharp.Reflection

let internal parseInt (s : string) = 
    match System.Int32.TryParse s with
        | false, _ -> None
        | true, i -> Some i

let internal parseDate (s : string) =
    let p1 (s : string) =
       let result = 
        System.DateTime.TryParseExact (
            s, 
            "MMM/dd/yyyy", 
            System.Globalization.CultureInfo.GetCultureInfo ("en-US"),
            System.Globalization.DateTimeStyles.None)
       match result with
        | false, _ -> None
        | true, d -> Some d
    let p2 (s : string) =
       let result = 
        System.DateTime.TryParseExact (
            s, 
            "yyyy-MM-dd", 
            System.Globalization.CultureInfo.GetCultureInfo ("en-US"),
            System.Globalization.DateTimeStyles.None)
       match result with
        | false, _ -> None
        | true, d -> Some d
    match p1 s with
        | None -> p2 s
        | x -> x

let internal req<'a> (name : string) (i : 'a option) = 
    match i with 
        | Some x -> x 
        | None -> failwith (sprintf "Missing required value: %s" name)

open System

let internal parseRecord<'a> (xml : XElement) =
    try
        let t = typeof<'a>
        let values =
            t.GetProperties ()
            |> Seq.map (fun p ->
                let v = get p.Name xml
                let parsedV : obj =
                    match p.PropertyType with
                        | t when t = typeof<string> ->(req p.Name v)  :> obj
                        | t when t = typeof<int> -> (req p.Name (Option.bind parseInt v)) :> obj
                        | t when t = typeof<DateTime> -> (req p.Name (Option.bind parseDate v)) :> obj
                        | t when t = typeof<string option> -> v :> obj
                        | t when t = typeof<int option> -> (Option.bind parseInt v) :> obj
                        | t when t = typeof<DateTime option> -> (Option.bind parseDate v) :> obj
                        | _ -> failwith "Unsupported type"
                parsedV)
            |> Array.ofSeq
        FSharpValue.MakeRecord (t, values)
        :?> 'a
    with
        | ex -> 
            failwith(sprintf "%s%s%s" ex.Message System.Environment.NewLine (xml.ToString ()))