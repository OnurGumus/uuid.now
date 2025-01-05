module Client.FlipBoard


open Fable.Core
open Browser
open Browser.Types
open Fable.Core.JsInterop
open System

//------------------------------------------
// Active Patterns and Helpers
//------------------------------------------
let (|ParseInt|_|) (s: string) =
    match Int32.TryParse(s) with
    | true, v -> Some v
    | _ -> None

let getData (el: HTMLElement) (prop: string) = el.dataset?(prop) |> Option.ofObj

let setData (el: HTMLElement) (prop: string) (value: string) = el.dataset?(prop) <- value

let getDataInt (defaultVal: int) (el: HTMLElement) (prop: string) =
    match getData el prop with
    | Some(ParseInt v) -> v
    | _ -> defaultVal

/// Shortcut to get the "value" dataset.
let flipValue flipEl =
    getData flipEl "value" |> Option.defaultValue ""

let setFlipValue flipEl v = setData flipEl "value" v

/// Overlay element
let overlay (flipEl: HTMLElement) =
    flipEl.querySelector (".final-char") :?> HTMLElement

let setOverlayText flipEl text = (overlay flipEl).textContent <- text

//------------------------------------------
// Constants
//------------------------------------------
let charList =
    [| '0'
       '1'
       '2'
       '3'
       '4'
       '5'
       '6'
       '7'
       '8'
       '9'
       'A'
       'B'
       'C'
       'D'
       'E'
       'F'
       '-' |]
    |> Array.map string

let FLIP_COUNT = 36 // 8-4-4-4-12

let board = document.getElementById ("flipBoard")

// Add a hidden aria-live region for final UUID announcements
let srValue = document.createElement ("div")
srValue.id <- "screenReaderValue"
srValue.setAttribute ("aria-live", "polite")
srValue?style?position <- "absolute"
srValue?style?left <- "-9999px"
document.body.appendChild (srValue) |> ignore

//------------------------------------------
// Building the Board
//------------------------------------------
let buildFlip () =
    let flip = document.createElement ("div")
    flip.className <- "flip"
    setData flip "value" ""
    setData flip "top" "0"
    flip.setAttribute ("aria-hidden", "true")

    let ul = document.createElement ("ul")
    ul?style?insetBlockStart <- "0px"

    charList
    |> Array.iter (fun ch ->
        let li = document.createElement ("li")
        li.textContent <- ch
        ul.appendChild (li) |> ignore)

    flip.appendChild (ul) |> ignore

    let overlay = document.createElement ("div")
    overlay.className <- "final-char"
    overlay.textContent <- "-"
    flip.appendChild (overlay) |> ignore

    flip

[ 0 .. FLIP_COUNT - 1 ]
|> List.map (fun _ -> buildFlip ())
|> List.iter (board.appendChild >> ignore)

