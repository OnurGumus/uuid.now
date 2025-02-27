module Client.CustomAnimatedText

open Fable.Core.JsInterop
open Browser.Types
open Browser.Dom
open WebComponent
open Fable.Core

[<AttachMembers>]
[<AllowNullLiteral>]
type CustomAnimatedText() =
    inherit HTMLElement()
    let mutable colorSchemeMediaQuery = None
    let mutable savedInnerContainer = Unchecked.defaultof<Browser.Types.HTMLElement>
    let mutable savedSlot = Unchecked.defaultof<Browser.Types.HTMLElement>
    let mutable isProcessing = false

    let slotChanged(self: Browser.Types.HTMLElement, innerContainer :Browser.Types.HTMLElement, slot:Browser.Types.HTMLElement) = 
        if isProcessing then
            ()
        else
            isProcessing <- true
            let assignedNodes = unbox<obj array> (slot?assignedNodes())
            // Clear any existing spans in innerContainer
            innerContainer?replaceChildren()

            // First pass: count total letters
            let mutable totalLetters = 0
            assignedNodes
            |> Array.iter (fun node ->
                let nodeType = unbox<int> node?nodeType
                if nodeType = 1 || nodeType = 3 then // ELEMENT_NODE or TEXT_NODE
                    let textContent = (node?textContent : string).Trim()
                    if not (System.String.IsNullOrEmpty textContent) then
                        totalLetters <- totalLetters + textContent.Length
            )

            // Animation parameters
            let totalDuration = 1.3
            let minDuration = 1.3
            let step = (totalDuration - minDuration) / float totalLetters
            let initialOffset = 10
            let mutable currentIndex = 0


            // Defer to the next animation frame
            window.requestAnimationFrame(fun _ ->
                assignedNodes
                |> Array.filter (fun node -> 
                    let nodeType = unbox<int> node?nodeType
                    let textContent = (node?textContent : string).Trim()
                    (nodeType = 1 || nodeType = 3) && not (System.String.IsNullOrEmpty textContent))
                |> Array.iteri (fun idx node ->
                    let nodeType = unbox<int> node?nodeType
                    let textContent = (node?textContent : string).Trim()
                    if not (System.String.IsNullOrEmpty textContent) then
                        // Grab the computed styles (from node if element, or from parent if text)
                        let styles =
                            if nodeType = 1 then // ELEMENT_NODE
                                window?getComputedStyle(node :?> Element)
                            else
                                window?getComputedStyle(unbox<Element> node?parentElement)

                        // Helper to copy certain style props from the computed style
                        let applyStyles (el: HTMLElement) =
                            [ 
                            "color"
                            "fontFamily"
                            "fontSize"
                            "fontWeight"
                            "fontStyle"
                            "letterSpacing"
                            "lineHeight"
                            "textShadow" ]
                            |> List.iter (fun prop ->
                                el?style?(prop) <- styles?(prop)
                            )

                        // Create a span per character
                        for i in 0 .. textContent.Length - 1 do
                            let ch = textContent.[i]
                            let span = document.createElement("span")
                            span.textContent <- if ch = ' ' then "\u00A0" else string ch
                            span.setAttribute("aria-hidden", "true")
                            applyStyles (span :?> HTMLElement)

                            let delay = float currentIndex * step
                            let duration = totalDuration - delay

                            // Decide if we have rotation
                            let hasRotation = self.hasAttribute("rotation")
                            let className =
                                if hasRotation then
                                    "with-rotation"
                                else
                                    "no-rotation"
                            
                            span.classList.add(className)

                            // Set CSS custom properties
                            span?style?setProperty("--animation-delay", sprintf "%fs" delay)
                            span?style?setProperty("--animation-duration", sprintf "%fs" duration)

                            if hasRotation then
                                span?style?setProperty("--initial-y", sprintf "%fpx" (float (initialOffset) * float (currentIndex + 4)))

                            innerContainer.appendChild(span) |> ignore
                            currentIndex <- currentIndex + 1

                        // Only add space if both current and next nodes have visible content
                        let nextNode = 
                            if idx + 1 < assignedNodes.Length then 
                                let next = assignedNodes.[idx + 1]
                                let nextType = unbox<int> next?nodeType
                                if (nextType = 1 || nextType = 3) && not (System.String.IsNullOrEmpty((next?textContent : string).Trim())) then
                                    Some next
                                else None
                            else None

                        match nextNode with
                        | Some _ when not (textContent.EndsWith(".")) ->  // Don't add space after periods
                            let space = document.createElement("span")
                            space.textContent <- "\u00A0"
                            applyStyles (space :?> HTMLElement)
                            innerContainer.appendChild(space) |> ignore
                        | _ -> ()
                )
                isProcessing <- false
            ) |> ignore
    do
        // Shadow root
        let container = document.createElement("div")
        let innerContainer = document.createElement("div")
        let slot = document.createElement("slot")
        
        // Save references
        savedInnerContainer <- innerContainer
        savedSlot <- slot
    
        let shadow = base.attachShadow {| mode = "open" |}

        // Create outer container

        container.classList.add("middle-text-container")

        // Create inner container for flex layout
        innerContainer.classList.add("middle-text")

        container.appendChild(innerContainer) |> ignore

        // Create a slot
        container.appendChild(slot) |> ignore

        // Listen to the slotchange event
        slot.addEventListener("slotchange", fun _ -> slotChanged(jsThis,innerContainer, slot)) |> ignore
            

        // Create and configure stylesheet
        let sheet = createCSSStyleSheet()
        sheet.replaceSync("""
            .middle-text-container {
                overflow: clip;
                display: flex;
                justify-content: center;
                align-items: center;
                inline-size: 100%;
            }

            .middle-text {
                display: flex;
                justify-content: center;
                align-items: flex-end;
                font-size: inherit;
                font-weight: inherit;
                font-family: inherit;
                font-style: inherit;
                font-variant: inherit;
                font-stretch: inherit;
                line-height: inherit;
                letter-spacing: inherit;
                text-transform: inherit;
                text-shadow: inherit;
                color: inherit;
            }

            .middle-text span {
                display: inline-block;
                color: inherit;
                opacity: 0;
                transform: translateY(var(--initial-y));
            }

            .middle-text span.with-rotation {
                animation: raiseUpRotated var(--animation-duration) cubic-bezier(0.55,-0.26, 0, 0.55) forwards,
                           fadeIn calc(var(--animation-duration)*1) cubic-bezier(0.55,-0.26, 0, 0.55) forwards;
            }

            .middle-text span.no-rotation {
                animation: raiseUpSimple var(--animation-duration) cubic-bezier(0.55,-0.26, 0, 0.55) forwards,
                           fadeIn calc(var(--animation-duration)*1) cubic-bezier(0.55,-0.26, 0, 0.55) forwards;
            }

            @keyframes raiseUpRotated {
                100% {
                    transform: translateY(0);
                }
            }

            @keyframes raiseUpSimple {
                0% { transform: translateY(100%); }
                100% { transform: translateY(0); }
            }

            @keyframes fadeIn {
                0% { opacity: 0; }
                100% { opacity: 1; }
            }

            /* Hide the original slot content */
            slot {
                display: none;
            }
        """)

        setAdoptedStyleSheets shadow [|sheet|]

        shadow.appendChild(container) |> ignore

    member this.triggerSlotChanged() =
        slotChanged(jsThis, savedInnerContainer, savedSlot)

    override this.connectedCallback() =
        printfn "connected"
        let mediaQuery = window?matchMedia("(prefers-color-scheme: dark)")
        colorSchemeMediaQuery <- Some mediaQuery
        let handler = fun _ -> printf "onur"; slotChanged(jsThis, savedInnerContainer, savedSlot)
        mediaQuery?addEventListener("change", handler)

    

    override this.disconnectedCallback() =
        match colorSchemeMediaQuery with
        | Some mediaQuery ->
            mediaQuery?removeEventListener("change", fun _ -> 
                slotChanged(jsThis, savedInnerContainer, savedSlot))
        | None -> ()

// Register the CustomAnimatedText component
attachStaticGetter<CustomAnimatedText, _> "observedAttributes" (fun () -> [| "rotation" |])
customElements.define("custom-animated-text", jsConstructor<CustomAnimatedText>, None)

let register () = ()