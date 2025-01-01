module Client.CustomAnimatedText

open Fable.Core.JsInterop
open Browser.Types
open Browser.Dom
open WebComponent

[<AllowNullLiteral>]
type CustomAnimatedText() =
    inherit HTMLElement()

    do
        // Shadow root
        let shadow = base.attachShadow {| mode = "open" |}

        // Create outer container
        let container = document.createElement("div")
        container.classList.add("middle-text-container")

        // Create inner container for flex layout
        let innerContainer = document.createElement("div")
        innerContainer.classList.add("middle-text")

        container.appendChild(innerContainer) |> ignore

        // Create a slot
        let slot = document.createElement("slot")
        container.appendChild(slot) |> ignore

        // Listen to the slotchange event
        slot.addEventListener("slotchange", fun _ ->
            let assignedNodes = unbox<obj array> (slot?assignedNodes())
            // Clear any existing spans in innerContainer
            innerContainer.innerHTML <- ""

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
                |> Array.iter (fun node ->
                    let nodeType = unbox<int> node?nodeType
                    if nodeType = 1 || nodeType = 3 then // ELEMENT_NODE or TEXT_NODE
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
                                [ "color"
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
                                applyStyles (span :?> HTMLElement)

                                let delay = float currentIndex * step
                                let duration = totalDuration - delay

                                // Decide if we have rotation
                                let hasRotation = jsThis?hasAttribute("rotation")
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

                            // If it's not the last node, add an extra space
                            let lastNode = assignedNodes.[assignedNodes.Length - 1]
                            if not (System.Object.ReferenceEquals(node, lastNode)) then
                                let space = document.createElement("span")
                                space.textContent <- "\u00A0"
                                applyStyles (space :?> HTMLElement)
                                innerContainer.appendChild(space) |> ignore
            )
            ) |> ignore)

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

// Register the CustomAnimatedText component
attachStaticGetter<CustomAnimatedText, _> "observedAttributes" (fun () -> [| "rotation" |])
customElements.define("custom-animated-text", jsConstructor<CustomAnimatedText>)

let register () = ()