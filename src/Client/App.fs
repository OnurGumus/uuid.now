module Client.App

open Browser
open Fable.Core.JsInterop

SlideUpElement.register ()
CustomAnimatedText.register ()
FancyButton.register ()
LightDarkSwitch.register ()

FlipBoard.getRandomV4Guid () |> FlipBoard.switchToGuid |> ignore

document.documentElement.classList.toggle ("dark-mode") |> ignore

let toggleDarkMode () =
    let classList = document.documentElement.classList
    classList.toggle ("dark-mode") |> ignore
    classList.toggle ("light-mode") |> ignore
    let el = document.querySelector("custom-animated-text")
    el?triggerSlotChanged() |> ignore

let lightDarkButton = document.querySelector(".light-dark")
if lightDarkButton.hasAttribute("dark") |> not then
    toggleDarkMode ()

lightDarkButton.addEventListener (
    "theme-changed",
    fun _ ->
        if document?startViewTransition then
            document?startViewTransition (fun _ -> toggleDarkMode ())
        else
            toggleDarkMode ()
)


