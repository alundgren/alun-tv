module TvRage

open System.Xml
open System.Xml.Linq
open System
open XmlParser

type ShowInfoResult =
    {
        showid : int
        showname : string
        showlink : string
        seasons : int
        startdate : System.DateTime
        ended : DateTime option
        status : string
    }

let private loadXml (url : string) =
    let xml = XDocument.Load(url)
    //printfn "%s" (xml.ToString())
    xml.Root

let showinfo (showId : int) = 
    let root = loadXml(sprintf "http://services.tvrage.com/feeds/showinfo.php?sid=%i" showId)
    parseRecord<ShowInfoResult> root

type EpisodeListEpisode =
   {
        epnum : int
        seasonnum : int
        airdate : DateTime option
        link : string
        title : string
   }
type EpisodeListResult =
    {
        showid : int
        no : int
        episodes : EpisodeListEpisode seq
    }
        
let episode_list (showId : int) =
    let root = loadXml(sprintf "http://services.tvrage.com/feeds/episode_list.php?sid=%i" showId)
    root
    |> children (fun e -> (xmlNameFilter "season" e) &&  not (xmlNameFilter "episode" e.Parent)) //episode part filters out retarded specials-episodes. with a nested season-element with just a number 
    |> Seq.map(fun seasonElement ->
        let seasonNr = 
            get "no" seasonElement
            |> Option.bind XmlParser.parseInt
            |> XmlParser.req "no"
        let episodes = 
            seasonElement
            |> children (xmlNameFilter "episode")
            |> Seq.map parseRecord<EpisodeListEpisode>
        {
            showid = showId
            no = seasonNr
            episodes = episodes
        })

///started and ended are years and ended can be 0
type ShowSearchResult =
    {
        showid : int
        name : string
        link : string
        seasons : int
        started : int
        ended : int
        status : string
    }
let search (partialName : string) =
    loadXml(sprintf 
        "http://services.tvrage.com/feeds/search.php?show=%s" 
        (System.Web.HttpUtility.UrlEncode(partialName)))
    |> children (xmlNameFilter "show")
    |> Seq.map parseRecord<ShowSearchResult>