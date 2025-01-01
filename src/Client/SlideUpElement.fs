module Client.SlideUpElement

open Fable.Core.JsInterop
open Client.WebComponent

[<AllowNullLiteral>]
type SlideUpElement() =
    inherit HTMLElement()

    // Create a shadow root in the constructor
    let shadow = base.attachShadow {| mode = "open" |}

    // Create and adopt stylesheet
    let sheet = createCSSStyleSheet ()

    do
        sheet.replaceSync (
            """
        .clip-container {
          overflow: clip;
          display: block;
          inline-size: 100%;
          block-size: 100%;
        }

        ::slotted(*) {
          display: block;
          transition: transform 1.3s cubic-bezier(0.55,-0.26, 0, 0.55),
                     opacity 1.3s cubic-bezier(0.55,-0.26, 0, 0.55);
        }
    """
        )

    do setAdoptedStyleSheets shadow [| sheet |]

    // Create container
    let container = Browser.Dom.document.createElement ("div")
    do container.classList.add ("clip-container")

    // Create slot
    let slot = Browser.Dom.document.createElement ("slot")
    do container.appendChild (slot) |> ignore

    // Append container to shadow root
    do shadow.appendChild (container) |> ignore

    override this.connectedCallback() =
        // In JS we do: this.shadowRoot.querySelector('slot').assignedElements()[0]
        let slotEl = shadow.querySelector ("slot")
        let assigned = unbox<obj array> (slotEl?assignedElements ())

        // If we have something in the slot
        if assigned.Length > 0 then
            // The first assigned element
            let content = assigned.[0]
            // content.style.transform <- "translateY(100%)"
            content?style?transform <- "translateY(100%)"
            content?style?opacity <- "0"

            // queue the animation to run once we have the next frame
            Browser.Dom.window.requestAnimationFrame (fun _ ->
                content?style?transform <- "translateY(0)"
                content?style?opacity <- "1")
            |> ignore

// Finally, define the new element
customElements.define ("slide-up-element", jsConstructor<SlideUpElement>, None)

let register () = ()
