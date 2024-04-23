module SignAnalysis
open Io.SignAnalysis
open System
open AST
open Parser
open Compiler

let rec checkSubset action node1 node2 map =
    if Set.isSubset (Set.ofList (semantics action Map.find node1 map)) (Set.ofList (Map.find node2 map)) then
        false
    else 
        true

let rec chaoticIteration edges map assignment = 
    match edges with
    | _ -> failwith "err" 
    | 

let analysis (input: Input) : Output =
    match parse Grammar.start_command input.commands with 
    | Ok ast -> 
        let compiler_output = Compiler.analysis { 
            commands = input.commands
            determinism = input.determinism}
        let edge, _ = Compiler.edges ast "qS" "qF" 0 input.determinism
        let node = chaoticIteration edge (Map.empty.Add ("qS", [input.assignment])) input.assignment
        { dot = compiler_output.dot
          final_node = "qS"
          initial_node = "qF"
          nodes = node}
    | Error e -> failwith e.Message