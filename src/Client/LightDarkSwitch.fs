module Client.LightDarkSwitch

open Fable.Core.JsInterop
open Client.WebComponent
open Fable.Core

[<AttachMembers>]
[<AllowNullLiteral>]
type LightDarkSwitch() =
    inherit HTMLElement()
    let mutable darkMode = false

    let content =
        html
            """
    <style>
        *{
            box-sizing: border-box;
        }
        .checkbox {
            opacity: 0;
            position: absolute;
        }

        .label {
            background-color: #ffa000;  /* Orange/amber for dark mode */
            border-radius: 50px;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 4px;
            position: relative;
            block-size: 48px;
            inline-size: 88px;
            transition: background-color 0.2s linear;
        }

        .checkbox:checked + .label {
            background-color: #1a237e;  /* Dark blue for light mode */
        }

        .ball {
            background-color: #fff;
            border-radius: 50%;
            position: absolute;
            inset-block-start: 4px;
            inset-inline-start: 4px;
            block-size: 40px;
            inline-size: 40px;
            transform: translateX(0px);
            transition: transform 0.2s linear;
            z-index: 2;
        }

        .checkbox:checked + .label .ball {
            transform: translateX(40px);
        }

        .moon, .sun {
            color: #f1c40f;
            inline-size: 24px;
            block-size: 24px;
            z-index: 1;
            transition: color 0.2s linear;
        }

        .moon {
            margin-inline-start: 8px;
        }

        .sun {
            margin-inline-end: 8px;
        }

        .checkbox:checked + .label .sun {
            color: #fff;
        }

        .checkbox:checked + .label .moon {
            color: #fff;
        }

        .checkbox:not(:checked) + .label .sun {
            color: #fff;
        }

        .checkbox:not(:checked) + .label .moon {
            color: #fff;
        }

        .checkbox:focus-visible + .label {
            outline: 2px solid #007fd4;
            outline-offset: 2px;
        }
        
        /* Hide decorative icons from screen readers */
        .moon svg, .sun svg {
            aria-hidden="true";
        }
    </style>
    <div class="light-dark">
        <input type="checkbox" 
               class="checkbox" 
               id="light-dark-checkbox" 
               aria-label="Switch to dark theme"
               role="switch"
               aria-checked="false">
        <label for="light-dark-checkbox" class="label">
            <span class="moon" aria-hidden="true">
                <slot name="moon-icon"></slot>
            </span>
            <span class="sun" aria-hidden="true">
                <slot name="sun-icon"></slot>
            </span>
            <span class="ball"></span>
        </label>
    </div>"""

    do
        let shadow = base.attachShadow {| mode = "open" |}
        shadow?setHTMLUnsafe (content)


    static member observedAttributes = [| "dark" |]

    override this.attributeChangedCallback(name: string, oldValue: obj, newValue: obj) =
        let storedDarkMode = Browser.WebStorage.localStorage.getItem ("darkMode")

        if storedDarkMode <> "false" && name = "dark" then
            darkMode <- not (isNull newValue)
            let checkbox = this.shadowRoot?querySelector ("#light-dark-checkbox")

            checkbox?``checked`` <- darkMode
            Browser.WebStorage.localStorage.setItem ("darkMode", string darkMode)
            checkbox?dispatchEvent (Browser.Event.Event.Create("change"))

    override this.connectedCallback() =
        let checkbox = this.shadowRoot?querySelector ("#light-dark-checkbox")

        // Restore from local storage or attribute
        let storedDarkMode = Browser.WebStorage.localStorage.getItem ("darkMode")

        match storedDarkMode with
        | null -> darkMode <- not (isNull (this?getAttribute ("dark")))
        | value ->
            darkMode <- value = "true"

            if darkMode then
                if this?getAttribute ("dark") = null then
                    this?setAttribute ("dark", "")
            else
                this?removeAttribute ("dark")
            checkbox?``checked`` <- darkMode

        // Update ARIA label based on state
        let updateAriaLabel (isChecked: bool) =
            checkbox?setAttribute("aria-label", if isChecked then "Switch to light theme" else "Switch to dark theme")
            checkbox?setAttribute("aria-checked", string isChecked)

        // Initial aria setup
        updateAriaLabel darkMode

        checkbox?addEventListener (
            "change",
            fun e ->
                let isChecked = e?target?``checked``
                darkMode <- isChecked
                updateAriaLabel isChecked
                Browser.WebStorage.localStorage.setItem ("darkMode", string isChecked)
                let detail = {| ``checked`` = isChecked |}

                let event =
                    Browser.Event.CustomEvent.Create(
                        "theme-changed",
                        !!{| bubbles = true
                             composed = true
                             detail = detail
                             cancelable = true |}
                    )
                this?dispatchEvent (event) |> ignore
        )
        |> ignore

        // Add keyboard support
        checkbox?addEventListener (
            "keydown",
            fun e ->
                if e?key = "Enter" then
                    e?preventDefault()
                    checkbox?click() |> ignore
        )
        |> ignore

customElements.define ("light-dark-switch", jsConstructor<LightDarkSwitch>, None)

let register () = ()
