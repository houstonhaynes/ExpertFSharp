open System.Net
open System.Net.Sockets
open System.IO
open System.Text.RegularExpressions
open System.Text

/// A table of MIME content types.
let mimeTypes =
    dict [".html", "text/html";
          ".htm", "text/html";
          ".txt", "text/plain";
          ".gif", "image/gif";
          ".jpg", "image/jpeg";
          ".png", "image/png"]

/// Compute a MIME type from a file extension.
let getMimeType(ext) =
    if mimeTypes.ContainsKey(ext) then mimeTypes.[ext]
    else "binary/octet"

/// The pattern Regex1 uses a regular expression to match one element.
let (|Regex1|_|) (patt : string) (inp : string) =
    try Some(Regex.Match(inp, patt).Groups.Item(1).Captures.Item(0).Value)
    with _ -> None

/// The root for the data we serve
let root = @"c:\inetpub\wwwroot"

/// Handle a TCP connection for an HTTP GET. We use an asynchronous task in
/// case any future actions in handling a request need to be asynchronous.
let handleRequest(client : TcpClient) = async {
    use stream = client.GetStream()
    let out = new StreamWriter(stream)
    let headers (lines : seq<string>) =
        let printLine s = s |> fprintf out "%s\r\n"
        lines |> Seq.iter printLine
        // An empty line is required before content, if any.
        printLine ""
        out.Flush()
    let notFound () = headers ["HTTP/1.0 404 Not Found"]
    let inp = new StreamReader(stream)
    let request = inp.ReadLine()
    match request with
    | "GET / HTTP/1.0" | "GET / HTTP/1.1" ->
        // From the root, redirect to the start page.
        headers ["HTTP/1.0 302 Found"; "Location: http://localhost:8090/iisstart.htm"]
    | Regex1 "GET /(.*?) HTTP/1\\.[01]$" fileName ->
        let fname = Path.Combine(root, fileName)
        let mimeType = getMimeType(Path.GetExtension(fname))
        if not(File.Exists(fname)) then notFound()
        else
            let content = File.ReadAllBytes fname
            ["HTTP/1.0 200 OK";
            sprintf "Content-Length: %d" content.Length;
            sprintf "Content-Type: %s" mimeType]
            |> headers
            stream.Write(content, 0, content.Length)
    | _ -> notFound()}

/// The server as an asynchronous process. We handle requests sequentially.
let server = async { 
    let socket = new TcpListener(IPAddress.Parse("127.0.0.1"), 8090)
    socket.Start()
    while true do
        use client = socket.AcceptTcpClient()
        do! handleRequest client}
//val mimeTypes : System.Collections.Generic.IDictionary<string,string>
//val getMimeType : ext:string -> string
//val ( |Regex1|_| ) : patt:string -> inp:string -> string option
//val root : string = "c:\inetpub\wwwroot"
//val handleRequest : client:System.Net.Sockets.TcpClient -> Async<unit>
//val server : Async<unit>

> Async.Start server;;
//val it : unit = ()

open System.IO
open System.Net

// From chapter 2, getting started ...
/// Get the contents of the URL via a web request
let http (url : string) =
    let req = WebRequest.Create(url)
    let resp = req.GetResponse()
    let stream = resp.GetResponseStream()
    let reader = new StreamReader(stream)
    let html = reader.ReadToEnd()
    resp.Close()
    html
//val http : url:string -> string

> http "http://127.0.0.1:8090/iisstart.htm";;
//val it : string = "..."   // the text of the iisstart.htm file will be shown here

> http "http://127.0.0.1:8090/";;
//val it : string = "..."   // the text of the iisstart.htm file will be shown here

type AsyncTcpServer(addr, port, handleServerRequest) = 
    let socket = new TcpListener(addr, port)
    member x.Start() = async {do x.Run()} |> Async.Start
    member x.Run() = 
        socket.Start()
        while true do
            let client = socket.AcceptTcpClient()
            Async.Start (async { try // Client has lifetime equal to the async request
                                     use _holder = client
                                     do! handleServerRequest client 
                                  with e -> () })

