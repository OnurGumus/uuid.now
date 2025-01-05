module Client.App

open Browser
open Fable.Core.JsInterop

SlideUpElement.register ()
CustomAnimatedText.register ()
FancyButton.register ()
LightDarkSwitch.register ()

FlipBoard.switchToGuid "00000000-0000-0000-0000-000000000000" |> ignore

document.documentElement.classList.toggle ("dark-mode") |> ignore

let toggleDarkMode () =
    let classList = document.documentElement.classList
    classList.toggle ("dark-mode") |> ignore
    classList.toggle ("light-mode") |> ignore
    let el = document.querySelector("custom-animated-text")
    el?triggerSlotChanged() |> ignore

let lightDarkButton = document.querySelector(".light-dark")

lightDarkButton.addEventListener (
    "change",
    fun _ ->
        if document?startViewTransition then
            document?startViewTransition (fun _ -> toggleDarkMode ())
        else
            toggleDarkMode ()
)


