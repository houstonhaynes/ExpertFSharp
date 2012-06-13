﻿> let squareAndAdd a b = a * a + b;;
//val squareAndAdd : a:int -> b:int -> int
> let squareAndAdd (a:float) b = a * a + b;;
//val squareAndAdd : a:float -> b:float -> float
> let squareAndAdd (a:float) (b:float) : float = a * a + b;;
//val squareAndAdd : a:float -> b:float -> float
> let s = "Couldn't put Humpty";;
//val s : string = "Couldn't put Humpty"
> s.Length;;
//val it : int = 19
> s.[13];;
//val it : char = 'H'
> s.[13..16];;
//val it : string = "Hump"
> let s = "Couldn't put Humpty";;
//val s : string = "Couldn't put Humpty"
> s.[13] <- 'h';;
//Script.fsx(17,1): error FS0751: Invalid indexer expression
>
> "Couldn't put Humpty" + " " + "together again";;
//val it : string = "Couldn't put Humpty together again"

let round x =
    if x >= 100 then 100
    elif x < 0 then 0
    else x

let round x =
    match x with
    | _ when x >= 100 -> 100
    | _ when x < 0 -> 0
    | _ -> x

let round2 (x, y) =
    if x >= 100 || y >= 100 then 100, 100
    elif x < 0 || y < 0 then 0, 0
    else x, y

> let rec factorial n = if n <= 1 then 1 else n * factorial (n - 1);;
//val factorial : n:int -> int
> factorial 5;;
//val it : int = 120

let rec length l =
    match l with
    | [] -> 0
    | h :: t -> 1 + length t

let rec repeatFetch url n =
    if n > 0 then
        let html = http url
        printfn "fetched <<< %s >>> on iteration %d" html n
        repeatFetch url (n - 1)

let rec badFactorial n = if n <= 0 then 1 else n * badFactorial n

let rec even n = (n = 0u) || odd(n - 1u)
and     odd n = (n <> 0u) && even(n - 1u)
//val even : n:uint32 -> bool
//val odd : n:uint32 -> bool

let even (n:uint32) = (n % 2u) = 0u
let odd  (n:uint32) = (n % 2u) = 1u

let oddPrimes = [3; 5; 7; 11]
let morePrimes = [13; 17]
let primes = 2 :: (oddPrimes @ morePrimes)
//val primes : int list = [2; 3; 5; 7; 11; 13; 17]
> let people = ["Adam"; "Dominic"; "James" ;;
//val people : string list = ["Adam"; "Dominic"; "James"]
> "Chris" :: people;;
//val it : string list = ["Chris"; "Adam"; "Dominic"; "James"]
> people;;
//val it : string list = ["Adam"; "Dominic"; "James"]

//List.length	'T list -> int	Returns the length of the list.
//List.head	'T list -> 'T	Returns the first element of a nonempty list.
//List.tail	'T list -> 'T list	Returns all the elements of a nonempty list except the first.
//List.init	int -> (int -> 'T) -> 'T list	Returns a new list. The length of the new list is specified by the first parameter. The second parameter must be a generating function that maps list indexes to values.
//List.append	'T list -> 'T list -> 'T list	Returns a new list containing the elements of the first list followed by the elements of the second list.
//List.filter	('T -> bool) -> 'T list -> 'T list	Returns a new list containing only those elements of the original list where the function returns true.
//List.map	('T -> 'U ) -> 'T list -> 'U list	Creates a new list by applying a function to each element of the original list.
//List.iter	('T -> unit) -> 'T list -> unit	Executes the given function for each element of the list.
//List.unzip	('T * 'U) list -> 'T list * 'U list	Returns two new lists containing the first and second elements of the pairs in the input list.
//List.zip	'T list -> 'U list -> ('T * 'U) list	Returns a new list containing the elements of the two input lists combined pairwise. The input lists must be the same length; otherwise, an exception is raised.
//List.toArray	'T list -> 'T[]	Creates an array from the given list.
//List.ofArray	'T[]    -> 'T list	Creates a list from the given array.

> List.head [5; 4; 3];;
//val it : int = 5
> List.tail [5; 4; 3];;
//val it : int list = [4; 3]
> List.map (fun x -> x * x) [1; 2; 3];;
//val it : int list = [1; 4; 9]
> List.filter (fun x -> x % 3 = 0) [2; 3; 5; 7; 9];;
//val it : int list = [3; 9]

type 'T option =
    | None
    | Some of 'T

> let people = [("Adam", None);
                ("Eve" , None);
                ("Cain", Some("Adam","Eve"));
                 "Abel", Some("Adam","Eve"))];;