//------------------------------------------
// Querying Flips Already on the Board
//------------------------------------------
let nodeListToArray (nodeList: NodeListOf<'T>) =
    [| for i in 0 .. nodeList.length - 1 do
           yield nodeList.[i] |]

let boardFlips () =
    board.querySelectorAll (".flip")
    |> nodeListToArray
    |> Array.map (fun el -> el :?> HTMLElement)

//------------------------------------------
// Flip Movement
//------------------------------------------
let resetFlip (flipEl: HTMLElement) =
    setData flipEl "top" "0"
    let ul = flipEl.querySelector ("ul")
    ul?style?insetBlockStart <- "0px"

let moveFlipDown (flipEl: HTMLElement) =
    let currentTop = getDataInt 0 flipEl "top"
    let newTop = currentTop - 35
    setData flipEl "top" (string newTop)
    let ul = flipEl.querySelector ("ul")
    ul?style?insetBlockStart <- sprintf "%dpx" newTop

//------------------------------------------
// Switching Characters
//------------------------------------------
let switchChar (flipEl: HTMLElement) (targetChar: string) =
    let currentVal = flipValue flipEl

    if currentVal = targetChar then
        ()
    else
        let rec step () =
            let nowVal = flipValue flipEl

            if nowVal = targetChar then
                ()
            else
                let iOpt = Array.tryFindIndex ((=) nowVal) charList
                let nextI = iOpt |> Option.map ((+) 1) |> Option.defaultValue 0

                let actualI, shouldReset =
                    if nextI >= charList.Length then 0, true else nextI, false

                if shouldReset then
                    resetFlip flipEl
                else
                    moveFlipDown flipEl

                let nextC = charList.[actualI]
                setFlipValue flipEl nextC
                setOverlayText flipEl nextC

                if nextC <> targetChar then
                    window.setTimeout (step, 80) |> ignore

        step ()

//------------------------------------------
// Switch the entire Board to a GUID
//------------------------------------------
let switchToGuid (guidStr: string) =
    boardFlips ()
    |> Array.iteri (fun i flipEl ->
        let c = if i < guidStr.Length then guidStr.[i].ToString() else "-"
        // Stagger the flips a bit
        window.setTimeout ((fun () -> switchChar flipEl c), i * 50) |> ignore)

    window.setTimeout (
        (fun () ->
            let srValEl = document.getElementById ("screenReaderValue")

            if not (isNull srValEl) then
                srValEl.textContent <- guidStr),
        FLIP_COUNT * 60
    )
    |> ignore

//------------------------------------------
// Generate random v4 GUID
//------------------------------------------
let getRandomV4Guid () =
    window?crypto?randomUUID().ToString().ToUpperInvariant()

//------------------------------------------
// Generate a time-based GUID
//------------------------------------------
let getTimeBasedGuid () =
    // Get Unix timestamp in milliseconds
    let timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    
    // Get 10 random bytes for the remaining fields
    let randomBytes = 
        window?crypto?getRandomValues(JS.Constructors.Uint8Array.Create(10))
        |> Seq.cast<byte>
        |> Seq.map (fun b -> sprintf "%02x" b)
        |> String.concat ""
    
    // Format timestamp as hex (48 bits = 12 hex chars)
    let timestampHex = sprintf "%012x" timestamp
    
    // First 48 bits are timestamp
    let timeHigh = timestampHex.Substring(0, 8)  // First 32 bits
    let timeMid = timestampHex.Substring(8, 4)   // Next 16 bits
    
    // Version 7
    let version = "7"
    let rand1 = randomBytes.Substring(0, 3)
    let verRand = version + rand1
    
    // Variant bits (10xx)
    let varByte = 
        let randByte = Convert.ToByte(randomBytes.Substring(3, 2), 16)
        sprintf "%02x" ((randByte &&& byte 0x3F) ||| byte 0x80)
    
    let remaining = randomBytes.Substring(5)
    
    sprintf "%s-%s-%s-%s%s-%s" 
        timeHigh timeMid verRand varByte 
        (randomBytes.Substring(4, 1)) remaining
    |> fun s -> s.ToUpperInvariant()

//------------------------------------------
// Button Handlers
//------------------------------------------
let resetBtn = document.getElementById ("resetBtn")
let v4Btn = document.getElementById ("v4Btn")
let timeBtn = document.getElementById ("timeBtn")
let copyBtn = document.getElementById ("copyBtn")

resetBtn.addEventListener ("click", fun _ -> switchToGuid "00000000-0000-0000-0000-000000000000")

v4Btn.addEventListener ("click", fun _ -> getRandomV4Guid () |> switchToGuid)

timeBtn.addEventListener ("click", fun _ -> getTimeBasedGuid () |> switchToGuid)

copyBtn.addEventListener (
    "click",
    fun _ ->
        let finalStr =
            boardFlips ()
            |> Array.fold
                (fun acc flipEl ->
                    let text = (overlay flipEl).textContent
                    let c = if isNull text then "-" else text
                    acc + c)
                ""

        window?navigator?clipboard?writeText (finalStr) |> ignore
        let toast = document.getElementById ("toast")
        toast?showPopover ()
)

document.addEventListener (
    "DOMContentLoaded",
    fun _ ->
        let toast = document.getElementById ("toast")

        toast.addEventListener (
            "toggle",
            fun (event) ->
                let target = event.currentTarget

                if (event.currentTarget?matches (":popover-open")) then
                    window.setTimeout ((fun () -> target?hidePopover ()), 5000, [||]) |> ignore
        )
)
