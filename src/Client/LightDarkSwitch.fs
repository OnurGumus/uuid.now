module Client.LightDarkSwitch

open Fable.Core.JsInterop
open Client.WebComponent
open Fable.Core
[<AttachMembers>]
[<AllowNullLiteral>]
type LightDarkSwitch() as this  =
    inherit HTMLElement()
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
                <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" 
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
        //this?setHTMLUnsafe(content)


    // Create a shadow root in the constructor
        let shadow = base.attachShadow {| mode = "open" |}
        shadow?setHTMLUnsafe(content)
    // // Create and adopt stylesheet
    // let sheet = createCSSStyleSheet ()

    // do
    //     sheet.replaceSync (
    //         """
    //     .clip-container {
    //       overflow: clip;
    //       display: block;
    //       inline-size: 100%;
    //       block-size: 100%;
    //     }

    //     ::slotted(*) {
    //       display: block;
    //       transition: transform 1.3s cubic-bezier(0.55,-0.26, 0, 0.55),
    //                  opacity 1.3s cubic-bezier(0.55,-0.26, 0, 0.55);
    //     }
    // """
    //     )

    // do setAdoptedStyleSheets shadow [| sheet |]

    // // Create container
    // let container = Browser.Dom.document.createElement ("div")
    // do container.classList.add ("clip-container")

    // // Create slot
    // let slot = Browser.Dom.document.createElement ("slot")
    // do container.appendChild (slot) |> ignore

    // // Append container to shadow root
    // do shadow.appendChild (container) |> ignore

    override this.connectedCallback() =
     ()
        // In JS we do: this.shadowRoot.querySelector('slot').assignedElements()[0]

// Finally, define the new element
customElements.define ("light-dark-switch", jsConstructor<LightDarkSwitch>, None)

let register () = ()