let quoteSize = 512 // one quote
let quote = Array.init<byte> quoteSize (fun i -> 1uy)

let handleRequest (client : TcpClient) =
    async {
        // Cleanup the client at the end of the request
        let stream = client.GetStream()
        do! stream.AsyncWrite(quote, 0, 1)  // write header
        while true do
            do! stream.AsyncWrite(quote, 0, quote.Length) 
            // Mock an I/O wait for the next quote
            do! Async.Sleep 1000

        stream.Close()
    }


let server() = 
    AsyncTcpServer(IPAddress.Loopback, 10003, handleRequest)

type AsyncTcpServerSecure(addr, port, handleServerRequest) = 

    let getCertficate() =
        // Instantiate the x509Store object to represent the Certificate Store
        // that contains the certificate to use for server authentication.
        let store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

        // Open the store for read-only access.
        store.Open(OpenFlags.ReadOnly);

        // Extract all the certificates that match your certificate criteria. 
        // Return all certificates that have the identifying value in their name.
        let cert = store.Certificates.Find(X509FindType.FindBySubjectName, 
                                           "localhost", true);

        // This assumes the collection contains one certificate, 
        // based on the search criteria.
        cert.[0];

    let handleServerRequestSecure (client : TcpClient) = 
        async {
            let stream = new SslStream(client.GetStream());
            do! stream.AsyncAuthenticateAsServer(getCertficate());
            
            if (stream.IsAuthenticated) then
                Console.WriteLine("IsAuthenticated: {0}", stream.IsAuthenticated);

                // In this example only the server is authenticated.
                Console.WriteLine("IsEncrypted: {0}", stream.IsEncrypted);
                Console.WriteLine("IsSigned: {0}", stream.IsSigned);

                // Indicates whether the current side of the connection 
                // is authenticated as a server.
                Console.WriteLine("IsServer: {0}", stream.IsServer);

            return! handleServerRequest stream
        }

    let server = AsyncTcpServer(addr, port, handleServerRequestSecure)

    member x.Start() = server.Start()

namespace Website

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html

[<JavaScript>]
let HelloWorld () =
    let welcome = P [Text "Welcome"]
    Div [
        welcome
        Input [Attr.Type "Button"; Attr.Value "Click me!"]
        |>! OnClick (fun e args ->
            welcome.Text <- "Hello, world!")
    ]

Div [Attr.Class "your-css-class"] -< [ � ]

type MyControl() =
    inherit Web.Control()

    [<JavaScript>]
    override this.Body = HelloWorld () :> _

//<configuration>
//  <system.web>
//    <pages>
//      <controls>
//        <add tagPrefix="WebSharper"
//             namespace="IntelliFactory.WebSharper.Web
//             assembly="IntelliFactory.WebSharper.Web" />
//        <add tagPrefix="ws"
//             namespace="Website"
//             assembly="Website" />
//      </controls>
//...
//
//<html xmlns="http://www.w3.org/1999/xhtml">
//  <head runat="server">
//    <title>Your WebSharper Application</title>
//    <WebSharper:ScriptManager runat="server" />
//  </head>
//  <body>
//    <ws:MyControl runat="server"/>
//  </body>
//</html>

module YourWebApplication =
    open IntelliFactory.WebSharper 
    
    module Server =

        [<Rpc>]
        let YourServerFunction (...) =
            ...

    module Client =

        [<JavaScript>]
        let YourClientFunction (...) =
            ...
            let data = Server.YourServerFunction (...)
            ...

open IntelliFactory.WebSharper

module MySite =
    open IntelliFactory.Html
    open IntelliFactory.WebSharper.Sitelets

    type Action = | MyPage

    module Pages =
        let MyPage =
            Content.PageContent <| fun ctx ->
                { Page.Default with
                    Title = Some "My page"
                    Body =
                        [
                            H1 [Text "Hello world!"]
                        ]
                }

    let EntireSite =
        Sitelet.Content "/" Action.MyPage Pages.MyPage

    type MyWebsite() =
        interface IWebsite<Action> with
            member this.Sitelet = EntireSite
            member this.Actions = []

