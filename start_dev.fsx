#load "setup.fsx"
open Common

printfn "Watching the development server at http://localhost:5173"
runCommand "npm" "run start"