//val people : (string * (string * string) option) list =
//  [("Adam", None); ("Eve", None); ("Cain", Some ("Adam", "Eve"));
//   ("Abel", Some ("Adam", "Eve"))]

let fetch url =
    try Some (http url)
    with :? System.Net.WebException -> None

> match (fetch "http://www.nature.com") with
  | Some text -> printfn "text = %s" text
  | None -> printfn "**** no web page found";;

//text = <!DOCTYPE html PUBLIC ...  (note: the HTML is shown here if connected to the web)

let isLikelySecretAgent url agent =
    match (url, agent) with
    | "http://www.control.org", 99 -> true
    | "http://www.control.org", 86 -> true
    | "http://www.kaos.org" , _  -> true
    | _ -> false

let printFirst primes =
    match primes with
    | h :: t -> printfn "The first prime in the list is %d" h
    | [] -> printfn "No primes found in the list"

> printFirst oddPrimes;;
//The first prime in the list is 3
//val it : unit = ()

> let showParents (name, parents) =
      match parents with
      | Some (dad, mum) -> printfn "%s has father %s, mother %s" name dad mum
      | None -> printfn "%s has no parents!" name;;

//val showParents : name:string * parents:(string * string) option -> unit

> showParents ("Adam", None);;
//Adam has no parents!

let highLow a b =
    match (a, b) with
    | ("lo", lo), ("hi", hi) -> (lo, hi)
    | ("hi", hi), ("lo", lo) -> (lo, hi)
    | _ -> failwith "expected a both a high and low value"

> highLow ("hi", 300) ("lo", 100);;
//val it : int * int = (100, 300)

> let urlFilter3 url agent =
      match url, agent with
      | "http://www.control.org", 86 -> true
      | "http://www.kaos.org", _ -> false;;

//Script.fsx(157,13): warning FS0025: Incomplete pattern matches on this expression. For example, the value '(_,0)' may indicate a case not covered by the pattern(s).
//val urlFilter3 : url:string -> agent:int -> bool

let urlFilter4 url agent =
    match url, agent with
    | "http://www.control.org", 86 -> true
    | "http://www.kaos.org", _ -> false
    | _ -> failwith "unexpected input"

> let urlFilter2 url agent =
      match url, agent with
      | "http://www.control.org", _ -> true
      | "http://www.control.org", 86 -> true
      | _ -> false;;

//Script.fsx(173,9): warning FS0026: This rule will never be matched
//val urlFilter2 : url:string -> agent:int -> bool

let sign x =
    match x with
    | _ when x < 0 -> -1
    | _ when x > 0 ->  1
    | _ -> 0

let getValue a =
    match a with
    | (("lo" | "low") ,v) -> v
    | ("hi", v) | ("high", v) -> v
    | _ -> failwith "expected a both a high and low value"

> let sites = ["http://www.live.com"; "http://www.google.com"];;
//val sites : string list = ["http://www.live.com"; "http://www.google.com"];;

> let fetch url = (url, http url);;
//val fetch : url:string -> string * string

> List.map fetch sites;;
//val it : (string * string) list =
//  [("http://www.live.com",
//    "<!-- ServerInfo:...
//</body></html>");
//   ("http://www.google.com",
//    "<!doctype html>...
//</script>")]    

> List.map;;
//val it : (('a -> 'b) -> 'a list -> 'b list) = <fun:clo@210-2>

> let primes = [2; 3; 5; 7];;
//val primes : int list = [2; 3; 5; 7]

> let primeCubes = List.map (fun n -> n * n * n) primes;;
//val primeCubes: int list = [8; 27; 125; 343] 