[<assembly : Sitelets.Website(typeof<MySite.MyWebsite>)>]
do ()

/// Represents HTML pages with embedded WebSharper controls.
type Page =
    {
      Doctype : string option
      Title : string option
      Renderer : string option -> string option -> Writer -> Writer -> HtmlTextWriter -> unit
      Head : Element<unit> seq
      Body : Element<Control> seq
    }

    static member Default =
        {
          Doctype = Some "<!DOCTYPE html>"
          Title = None
          Head = []
          Renderer = ...
          Body = []
        }

//// Represents HTTP responses
type Response =
    {
        Status : Http.Status
        Headers : Http.Header seq
        WriteBody : System.IO.Stream -> unit
    }

Content.CustomContent <| fun ctx ->
    let cd = "attachment; filename=\"myfile.zip\""
    {
         Status = Http.Status.Ok
         Headers = [Http.Header.Custom "Content-Disposition" cd]
         WriteBody = ...
    }

open System.IO
open IntelliFactory.WebSharper.Sitelets

module MySite =
    open IntelliFactory.WebSharper
    open IntelliFactory.Html

    // Your sitelet action type
    type Action =
        | Home

    module Skin =
        type Placeholders =
            {
                Title : string
                Body : list<Content.HtmlElement>
            }

        let MainTemplate =
            let path = Path.Combine(__SOURCE_DIRECTORY__, "MyTemplate.html")
            Content.Template<Placeholders>(path)
                .With("title", fun x -> x.Title)
                .With("body", fun x -> x.Body)

        let WithTemplate title body : Content<Action> =
            Content.WithTemplate MainTemplate <| fun context ->
                {
                    Title = title
                    Body = body context
                }

//<!DOCTYPE html>
//<html>
//  <head>
//    <title>${title}</title>
//    <meta name="generator" content="websharper" data-replace="scripts" />
//  </head>
//  <body>
//    <div data-hole="body"></div>
//  </body>
//</html>

MySite.Skin.WithTemplate "Hello World" <| fun ctx ->
    [
        Div [new Website.MyControl()]
    ]

module MySite =
    open IntelliFactory.Html
    open IntelliFactory.WebSharper.Sitelets

    type Action =
        | MyPage
        | Protected
        | Login of Action option
        | Logout

    module Pages =

        /// A helper function to create a hyperlink.
        let ( => ) title href =
            A [HRef href] -< [Text title]

        /// A helper function to create a 'fresh' URL with a random parameter
        /// in order to make sure that browsers don't show a cached version.
        let R url =
            url + "?d=" + System.Uri.EscapeUriString (System.DateTime.Now.ToString())

        module Utils =

            let SimpleContent title content =
                Content.PageContent <| fun ctx ->
                    { Page.Default with
                        Title = Some title
                        Body =
                          [
                            match UserSession.GetLoggedInUser() with
                            | None ->
                                yield "Login" => ctx.Link (Action.Login (Some Action.MyPage))
                            | Some user ->
                                yield "Log out ["+user+"]" => R (ctx.Link Action.Logout)
                          ] @ content ctx
                    }


        let MyPage =
            Utils.SimpleContent "My Page" <| fun ctx ->
                [
                    H1 [Text "Hello world!"]
                    "Protected content" => R (ctx.Link Action.Protected)
                ]

        let ProtectedPage =
            Utils.SimpleContent "Protected Page" <| fun ctx ->
                [
                    H1 [Text "This is protected content!"]
                    "Go back" => (ctx.Link Action.MyPage)
                ]

        let LoginPage action =
            Utils.SimpleContent "Login Page" <| fun ctx ->
                let redirectUrl =
                    match action with
                    | None ->
                        Action.MyPage
                    | Some action ->
                        action
                    |> ctx.Link
                    |> R
                [
                    H1 [Text "You have been logged in magically..."]
                    "Proceed further" => redirectUrl
                ]

    let NonProtected =
        Sitelet.Infer <| function
            | Action.MyPage ->
                Pages.MyPage
            | Action.Login action->
                // Log in a user as "visitor" without requiring anything
                UserSession.LoginUser "visitor"
                Pages.LoginPage action
            | Action.Logout ->
                // Log out the "visitor" user and redirect to home
                UserSession.Logout ()
                Content.Redirect Action.MyPage
            | Action.Protected ->
                Content.ServerError

    type Action =
        | [<CompiledName "home">] MyPage
        | Protected
        | Login of Action option
        | Logout


    let Protected =
        let filter : Sitelet.Filter<Action> =
            {
                VerifyUser = fun _ -> true
                LoginRedirect = Some >> Action.Login
            }

        Sitelet.Protect filter <|
            Sitelet.Content "/protected" Action.Protected Pages.ProtectedPage

    let EntireSite = Protected <|> NonProtected


