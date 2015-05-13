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

[<EntryPoint>]
let main args =
    use client = new WebClient()
    in 
        client.Headers.Add("user-agent", "SpeedTest");
        let doc = new XmlDocument()
        in doc.LoadXml(client.DownloadString("http://www.speedtest.net/speedtest-config.php"));
            let config = new Configuration(doc);
            in Console.WriteLine(config.Times)
    0