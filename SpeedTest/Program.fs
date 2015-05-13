module SpeedTest

open System
open System.Net
open System.Xml

type Isp = { Name: string; Rating: float; DownloadAvg: int; UploadAvg: int }

type ClientInfo = { Ip: string; Lat: float; Lon: float; Isp: Isp; Rating: float }

type Configuration (document: XmlDocument) = 
    member this.Client = this.GetClientInfo(document.GetElementsByTagName("client").Item(0))
    member this.Times = this.GetTimes(document.GetElementsByTagName("times").Item(0))
    member private this.GetClientInfo (client: XmlNode) =
        let attrs = client.Attributes
        in
        {
            Ip = attrs.["ip"].Value;
            Lat = attrs.["lat"].Value |> Double.Parse;
            Lon = attrs.["lon"].Value |> Double.Parse;
            Isp = this.GetIsp(attrs.["isp"].Value, attrs.["isprating"].Value, attrs.["ispdlavg"].Value, attrs.["ispulavg"].Value);
            Rating = attrs.["rating"].Value |> Double.Parse
        }
    member private this.GetIsp (name, rating, downloadAvg, uploadAvg) =
        {
            Name = name;
            Rating = rating |> Double.Parse;
            DownloadAvg = downloadAvg |> Int32.Parse;
            UploadAvg = uploadAvg |> Int32.Parse
        }
    member private this.GetTimes (times: XmlNode) =
        times.Attributes
        |> Seq.cast<XmlAttribute>
        |> Seq.map (fun time -> time.InnerText)
        |> Seq.toList

let radians angleDeg = angleDeg * Math.PI / 180.0

let square x = x * x

let distance origin dest =
    let lat1, lon1 = origin
    let lat2, lon2 = dest
    let radius = 6378.0 // km
    let dlat = radians(lat2 - lat1)
    let dlon = radians(lon2 - lon1)
    let a = square (sin (dlat / 2.0)) + cos (radians lat1) * cos (radians lat2) * square (sin (dlon / 2.0))
    let c = 2.0 * atan2 (sqrt a) (sqrt (1.0 - a))
    radius * c

[<EntryPoint>]
let main args =
    use client = new WebClient()
    in 
        client.Headers.Add("user-agent", "SpeedTest");
        let doc = new XmlDocument()
        in doc.LoadXml(client.DownloadString("http://www.speedtest.net/speedtest-config.php"));
            let config = new Configuration(doc);
            in printfn "%A" config.Times
    0