namespace Website

type Order =
    {
        ItemName : string
        Quantity : int
    }
    static member Dummy() =
        {ItemName = "N/A"; Quantity = 0}

module Orders =
    let private id = ref 0
    let Store = ref Map.empty

    let Save (id : int) (order : Order) =
        Store := (!Store).Add(id, order)

    let FindById (id : int) =
        if (!Store).ContainsKey id then
            Some <| (!Store).[id]
        else
            None

    let Delete (id : int) =
        if (!Store).ContainsKey id then
            Store := (!Store).Remove id

    let GetId () =
        id := !id + 1
        !id


open IntelliFactory.WebSharper

type Action =
    | CreateOrderForm
    | CreateOrder of Order
    | DeleteOrder of int
    | GetOrder of int
    | ListOrders
    | UpdateOrder of int * Order

module Skin =
    open System.Web
    open IntelliFactory.Html
    open IntelliFactory.WebSharper.Sitelets

    let TemplateLoadFrequency = Content.Template.PerRequest

    type Page = {Body : list<Content.HtmlElement>}

    let MainTemplate =
        let path = HttpContext.Current.Server.MapPath("~/Main.html")
        Content.Template<Page>(path, TemplateLoadFrequency)
            .With("body", fun x -> x.Body)

    let WithTemplate body : Content<Action> =
        Content.WithTemplate MainTemplate <| fun context ->
            {
                Body = body context
            }

//<!DOCTYPE html>
//<html>
//  <head>
//    <meta name="generator" content="websharper" data-replace="scripts" />
//  </head>
//  <body>
//    <div data-hole="body" />
//  </body>
//</html>

module Client =
    open IntelliFactory.WebSharper.Formlet
    open IntelliFactory.WebSharper.Html

    [<JavaScript>]
    let OrderForm orderPostUrl =
        Formlet.Yield (fun title qty -> {ItemName = title; Quantity = qty})
        <*> (Controls.Input ""
            |> Validator.IsNotEmpty "Must enter a title"
            |> Enhance.WithTextLabel "Title")
        <*> (Controls.Input ""
            |> Validator.IsInt "Must enter a valid quantity"
            |> Formlet.Map int
            |> Enhance.WithTextLabel "Quantity")
        |> Enhance.WithSubmitAndResetButtons
        |> Enhance.WithErrorSummary "Errors"
        |> Enhance.WithFormContainer
        |> Enhance.WithJsonPost
            {
                Enhance.JsonPostConfiguration.EncodingType = None
                Enhance.JsonPostConfiguration.ParameterName = "order"
                Enhance.JsonPostConfiguration.PostUrl = Some orderPostUrl
            }

    type OrderFormControl(orderPostUrl : string) =
        inherit Web.Control()

        [<JavaScript>]
        override this.Body =
            Div [
                OrderForm(orderPostUrl)
            ] :> _

