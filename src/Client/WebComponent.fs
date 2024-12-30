module Client.WebComponent


open Fable.Core

[<Global>]
module customElements =
    let define (name: string, constructorFn: obj) = jsNative

[<Global>]
type ShadowRoot() =
    member _.appendChild(node: Browser.Types.Node) = jsNative
    member _.querySelector(selector: string) : Browser.Types.HTMLElement = jsNative

[<Global;AbstractClass>]
[<AllowNullLiteral>]
type HTMLElement() =
    member _.attachShadow(init: obj): ShadowRoot = jsNative
    abstract connectedCallback: unit -> unit
