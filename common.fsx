open System.Diagnostics
open System.IO

let runCommand (command: string) (args: string) =
    let startInfo = ProcessStartInfo(command, args)
    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardError <- true

    use proc = new Process()
    proc.StartInfo <- startInfo
    
    proc.OutputDataReceived.Add(fun args -> 
        if not (isNull args.Data) then 
            printfn "%s" args.Data)
            
    proc.ErrorDataReceived.Add(fun args -> 
        if not (isNull args.Data) then 
            eprintfn "%s" args.Data)

    proc.Start() |> ignore
    proc.BeginOutputReadLine()
    proc.BeginErrorReadLine()
    proc.WaitForExit()

    if proc.ExitCode <> 0 then
        failwithf "Command '%s %s' failed with exit code %d" command args proc.ExitCode

// Shared tasks
let restoreDotnetTools () =
    printfn "Restoring .NET tools..."
    runCommand "dotnet" "tool restore"

let restorePaketPackages () =
    printfn "Restoring Paket packages..."
    runCommand "dotnet" "paket restore"

let buildDotnetProject () =
    printfn "Building the .NET project..."
    runCommand "dotnet" "build"

let navigateToClientFolder () =
    printfn "Navigating to client folder..."
    Directory.SetCurrentDirectory("src/Client")

let installNpmDependencies () =
    printfn "Installing NPM dependencies..."
    runCommand "npm" "install"