module Pages =
    open IntelliFactory.Html

    let ( => ) text url =
        A [HRef url] -< [Text text]

    let Links (ctx : Context<Action>) =
        UL [
            LI ["Home" => ctx.Link Action.ListOrders]
            LI ["New"  => ctx.Link Action.CreateOrderForm]
        ]

    let CreateOrderFormPage =
        Skin.WithTemplate <| fun ctx ->
            [
                H1 [Text "Create order"]
                Links ctx
                HR []
                Div [
                    new Client.OrderFormControl(
                        Order.Dummy() |> Action.CreateOrder |> ctx.Link
                    )
                ]
            ]

    let ListOrdersPage =
        Skin.WithTemplate <| fun ctx ->
            [
                H1 [Text "Orders"]
                Links ctx
                HR []
                UL <|
                    ((!Orders.Store)
                    |> Map.toList
                    |> Seq.map (fun (id, order) ->
                        LI [
                            A [HRef <| ctx.Link (Action.GetOrder id)] -< [
                                sprintf "#%d: %s [%d]" id order.ItemName order.Quantity
                                |> Text
                            ]
                        ]
                    ))
            ]

    let GetOrder id =
        if (!Orders.Store).ContainsKey id then
            Content.CustomContent <| fun ctx ->
                {
                    Http.Response.Status = Http.Status.Ok
                    Http.Response.Headers = [Http.Header.Custom "Content-type" "application/json"]
                    Http.Response.WriteBody = fun stream ->
                        use writer = new System.IO.StreamWriter(stream)
                        let order = (!Orders.Store).[id]
                        let encoder = Web.Shared.Json.GetEncoder(typeof<Order>)
                        order
                        |> encoder.Encode
                        |> Web.Shared.Json.Pack
                        |> Core.Json.Stringify
                        |> writer.Write
                        writer.Close()
                }
        else
            Content.NotFound


module MySite =
    open UrlHelpers

    let (|PATH|_|) (uri : System.Uri) = Some <| uri.LocalPath

    let MySitelet =
        Sitelet.Sum [
            Sitelet.Content "/create" Action.CreateOrderForm Pages.CreateOrderFormPage
            Sitelet.Content "/orders" Action.ListOrders Pages.ListOrdersPage
        ]
        <|>
        {
            Router = Sitelets.Router.New
                <| function
                    | POST (values, PATH @"/order") ->
                        try
                            let decoder = Web.Shared.Json.GetDecoder<Order>()
                            let order =
                                values
                                |> Map.ofList
                                |> fun map -> map.["order"]
                                |> Core.Json.Parse
                                |> decoder.Decode
                            Some <| Action.CreateOrder order
                        with
                        | _ ->
                            None
                    | GET (values, SPLIT_BY '/' ["order"; INT id]) ->
                        Some <| Action.GetOrder id
                    | PUT (values, SPLIT_BY '/' ["/order"; INT id]) ->
                        try
                            let decoder = Web.Shared.Json.GetDecoder<Order>()
                            let order =
                                values
                                |> Map.ofList
                                |> fun map -> map.["order"]
                                |> Core.Json.Parse
                                |> decoder.Decode
                            Some <| Action.UpdateOrder(id, order)
                        with
                        | _ ->
                            None
                    | DELETE (values, SPLIT_BY '/' ["/order"; INT id]) ->
                        Some <| Action.DeleteOrder id
                    | _ ->
                        None
                <| function
                    | Action.CreateOrder order ->
                        Some <| System.Uri(@"/order", System.UriKind.Relative)
                    | Action.DeleteOrder id
                    | Action.GetOrder id
                    | Action.UpdateOrder (id, _) ->
                        Some <| System.Uri(sprintf @"/order/%d" id, System.UriKind.Relative)
                    | _ ->
                        None

            Controller =
                {
                    Handle = function
                        | Action.CreateOrder order ->
                            Orders.Save (Orders.GetId()) order
                            Content.Redirect Action.ListOrders
                        | Action.DeleteOrder id ->
                            Orders.Delete id
                            Content.Redirect Action.ListOrders
                        | Action.GetOrder id ->
                            Pages.GetOrder id
                        | Action.UpdateOrder (id, order) ->
                            Orders.Save id order
                            Content.Redirect Action.ListOrders
                        | _ ->
                            failwith "unmatched"
                }
        }

    type MyWebsite() =
        interface IWebsite<Action> with
            member this.Sitelet = MySitelet
            member this.Actions = []
                    | GET (values, SPLIT_BY '/' ["order"; INT id]) ->
                        Some <| Action.GetOrder id

