module Client.FancyButton

open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Client.WebComponent
open Browser.Dom

let openAnimation =
    [| createObj [ "transform" ==> "scale(0)"; "opacity" ==> 1 ]
       createObj [ "transform" ==> "scale(4)"; "opacity" ==> 0 ] |]

type FancyButton() =
    inherit HTMLButtonElement()
    let self = jsThis


    let createRipple (event: MouseEvent) =
        let button = event.currentTarget :?> HTMLButtonElement
        let diameter = max button.clientWidth button.clientHeight
        let radius = float diameter / 2.0

        let circle = document.createElement ("button-ripple")

        let styles =
            createObj
                [ "inlineSize" ==> sprintf "%dpx" diameter
                  "blockSize" ==> sprintf "%dpx" diameter
                  "inset-inline-start"
                  ==> sprintf "%fpx" (event.clientX - float button.offsetLeft - radius)
                  "inset-block-start"
                  ==> sprintf "%fpx" (event.clientY - float button.offsetTop - radius)
                  "position" ==> "absolute"
                  "borderRadius" ==> "50%"
                  "transform" ==> "scale(0)"
                  "backgroundColor" ==> "rgba(255, 255, 255, 0.7)" ]

        JS.Constructors.Object.assign (circle?style, styles) |> ignore
        button.appendChild (circle) |> ignore

        let animation =
            circle?animate (openAnimation, createObj [ "duration" ==> 600; "iterations" ==> 1; "easing" ==> "ease-in" ])

        animation?finished?``then`` (fun _ -> circle.remove ()) |> ignore

    do
        self?style?position <- "relative"
        self?style?overflow <- "clip"
        self?addEventListener ("click", fun e -> createRipple (e))

// Register the web component
let extends = createObj [ "extends" ==> "button" ]
customElements.define ("fancy-button", jsConstructor<FancyButton>, {| extends = "button" |})

let register () = ()
