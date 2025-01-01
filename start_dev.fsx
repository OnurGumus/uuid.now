#load "setup.fsx"
open Common
open Setup

// Start the development server
printfn "Starting the development server..."
runCommand "npm" "run start"
printfn "Server is running at http://localhost:5173"