[<assembly : Website(typeof<MySite.MyWebsite>)>]
do ()

//{"$TYPES":[],"$DATA":{"$V":{"ItemName":"Windows Server 2012","Quantity":5}}}

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Formlet
open IntelliFactory.WebSharper.Html

[<JavaScript>]
let Snippet1 = Controls.Input "initial value"

[<JavaScript>]
let RunInBlock title f formlet =
    let output = Div []
    formlet
    |> Formlet.Run (fun res ->
        let elem = f res
        output -< [ elem ] |> ignore)
    |> fun form ->
        Div [Attr.Style "float:left;margin-right:20px;width:300px;min-height:200px;"] -< [
 
            H5 [Text title]
            Div [form]
            output
        ]

[<JavaScript>]
let RunSnippet title formlet =
    formlet
    |> RunInBlock title (fun s ->
        Div [
            P ["You entered: " + s |> Text]
        ])

type Snippet() = 
    inherit Web.Control()

    [<JavaScript>]
    override this.Body = RunSnippet "Snippet1" Snippet1 :> _

[<JavaScript>]
let Snippet1a =
    Formlet.Yield (fun name -> name)
    <*> (Controls.Input ""
         |> Validator.Is (fun s -> s.Length > 3) "Enter a valid name")

Formlet.Yield (fun v1 ... vn -> <compose into a single return value>)
<*> formlet1
<*> ...
<*> formletn

[<JavaScript>]
let Snippet1b =
    Formlet.Yield (fun name -> name)
    <*> (Controls.Input ""
         |> Validator.IsNotEmpty "Enter a valid name"
         |> Enhance.WithFormContainer)

[<JavaScript>]
let Snippet1c =
    Formlet.Yield (fun name -> name)
    <*> (Controls.Input ""
         |> Validator.IsNotEmpty "Enter a valid name"
         |> Enhance.WithFormContainer
         |> Enhance.WithSubmitAndResetButtons)

[<JavaScript>]
let Snippet1d =
    Formlet.Yield (fun name -> name)
    <*> (Controls.Input ""
         |> Validator.IsNotEmpty "Enter a valid name"
         |> Enhance.WithValidationIcon
         |> Enhance.WithErrorSummary "Errors"
         |> Enhance.WithSubmitAndResetButtons
         |> Enhance.WithFormContainer)

[<JavaScript>]
let Snippet1e =
    Formlet.Yield (fun name -> name)
    <*> (Controls.Input ""
         |> Validator.IsNotEmpty "Enter a valid name"
         |> Enhance.WithValidationIcon
         |> Enhance.WithTextLabel "Name"
         |> Enhance.WithSubmitAndResetButtons
         |> Enhance.WithFormContainer)

[<JavaScript>]
let Snippet1f =
    Formlet.Yield (fun name -> name)
    <*> (Controls.Input ""
         |> Validator.IsNotEmpty "Enter a valid name"
         |> Enhance.WithValidationIcon
         |> Enhance.WithLabelAndInfo "Name" "Enter your name"
         |> Enhance.WithSubmitAndResetButtons
         |> Enhance.WithFormContainer)

