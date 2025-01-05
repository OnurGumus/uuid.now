module Client.LightDarkSwitch

open Fable.Core.JsInterop
open Client.WebComponent
open Fable.Core

[<AttachMembers>]
[<AllowNullLiteral>]
type LightDarkSwitch()  =
    inherit HTMLElement()
    let mutable darkMode = false
    let content = html """
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
            color: #ffffff80;
        }

        .checkbox:not(:checked) + .label .sun {
            color: #fff;
        }

        .checkbox:not(:checked) + .label .moon {
            color: #fff;
        }
    </style>
    </style>
    <div class="light-dark">
        <input type="checkbox" class="checkbox" id="light-dark-checkbox" aria-label="Toggle dark mode">
        <label for="light-dark-checkbox" class="label">
            <!-- Moon SVG -->
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" class="moon">
                <path d="M21 12.79A9 9 0 1 1 11.21 3 A7 7 0 0 0 21 12.79z" 
                      stroke="currentColor" 
                      fill="none" 
                      stroke-width="2" 
                      stroke-linecap="round" 
                      stroke-linejoin="round"/>
            </svg>
            
            <!-- Sun SVG -->
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" class="sun">
                <circle cx="12" cy="12" r="5" stroke="currentColor" stroke-width="2" fill="currentColor"/>
                <line x1="12" y1="3" x2="12" y2="1" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
                <line x1="12" y1="23" x2="12" y2="21" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
                <line x1="21" y1="12" x2="23" y2="12" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
                <line x1="1" y1="12" x2="3" y2="12" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
                <line x1="18.36" y1="18.36" x2="19.78" y2="19.78" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
                <line x1="4.22" y1="4.22" x2="5.64" y2="5.64" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
                <line x1="18.36" y1="5.64" x2="19.78" y2="4.22" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
                <line x1="4.22" y1="19.78" x2="5.64" y2="18.36" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
            </svg>
            <div class="ball"></div>
        </label>
    </div>
</div>"""
    
    do
        let shadow = base.attachShadow {| mode = "open" |}
        shadow?setHTMLUnsafe(content)
    

    static member observedAttributes = [| "dark" |]
    
    override this.attributeChangedCallback(name: string, oldValue: obj, newValue: obj) =
        let storedDarkMode = Browser.WebStorage.localStorage.getItem("darkMode")
        if storedDarkMode <> "false" && name = "dark" then
            darkMode <- not (isNull newValue)
            let checkbox = this.shadowRoot?querySelector("#light-dark-checkbox")
          
            checkbox?``checked`` <- darkMode
            Browser.WebStorage.localStorage.setItem("darkMode", string darkMode)
            checkbox?dispatchEvent(Browser.Event.Event.Create("change"))

    override this.connectedCallback() =
        let checkbox = this.shadowRoot?querySelector("#light-dark-checkbox")
        
        // Restore from local storage or attribute
        let storedDarkMode = Browser.WebStorage.localStorage.getItem("darkMode")
        match storedDarkMode with
        | null -> 
            darkMode <- not (isNull (this?getAttribute("dark")))
        | value -> 
            darkMode <- value = "true"
            if darkMode then
                if this?getAttribute("dark") = null then
                    this?setAttribute("dark", "")
            else
                this?removeAttribute("dark")
        
        checkbox?``checked`` <- darkMode
        
        checkbox?addEventListener("change", fun e ->
            let isChecked = e?target?``checked``
            darkMode <- isChecked
            Browser.WebStorage.localStorage.setItem("darkMode", string isChecked)
            let detail = {| ``checked`` = isChecked |}
            let event = Browser.Event.CustomEvent.Create ("theme-changed", !! {| bubbles = true; composed = true; detail = detail; cancelable = true |})
            this?dispatchEvent(event) |> ignore
        ) |> ignore

// Finally, define the new element
customElements.define ("light-dark-switch", jsConstructor<LightDarkSwitch>, None)

let register () = ()
