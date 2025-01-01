module Client.WebComponent


open Fable.Core
open Fable.Core.JsInterop

[<Global>]
module customElements =
    let define (name: string, constructorFn: obj, extends: obj) = jsNative

[<Global>]
type ShadowRoot() =
    member _.appendChild(node: Browser.Types.Node) = jsNative
    member _.querySelector(selector: string) : Browser.Types.HTMLElement = jsNative

[<Global; AbstractClass>]
[<AllowNullLiteral>]
type HTMLElement() =
    member _.attachShadow(init: obj) : ShadowRoot = jsNative
    // Web Components lifecycle methods:
    abstract connectedCallback: unit -> unit
    abstract disconnectedCallback: unit -> unit
    abstract attributeChangedCallback: string * obj * obj -> unit
    // We supply placeholders in case we need them:
    default _.connectedCallback() = ()
    default _.disconnectedCallback() = ()
    default _.attributeChangedCallback(_, _, _) = ()

[<Global; AbstractClass>]
type HTMLButtonElement() =
    inherit HTMLElement()
    member _.clientWidth: int = jsNative
    member _.clientHeight: int = jsNative
    member _.offsetLeft: int = jsNative
    member _.offsetTop: int = jsNative
    member _.style = jsNative
    member _.animate(keyframes: obj[], options: obj) = jsNative
    member _.appendChild(node: Browser.Types.Node) : Browser.Types.Node = jsNative

let inline attachStatic<'T> (name: string) (f: obj) : unit = jsConstructor<'T>?name <- f

let inline attachStaticGetter<'T, 'V> (name: string) (f: unit -> 'V) : unit =
    JS.Constructors.Object.defineProperty (jsConstructor<'T>, name, !!{| get = f |})
    |> ignore

[<AllowNullLiteral>]
type CSSStyleSheet =
    abstract replaceSync: string -> unit
    abstract cssRules: obj array

[<Emit("new CSSStyleSheet()")>]
let createCSSStyleSheet () : CSSStyleSheet = jsNative

[<Emit("$0.adoptedStyleSheets = $1")>]
let setAdoptedStyleSheets (element: obj) (sheets: CSSStyleSheet[]) : unit = jsNative
