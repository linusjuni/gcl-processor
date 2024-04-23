module SignAnalysis
open Io.SignAnalysis
open System
open AST
open Parser
open Compiler

type AbstractResult =
    | Signs of List<Sign>
    | Booleans of List<bool>
    | Undefined

let abstract_addition = 
    [
        ((Negative, Negative), Signs([Negative]))
        ((Negative, Zero), Signs([Negative]))
        ((Negative, Positive), Signs([Negative; Zero; Positive]))

        ((Zero, Negative), Signs([Negative]))
        ((Zero, Zero), Signs([Zero]))
        ((Zero, Positive), Signs([Positive]))

        ((Positive, Negative), Signs([Negative; Zero; Positive]))
        ((Positive, Zero), Signs([Positive]))
        ((Positive, Positive), Signs([Positive]))
    ]
    |> Map.ofList

let abstract_subtraction = 
    [
        ((Negative, Negative), Signs([Negative; Zero; Positive]))
        ((Negative, Zero), Signs([Negative]))
        ((Negative, Positive), Signs([Negative; Zero; Positive]))

        ((Zero, Negative), Signs([Negative]))
        ((Zero, Zero), Signs([Zero]))
        ((Zero, Positive), Signs([Positive]))

        ((Positive, Negative), Signs([Positive]))
        ((Positive, Zero), Signs([Positive]))
        ((Positive, Positive), Signs([Negative; Zero; Positive]))
    ]
    |> Map.ofList

let abstract_multiplication = 
    [
        ((Negative, Negative), Signs([Positive]))
        ((Negative, Zero), Signs([Zero]))
        ((Negative, Positive), Signs([Negative]))

        ((Zero, Negative), Signs([Zero]))
        ((Zero, Zero), Signs([Zero]))
        ((Zero, Positive), Signs([Zero]))

        ((Positive, Negative), Signs([Negative]))
        ((Positive, Zero), Signs([Zero]))
        ((Positive, Positive), Signs([Positive]))
    ]
    |> Map.ofList

let abstract_division =

    [
        ((Negative, Negative), Signs([Positive]))
        ((Negative, Zero), Undefined)
        ((Negative, Positive), Signs([Negative]))

        ((Zero, Negative), Signs([Zero]))
        ((Zero, Zero), Undefined)
        ((Zero, Positive), Signs([Zero]))

        ((Positive, Negative), Signs([Negative]))
        ((Positive, Zero), Undefined)
        ((Positive, Positive), Signs([Positive]))
    ]
    |> Map.ofList

let abstract_power = 
    [
        ((Negative, Negative), Signs([Negative; Positive]))
        ((Negative, Zero), Undefined)
        ((Negative, Positive), Signs([Negative; Positive]))

        ((Zero, Negative), Undefined)
        ((Zero, Zero), Signs([Positive]))
        ((Zero, Positive), Signs([Zero]))

        ((Positive, Negative), Signs([Positive]))
        ((Positive, Zero), Signs([Positive]))
        ((Positive, Positive), Signs([Positive]))
    ]
    |> Map.ofList

let abstract_greaterthan =
    [
        ((Negative, Negative), Booleans([True; False]))
        ((Negative, Zero), Booleans([False]))
        ((Negative, Positive), Booleans([False]))

        ((Zero, Negative), Booleans([True]))
        ((Zero, Zero), Booleans([False]))
        ((Zero, Positive), Booleans([False]))

        ((Positive, Negative), Booleans([True]))
        ((Positive, Zero), Booleans([False]))
        ((Positive, Positive), Booleans([True; False]))
    ]
    |> Map.ofList

let abstract_lessthan =
    [
        ((Negative, Negative), Booleans([True; False]))
        ((Negative, Zero), Booleans([False]))
        ((Negative, Positive), Booleans([False]))

        ((Zero, Negative), Booleans([True]))
        ((Zero, Zero), Booleans([False]))
        ((Zero, Positive), Booleans([False]))

        ((Positive, Negative), Booleans([True]))
        ((Positive, Zero), Booleans([True]))
        ((Positive, Positive), Booleans([True; False]))
    ]
    |> Map.ofList

let abstract_equal =
    [
        ((Negative, Negative), Booleans([True; False]))
        ((Negative, Zero), Booleans([False]))
        ((Negative, Positive), Booleans([False]))

        ((Zero, Negative), Booleans([False]))
        ((Zero, Zero), Booleans([True]))
        ((Zero, Positive), Booleans([False]))

        ((Positive, Negative), Booleans([False]))
        ((Positive, Zero), Booleans([False]))
        ((Positive, Positive), Booleans([True; False]))
    ]
    |> Map.ofList

let abstract_notequal =
    [
        ((Negative, Negative), Booleans([False]))
        ((Negative, Zero), Booleans([True]))
        ((Negative, Positive), Booleans([True]))

        ((Zero, Negative), Booleans([True]))
        ((Zero, Zero), Booleans([False]))
        ((Zero, Positive), Booleans([True]))

        ((Positive, Negative), Booleans([True]))
        ((Positive, Zero), Booleans([True]))
        ((Positive, Positive), Booleans([False]))
    ]
    |> Map.ofList

let abstract_greaterthanequal =
    [
        ((Negative, Negative), Booleans([True; False]))
        ((Negative, Zero), Booleans([False]))
        ((Negative, Positive), Booleans([False]))

        ((Zero, Negative), Booleans([True]))
        ((Zero, Zero), Booleans([True]))
        ((Zero, Positive), Booleans([False]))

        ((Positive, Negative), Booleans([False]))
        ((Positive, Zero), Booleans([True]))
        ((Positive, Positive), Booleans([True; False]))
    ]
    |> Map.ofList

let abstract_lessthanequal =
    [
        ((Negative, Negative), Booleans([True; False]))
        ((Negative, Zero), Booleans([False]))
        ((Negative, Positive), Booleans([False]))

        ((Zero, Negative), Booleans([True]))
        ((Zero, Zero), Booleans([True]))
        ((Zero, Positive), Booleans([False]))

        ((Positive, Negative), Booleans([True]))
        ((Positive, Zero), Booleans([True]))
        ((Positive, Positive), Booleans([True; False]))
    ]
    |> Map.ofList

let abstract_and =
    [
        ((True, True), Booleans([True]))
        ((True, False), Booleans([False]))

        ((False, True), Booleans([False]))
        ((False, False), Booleans([False]))

    ]
    |> Map.ofList

let abstract_shortand = abstract_and

let abstract_or =
    [
        ((True, True), Booleans([True]))
        ((True, False), Booleans([True]))

        ((False, True), Booleans([True]))
        ((False, False), Booleans([False]))

    ]
    |> Map.ofList

let abstract_shortor = abstract_or

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
