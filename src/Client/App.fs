module Client.App

open Fable.Core
open Browser
open Fable.Core.JsInterop
open Browser.Types

//------------------------------------------
// Active Patterns and Helpers
//------------------------------------------
open System
open System.Globalization

/// Active pattern to parse integer from string.
let (|ParseInt|_|) (s: string) =
    match Int32.TryParse(s) with
    | true, v -> Some v
    | _ -> None

/// Safely read dataset property as string, or return None
let getData (el: HTMLElement) (prop: string) = el.dataset?(prop) |> Option.ofObj

/// Safely read dataset property as int, or return defaultVal
let getDataInt defaultVal (el: HTMLElement) (prop: string) =
    match getData el prop with
    | Some(ParseInt v) -> v
    | _ -> defaultVal

/// Set dataset property
let setData (el: HTMLElement) (prop: string) (value: string) = el.dataset?(prop) <- value

/// Shortcut to get the current "value" from the flip
let getFlipValue (flipEl: HTMLElement) =
    getData flipEl "value" |> Option.defaultValue ""

/// Shortcut to set the "value" on the flip
let setFlipValue (flipEl: HTMLElement) (v: string) = setData flipEl "value" v

/// Get the overlay element
let getOverlay (flipEl: HTMLElement) =
    flipEl.querySelector (".final-char") :?> HTMLElement

/// Update the overlay text
let setOverlayText (flipEl: HTMLElement) (text: string) =
    getOverlay flipEl |> fun o -> o.textContent <- text

//------------------------------------------
// Constants and Globals
//------------------------------------------

// Characters used for uppercase v4 or time-based GUIDs, including dash
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

//------------------------------------------
// Build the initial board
//------------------------------------------
for _i in 0 .. FLIP_COUNT - 1 do
    let flip = document.createElement ("div")
    flip.className <- "flip"
    setData flip "value" ""
    setData flip "top" "0"

    let ul = document.createElement ("ul")
    ul?style?top <- "0px"

    // append characters
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

    board.appendChild (flip) |> ignore

//------------------------------------------
// Flip Movement
//------------------------------------------
let moveFlipDown (flipEl: HTMLElement) =
    let currentTop = getDataInt 0 flipEl "top"
    let newTop = currentTop - 35
    setData flipEl "top" (string newTop)
    let ul = flipEl.querySelector ("ul")
    ul?style?top <- sprintf "%dpx" newTop

let resetFlip (flipEl: HTMLElement) =
    setData flipEl "top" "0"
    let ul = flipEl.querySelector ("ul")
    ul?style?top <- "0px"

//------------------------------------------
// Switching Characters
//------------------------------------------
let switchChar (flipEl: HTMLElement) (targetChar: string) =
    let currentVal = getFlipValue flipEl

    if currentVal = targetChar then
        ()
    else
        let rec step () =
            let nowVal = getFlipValue flipEl

            if nowVal = targetChar then
                ()
            else
                // next index
                let iOpt = Array.tryFindIndex ((=) nowVal) charList

                let nextI =
                    match iOpt with
                    | Some i -> i + 1
                    | None -> 0
                // wrap around?
                let (actualI, shouldReset) =
                    if nextI >= charList.Length then
                        (0, true)
                    else
                        (nextI, false)

                if shouldReset then
                    resetFlip flipEl
                else
                    moveFlipDown flipEl

                let nextC = charList.[actualI]
                setFlipValue flipEl nextC
                setOverlayText flipEl nextC

                // Keep going if not there yet
                if nextC <> targetChar then
                    window.setTimeout (step, 80) |> ignore

        step ()

//------------------------------------------
// Switch the entire board to a GUID
//------------------------------------------
let switchToGuid (guidStr: string) =
    let flips = board.querySelectorAll (".flip")

    for i in 0 .. FLIP_COUNT - 1 do
        let flipEl = flips.[i] :?> HTMLElement
        let c = if i < guidStr.Length then guidStr.[i].ToString() else "-"
        window.setTimeout ((fun () -> switchChar flipEl c), i * 50) |> ignore

//------------------------------------------
// Generate random v4 GUID
//------------------------------------------
let getRandomV4Guid () =
    window?crypto?randomUUID().ToString().ToUpperInvariant()

//------------------------------------------
// Generate a time-based GUID
//------------------------------------------
let getTimeBasedGuid () =
    // 1. current Unix timestamp => 8-char hex
    let timestamp =
        System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() |> int |> sprintf "%08x"

    // 2. 12 random bytes from Crypto
    let randomBytes = window?crypto?getRandomValues (JS.Uint8Array.Create(12))

    // 3. Convert bytes to hex
    let rndHex =
        randomBytes
        |> Seq.cast<byte>
        |> Seq.map (fun b -> sprintf "%02x" b)
        |> String.concat ""

    // 4. Split into 4-4-4-12
    let part1 = rndHex.Substring(0, 4)
    let part2 = rndHex.Substring(4, 4)
    let part3 = rndHex.Substring(8, 4)
    let part4 = rndHex.Substring(12)

    // 5. Combine
    sprintf "%s-%s-%s-%s-%s" timestamp part1 part2 part3 part4
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
        let flips = board.querySelectorAll (".flip")
        let mutable finalStr = ""

        for i = 0 to flips.length - 1 do
            let flipEl = flips.[i] :?> HTMLElement
            let over = flipEl.querySelector (".final-char") :?> HTMLElement
            let text = if isNull over.textContent then "-" else over.textContent
            finalStr <- finalStr + text

        window?navigator?clipboard?writeText (finalStr) |> ignore
        window.alert ("Copied:\n" + finalStr)
)

//------------------------------------------
// Initialize the board on DOMContentLoaded
//------------------------------------------
document.addEventListener ("DOMContentLoaded", fun _ -> switchToGuid "00000000-0000-0000-0000-000000000000")