let resultsOfFetch = List.map (fun url -> (url, http url)) sites
val resultsOfFetch : (string * string) list =
//  [("http://www.live.com",
//    "<!-- ServerInfo: BAYIDSLGN1J53 2012.04.24.15.52.18 Live1 Unkn"+[12073 chars]);
//   ("http://www.google.com",
//    "<!doctype html><html itemscope itemtype="http://schema.org/We"+[45073 chars])]

> List.map (fun (_, p) -> String.length p) resultsOfFetch;;
//val it : int list = [12134; 45134]

let delimiters = [|' '; '\n'; '\t'; '<'; '>'; '='|]
//val delimiters : char [] = [|' '; '\010'; '\009'; '<'; '>'; '='|]

let getWords (s:string) = s.Split delimiters
//val getWords : s:string -> string []

let getStats site =
    let url = "http://" + site
    let html = http url
    let hwords = html |> getWords
    let hrefs = html |> getWords |> Array.filter (fun s -> s = "href")
    (site, html.Length, hwords.Length, hrefs.Length)
//val getStats : site:string -> string * int * int * int

> let sites = ["www.live.com"; "www.google.com"; "search.yahoo.com"];;
//val sites : string list =
//  ["www.live.com"; "www.google.com"; "search.yahoo.com"]

> sites |> List.map getStats;;
//val it : (string * int * int * int) list =
//  [("www.live.com", 12132, 892, 11); ("www.google.com", 44139, 2586, 33);
//   ("search.yahoo.com", 12617, 846, 23)]

List.map	('T -> 'U) -> 'T list -> 'U list
Array.map	('T -> 'U) -> 'T [] -> 'U []
Option.map	('T -> 'U) -> 'T option -> 'U option
Seq.map	('T -> 'U) -> seq<'T> -> seq<'U>

site.Map getStats
// TODO:    Work out how to get the above working

[1; 2; 3] |> List.map (fun x -> x * x * x)
//val it : int list = [1; 8; 27], just as if you had written this:
List.map (fun x -> x * x * x) [1; 2; 3]
(|>);;
//val it : ('a -> ('a -> 'b) -> 'b) = <fun:it@262-11>

let google = http "http://www.google.com"
//val google : string =
//  "<!doctype html><html itemscope itemtype="http://schema.org/We"+[44114 chars]

google |> getWords |> List.filter (fun s -> s = "href") |> List.length
//Script.fsx(270,23): error FS0001: Type mismatch. Expecting a
//    string [] -> 'a    
//but given a
//    'b list -> 'b list    
//The type 'string []' does not match the type ''a list'

let countLinks = getWords >> List.filter (fun s -> s = "href") >> List.length
//Script.fsx(277,30): error FS0001: Type mismatch. Expecting a
//    string [] -> 'a    
//but given a
//    'b list -> 'b list    
//The type 'string []' does not match the type ''a list'

google |> countLinks
//Script.fsx(284,11): error FS0039: The value or constructor 'countLinks' is not defined

let (>>) f g x = g(f(x))
//val ( >> ) : f:('a -> 'b) -> g:('b -> 'c) -> x:'a -> 'c
(>>);;
//val it : (('a -> 'b) -> ('b -> 'c) -> 'a -> 'c) = <fun:clo@289-3>

let shift (dx, dy) (px, py) = (px + dx, py + dy)
//val shift : dx:int * dy:int -> px:int * py:int -> int * int

let shiftRight = shift (1, 0)
let shiftUp = shift (0, 1)
let shiftLeft = shift (-1, 0)
let shiftDown = shift (0, -1)

//val shiftRight : (int * int -> int * int)
//val shiftUp : (int * int -> int * int)
//val shiftLeft : (int * int -> int * int)
//val shiftDown : (int * int -> int * int)

> shiftRight (10, 10);;
//val it : int * int = (11, 10)

> List.map (shift (2,2)) [(0,0); (1,0); (1,1); (0,1)];;
//val it : (int * int) list = [(2, 2); (3, 2); (3, 3); (2, 3)]

open System.Drawing