module Formlets =
    ...
    type Snippets() = 
        inherit Web.Control()

        [<JavaScript>]
        override this.Body =
            Div [
                RunSnippet "Snippet1"  Snippet1
                RunSnippet "Snippet1a" Snippet1a
                RunSnippet "Snippet1b" Snippet1b
                RunSnippet "Snippet1c" Snippet1c
                RunSnippet "Snippet1d" Snippet1d
                RunSnippet "Snippet1e" Snippet1e
                RunSnippet "Snippet1f" Snippet1f
            ] :> _

module MySite =
    open IntelliFactory.Html
    open IntelliFactory.WebSharper.Sitelets

    type Action = | Home

    module Pages =
        let SnippetsPage =
            Content.PageContent <| fun ctx ->
                { Page.Default with
                    Title = Some "Formlet snippets"
                    Body =
                        [
                            H1 [Text "Snippets"]
                            Div [new Formlets.Snippets()]
                        ]
                }

    let EntireSite =
        Sitelet.Content "/home" Action.Home Pages.SnippetsPage

    type MyWebsite() =
        interface IWebsite<Action> with
            member this.Sitelet = EntireSite
            member this.Actions = []

[<assembly : Sitelets.Website(typeof<MySite.MyWebsite>)>]
do ()

[<JavaScript>]
let input (label : string) (err : string) = 
    Controls.Input ""
    |> Validator.IsNotEmpty err
    |> Enhance.WithValidationIcon
    |> Enhance.WithTextLabel label

[<JavaScript>]
let inputInt (label : string) (err : string) = 
    Controls.Input ""
    |> Validator.IsInt err
    |> Enhance.WithValidationIcon
    |> Enhance.WithTextLabel label

[<JavaScript>]
let Snippet2 : Formlet<string * int> =
    Formlet.Yield (fun name age -> name, age |> int)
    <*> input "Name" "Please enter your name"
    <*> inputInt "Age" "Please enter a valid age"
    |> Enhance.WithSubmitAndResetButtons
    |> Enhance.WithFormContainer

[<JavaScript>]
let Snippet3a =
    Formlet.Yield (fun name age -> name, age |> int)
    <*> input "Name" "Please enter your name"
    <*> inputInt "Age" "Please enter a valid age"
    |> Enhance.WithLegend "Person"
    |> Enhance.WithTextLabel "Person"
    |> Formlet.Many
    |> Enhance.WithLegend "People"
    |> Enhance.WithSubmitAndResetButtons
    |> Enhance.WithFormContainer

[<JavaScript>]
let Snippet4 =
    Formlet.Do {
        let! name = input "Name" "Please enter your name"
        let! age = inputInt "Age" "Please enter a valid age"
        return name, age |> int
    }
    |> Enhance.WithSubmitAndResetButtons
    |> Enhance.WithFormContainer

[<JavaScript>]
let Snippet4b =
    Formlet.Do {
        let! name = input "Name" "Please enter your name"
                    |> Enhance.WithSubmitAndResetButtons
                    |> Enhance.WithFormContainer
        let! age =  inputInt "Age" "Please enter a valid age"
                    |> Enhance.WithSubmitAndResetButtons
                    |> Enhance.WithFormContainer
        return name, age |> int
    }
    |> Formlet.Flowlet

    open IntelliFactory.WebSharper.Core

    type MyResource() =
        interface Resources.IResource with
            member this.Render ctx writer =
                writer.WriteLine "<script type=\"javascript\" src=\"lib\\my.js\"></script>"

    [<assembly : System.Web.UI.WebResource("my.js", "text/javascript")>]
    do ()

    type MyEmbeddedResource() =
        inherit Resources.BaseResource("my.js")

    type MyExternalResource() =
        inherit Resources.BaseResource(@"http:\\your.domain.net", "lib.js", "style.css")

    [<Require(typeof<MyExternalResource>)>]
    type Hello = ..

open IntelliFactory.WebSharper

[<Proxy(typeof<System.Int32>)>]
type private Int32 =

    static member MaxValue with [<Inline "2147483647">]  get () = 0
    static member MinValue with [<Inline "-2147483648">] get () = 0

    [<Inline "parseInt($s)">]
    static member Parse(s: string) = X<int>
