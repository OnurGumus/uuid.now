module Client.OnOffSwitch

open Fable.Core.JsInterop
open Client.WebComponent
open Fable.Core

[<AttachMembers>]
[<AllowNullLiteral>]
type OnOffSwitch() as this =
    inherit HTMLElement()
    let mutable isOn = false
    let mutable storeKey = "isOn"
    let mutable doPersist = false

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
            background-color: #ffa000;  /* Orange/amber for off mode */
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
            background-color: #1a237e;  /* Dark blue for on mode */
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

        .off, .on {
            color: #f1c40f;
            inline-size: 24px;
            block-size: 24px;
            z-index: 1;
            transition: color 0.2s linear;
        }

        .off {
            margin-inline-start: 8px;
        }

        .on {
            margin-inline-end: 8px;
        }

        .checkbox:checked + .label .on {
            color: #fff;
        }

        .checkbox:checked + .label .off {
            color: #fff;
        }

        .checkbox:not(:checked) + .label .on {
            color: #fff;
        }

        .checkbox:not(:checked) + .label .off {
            color: #fff;
        }

        .checkbox:focus-visible + .label {
            outline: 2px solid #007fd4;
            outline-offset: 2px;
        }
        
        /* Hide decorative icons from screen readers */
        .off svg, .on svg {
            aria-hidden="true";
        }
    </style>
    <div class="on-off">
        <input type="checkbox" 
               class="checkbox" 
               id="on-off-checkbox" 
               aria-label="Switch off"
               role="switch"
               aria-checked="false">
        <label for="on-off-checkbox" class="label">
            <span class="off" aria-hidden="true">
                <slot name="off-icon"></slot>
            </span>
            <span class="on" aria-hidden="true">
                <slot name="on-icon"></slot>
            </span>
            <span class="ball"></span>
        </label>
    </div>"""

    do
        let shadow = base.attachShadow {| mode = "open" |}
        shadow?setHTMLUnsafe (content)


    static member observedAttributes = [| "on"; "name"; "persist" |]

    override this.attributeChangedCallback(attrName, oldValue, newValue) =
        match attrName with
        | "name" ->
            storeKey <- if isNull newValue then "isOn" else string newValue
        | "persist" ->
            doPersist <- not (isNull newValue)
        | "on" ->
            // Always set based on attribute presence, then persist if enabled
            isOn <- not (isNull newValue)
            let checkbox = this.shadowRoot?querySelector("#on-off-checkbox")
            checkbox?``checked`` <- isOn
            if doPersist then
                Browser.WebStorage.localStorage.setItem(storeKey, string isOn)
            checkbox?dispatchEvent(Browser.Event.Event.Create("change"))
        | _ -> ()

    override this.connectedCallback() =
        let checkbox = this.shadowRoot?querySelector ("#on-off-checkbox")

        // Restore from local storage or attribute
        let storedOn =
            if doPersist then
                Browser.WebStorage.localStorage.getItem(storeKey)
            else
                null

        match storedOn with
        | null -> isOn <- not (isNull (this?getAttribute ("on")))
        | value ->
            isOn <- value = "true"

            if isOn then
                if this?getAttribute ("on") = null then
                    this?setAttribute ("on", "")
            else
                this?removeAttribute ("on")
            checkbox?``checked`` <- isOn

        // Update ARIA label based on state
        let updateAriaLabel (isChecked: bool) =
            checkbox?setAttribute("aria-label", if isChecked then "Switch off" else "Switch on")
            checkbox?setAttribute("aria-checked", string isChecked)

        // Initial aria setup
        updateAriaLabel isOn

        checkbox?addEventListener (
            "change",
            fun e ->
                let isChecked = e?target?``checked``
                isOn <- isChecked
                updateAriaLabel isChecked
                if doPersist then
                    Browser.WebStorage.localStorage.setItem(storeKey, string isChecked)
                let detail = {| ``checked`` = isChecked |}

                let event =
                    Browser.Event.CustomEvent.Create(
                        "state-changed",
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

customElements.define ("on-off-switch", jsConstructor<OnOffSwitch>, None)

let register () = ()