let remap (r1:Rectangle) (r2:Rectangle) =
    let scalex = float r2.Width / float r1.Width
    let scaley = float r2.Height / float r1.Height
    let mapx x = int (float r2.Left + truncate (float (x - r1.Left) * scalex))
    let mapy y = int (float r2.Top + truncate (float (y - r1.Top) * scaley))
    let mapp (p:Point) = Point(mapx p.X, mapy p.Y)
    mapp

//val remap :
//  r1:System.Drawing.Rectangle ->
//    r2:System.Drawing.Rectangle ->
//      (System.Drawing.Point -> System.Drawing.Point)

> let mapp = remap (Rectangle(100, 100, 100, 100)) (Rectangle(50, 50, 200, 200));;
//val mapp : Point -> Point

> mapp (Point(100, 100));;
//val it : Point = {X=50,Y=50} {IsEmpty = false;
//                              X = 50;
//                              Y = 50;}

> mapp (Point(150, 150));;
//val it : Point = {X=150,Y=150} {IsEmpty = false;
//                                X = 150;
//                                Y = 150;}

> mapp (Point(200, 200));;
//val it : Point = {X=250,Y=250} {IsEmpty = false;
//                                X = 250;
//                                Y = 250;}

let sites = ["http://www.live.com";
             "http://www.google.com";
             "http://search.yahoo.com"]

sites |> List.iter (fun site -> printfn "%s, length = %d" site (http site).Length)
//http://www.live.com, length = 12137
//http://www.google.com, length = 45140
//http://search.yahoo.com, length = 12617
//val it : unit = ()

> open System;;
 let start = DateTime.Now;;
//val start : DateTime = 13/06/2012 9:54:36 p.m.

 http "http://www.newscientist.com";;
//val it : string = "<!DOCTYPE html...</html>"

 let finish = DateTime.Now;;
//val finish : DateTime = 13/06/2012 9:54:37 p.m.

> let elapsed = finish - start;;
//val elapsed : TimeSpan = 00:00:01.1550660

open System

let time f =
    let start = DateTime.Now
    let res = f()
    let finish = DateTime.Now
    (res, finish - start)
//val time : f:(unit -> 'a) -> 'a * TimeSpan

> time (fun () -> http "http://www.newscientist.com");;
//val it : string * TimeSpan = ...   (The HTML text and time will be shown here)
> open System.IO;;

> [@"C:\Program Files"; @"C:\Windows"] |> List.map Directory.GetDirectories;;
//val it : string [] list =
//  [[|"C:\Program Files\7-Zip"; "C:\Program Files\Application Verifier";
//     "C:\Program Files\Common Files"; "C:\Program Files\DVD Maker";...
//     "C:\Windows\Vss"; "C:\Windows\Web"; "C:\Windows\winsxs"|]]

> open System;;

> let f = Console.WriteLine;;
//Script.fsx(388,9): error FS0041: A unique overload for method 'WriteLine' could not be determined based on type information prior to this program point. A type annotation may be needed. Candidates: Console.WriteLine(buffer: char []) : unit, Console.WriteLine(format: string, params arg: obj []) : unit, Console.WriteLine(value: bool) : unit, Console.WriteLine(value: char) : unit, Console.WriteLine(value: decimal) : unit, Console.WriteLine(value: float) : unit, Console.WriteLine(value: float32) : unit, Console.WriteLine(value: int) : unit, Console.WriteLine(value: int64) : unit, Console.WriteLine(value: obj) : unit, Console.WriteLine(value: string) : unit, Console.WriteLine(value: uint32) : unit, Console.WriteLine(value: uint64) : unit

> let f = (Console.WriteLine:string -> unit);;
//val f : arg00:string -> unit

> seq {0 .. 2};;
//val it : seq<int> = seq [0; 1; 2]

> seq {-100.0 .. 100.0};;
//val it : seq<float> = seq [-100.0; -99.0; -98.0; -97.0; ...]

> seq {1I .. 1000000000000I};;
//val it : seq<Numerics.BigInteger> =
//  seq [1 {IsEven = false;
//          IsOne = true;
//          IsPowerOfTwo = true;
//          IsZero = false;
//          Sign = 1;}; ...]

