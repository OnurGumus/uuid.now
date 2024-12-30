module Client.App

open Fable.Core
open Browser
open Fable.Core.JsInterop
open Browser.Types

// Characters used for uppercase v4 or time-based GUIDs, including dash
let charList =
    [| "0"
       "1"
       "2"
       "3"
       "4"
       "5"
       "6"
       "7"
       "8"
       "9"
       "A"
       "B"
       "C"
       "D"
       "E"
       "F"
       "-" |]

let FLIP_COUNT = 36 // 8-4-4-4-12

let board = document.getElementById("flipBoard")

// Build 36 flips
for i = 0 to FLIP_COUNT - 1 do
    let flip = document.createElement("div")
    flip.className <- "flip"
    flip.dataset?value <- ""
    flip.dataset?top <- "0"

    let ul = document.createElement("ul")
    ul?style?top <- "0px"

    charList
    |> Array.iter (fun ch ->
        let li = document.createElement("li")
        li.textContent <- ch
        ul.appendChild(li) |> ignore)

    flip.appendChild(ul) |> ignore

    let overlay = document.createElement("div")
    overlay.className <- "final-char"
    overlay.textContent <- "-"
    flip.appendChild(overlay) |> ignore

    board.appendChild(flip) |> ignore

// Move UL down 35px
let moveFlipDown (flipEl: HTMLElement) =
    let currentTop =
        match flipEl.dataset?top |> Option.ofObj with
        | Some s -> 
            match System.Int32.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture) with
            | (true, value) -> value
            | _ -> 0
        | None -> 0

    let newTop = currentTop - 35
    flipEl.dataset?top <- newTop.ToString()

    let ul = flipEl.querySelector("ul")
    ul?style?top <- sprintf "%dpx" newTop

// Reset UL position to top
let resetFlip (flipEl: HTMLElement) =
    flipEl.dataset?top <- "0"
    (flipEl.querySelector("ul"))?style?top <- "0px"

// Corrected switchChar function with proper index handling
let switchChar (flipEl: HTMLElement) (targetChar: string) =
    let currentVal = flipEl.dataset?value |> Option.defaultValue ""

    if currentVal = targetChar then
        ()
    else
        // Safely find the index of currentVal
        let idxOpt = Array.tryFindIndex ((=) currentVal) charList

        // Set idx to 0 if currentVal is not found
        let idx = defaultArg idxOpt 0

        // Safely find the index of targetChar
        let targetIdxOpt = Array.tryFindIndex ((=) targetChar) charList

        match targetIdxOpt with
        | None ->
            // Target character not found; optionally handle this case
            ()
        | Some targetIdx ->
            let rec step () =
                let nowVal = flipEl.dataset?value |> Option.defaultValue ""

                if nowVal = targetChar then
                    ()
                else
                    // Safely find the index of nowVal
                    let iOpt = Array.tryFindIndex ((=) nowVal) charList

                    // Determine the next index
                    let nextI =
                        match iOpt with
                        | Some i -> i + 1
                        | None -> 0

                    // Determine the actual index to use, wrapping around if necessary
                    let actualI, shouldReset =
                        if nextI >= charList.Length then
                            (0, true)
                        else
                            (nextI, false)

                    // Perform actions based on whether we need to reset
                    if shouldReset then
                        resetFlip flipEl
                    else
                        moveFlipDown flipEl

                    // Update the character only if the index is valid
                    if actualI >= 0 && actualI < charList.Length then
                        let nextC = charList.[actualI]
                        flipEl.dataset?value <- nextC

                        // Update overlay each step
                        let overlay = flipEl.querySelector(".final-char") :?> HTMLElement
                        overlay.textContent <- nextC

                        // Continue stepping if target not reached
                        if nextC <> targetChar then
                            window.setTimeout(step, 80) |> ignore

            step ()

// Switch the entire board to a specific GUID string
let switchToGuid (guidStr: string) =
    let flips = board.querySelectorAll(".flip")

    for i = 0 to FLIP_COUNT - 1 do
        let flipEl = flips.[i] :?> HTMLElement
        let c = if i < guidStr.Length then guidStr.[i].ToString() else "-"
        window.setTimeout((fun () -> switchChar flipEl c), i * 50) |> ignore

// Generate a random v4 GUID using Crypto API
let getRandomV4Guid () =
    window?crypto?randomUUID().ToString().ToUpperInvariant()

// Generates a time-based GUID in the format `XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX`
let getTimeBasedGuid () =
    // 1. Get the current Unix timestamp in seconds and convert to an 8-character hexadecimal string
    let timestamp =
        System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        |> int
        |> sprintf "%08x"

    // 2. Generate 12 random bytes using the Crypto API
    let randomBytes = window?crypto?getRandomValues(JS.Uint8Array.Create(12))

    // 3. Convert each byte to a two-character hexadecimal string and concatenate
    let rndHex =
        randomBytes
        |> Seq.cast<byte>
        |> Seq.map (fun b -> sprintf "%02x" b)
        |> String.concat ""

    // 4. Split the random hex string into parts: 4-4-4-12
    let part1 = rndHex.Substring(0, 4)
    let part2 = rndHex.Substring(4, 4)
    let part3 = rndHex.Substring(8, 4)
    let part4 = rndHex.Substring(12)

    // 5. Combine the timestamp and random parts into the final GUID format
    let res = sprintf "%s-%s-%s-%s-%s" timestamp part1 part2 part3 part4
    res.ToUpperInvariant()

// Retrieve button elements
let resetBtn = document.getElementById("resetBtn")
let v4Btn = document.getElementById("v4Btn")
let timeBtn = document.getElementById("timeBtn")
let copyBtn = document.getElementById("copyBtn")

// Add event listeners to buttons
resetBtn.addEventListener(
    "click",
    fun _ ->
        let zero = "00000000-0000-0000-0000-000000000000"
        switchToGuid zero
)

v4Btn.addEventListener(
    "click",
    fun _ ->
        let g = getRandomV4Guid ()
        switchToGuid g
)

timeBtn.addEventListener("click", fun _ ->
    let t = getTimeBasedGuid()
    switchToGuid t
)

copyBtn.addEventListener("click", fun _ ->
    let mutable finalStr = ""
    let flips = board.querySelectorAll(".flip")
    for i = 0 to flips.length - 1 do
        let flipEl = flips.[i] :?> HTMLElement
        let over = flipEl.querySelector(".final-char")
        finalStr <- finalStr + (if over?textContent = null then "-" else over?textContent)

    window?navigator?clipboard?writeText(finalStr) |> ignore
    window.alert("Copied:\n" + finalStr)
)

// Initialize the board with zero GUID on DOMContentLoaded
document.addEventListener("DOMContentLoaded", fun _ ->
    let zeroGUID = "00000000-0000-0000-0000-000000000000"
    switchToGuid zeroGUID
)