> seq {1 .. 2 .. 5};;
//val it : seq<int> = seq [1; 3; 5]

> seq { 1 .. -2 .. -5};;
//val it : seq<int> = seq [1; -1; -3; -5]

> seq {0 .. 2 .. 5};;
//val it : seq<int> = seq [0; 2; 4]

> let range = seq {0 .. 2 .. 6};;
//val range : seq<int>

> for i in range do
      printfn "i = %d" i;;
//i = 0
//i = 2
//i = 4
//i = 6
//val it : unit = ()

> let range = seq {0 .. 10};;
//val range : seq<int>

> range |> Seq.map (fun i -> (i, i * i));;
//val it : seq<int * int> = seq [(0, 0); (1, 1); (2, 4); (3, 9); ...]

//Seq.append	: seq<'T> -> seq<'T> -> seq<'T>
//Seq.concat	: seq< seq<'T> >  -> seq<'T>
//Seq.choose	: ('T -> 'U option) -> seq<'T> -> seq<'U>
//Seq.delay	: (unit -> seq<'T>) -> seq<'T>
//Seq.empty	seq<'T>
//Seq.iter	: ('T -> unit) -> seq<'T> -> unit
//Seq.filter	: ('T -> bool) -> seq<'T> -> seq<'T>
//Seq.map	: ('T -> 'U) -> seq<'T> -> seq<'U>
//Seq.singleton	: 'T -> seq<'T>
//Seq.truncate	: int -> seq<'T> -> seq<'T>
//Seq.toList	: seq<'T> -> 'T list
//Seq.ofList	: 'T list -> seq<'T>
//Seq.toArray	: seq<'T> -> 'T[]
//Seq.ofArray	: 'T[] -> seq<'T>

open System.IO
let rec allFiles dir =
    Seq.append
        (dir |> Directory.GetFiles)
        (dir |> Directory.GetDirectories |> Seq.map allFiles |> Seq.concat)
//val allFiles : dir:string -> seq<string>

> allFiles @"c:\WINDOWS\system32";;

//val it : seq<string> =
//  seq
//    ["c:\WINDOWS\system32\12520437.cpx"; "c:\WINDOWS\system32\12520850.cpx";
//     "c:\WINDOWS\system32\aaclient.dll";
//     "c:\WINDOWS\system32\accessibilitycpl.dll"; ...]

> let squares = seq {for i in 0 .. 10 -> (i, i * i)};;
//val squares : seq<int * int>

> seq {for (i, iSquared) in squares -> (i, iSquared, i * iSquared)};;
//val it : seq<int * int * int> =
//  seq [(0, 0, 0); (1, 1, 1); (2, 4, 8); (3, 9, 27); ...]

let checkerboardCoordinates n = 
   seq {for row in 1 .. n do
            for col in 1 .. n do
                if (row + col) % 2 = 0 then
                    yield (row, col)}
//val checkerboardCoordinates : n:int -> seq<int * int>

> checkerboardCoordinates 3;;
//val it : seq<int * int> = seq [(1, 1); (1, 3); (2, 2); (3, 1); ...]

let fileInfo dir =
    seq {for file in Directory.GetFiles dir do
            let creationTime = File.GetCreationTime file 
            let lastAccessTime = File.GetLastAccessTime file
            yield (file,creationTime,lastAccessTime)}
//val fileInfo : dir:string -> seq<string * DateTime * DateTime>

let rec allFiles dir =
    seq {for file in Directory.GetFiles dir do
             yield file
         for subdir in Directory.GetDirectories dir do 
             yield! allFiles subdir}
//val allFiles : dir:string -> seq<string>

> [1 .. 4];;
//val it : int list = [1; 2; 3; 4]

> [for i in 0 .. 3 -> (i, i * i)];;
//val it : (int * int) list = [(0, 0); (1, 1); (2, 4); (3, 9)]

> [|for i in 0 .. 3 -> (i, i * i)|];;
//val it : (int * int) [] = [|(0, 0); (1, 1); (2, 4); (3, 9)|]
