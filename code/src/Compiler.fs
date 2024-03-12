module Compiler

open Io.Compiler
open Io.GCL
open FSharp.Text.Lexing
open System
open AST
open Parser

type Label =
    | CommandSkipLabel
    | CommandAssignmentLabel of string * expr
    | CommandListAssignmentLabel of string * expr * expr
    | BoolLabel of bool

type Edge =
    { source: string
      label: Label
      target: string }

let createNodeIdFromNodeCount x = x |> string |> (+) "q"

let rec Done gc =
    match gc with
    | Implies(b, c) -> Not(b)
    | GuardedOr(gc, gc1) -> And(Done gc, Done gc1)

let rec NodesInCommand command =
    match command with
    | Skip -> 1
    | Assignment(var, expr) -> 1
    | ListAssignment(var, expr1, expr2) -> 1
    | Program(c, c') -> NodesInCommand c + NodesInCommand c'
    | If(gc) -> NodesInGuardedCommand gc
    | Do(gc) -> NodesInGuardedCommand gc

and NodesInGuardedCommand gcommand =
    match gcommand with
    | Implies(b, c) -> 1 + NodesInCommand c
    | GuardedOr(gc, gc1) -> NodesInGuardedCommand gc + NodesInGuardedCommand gc1

let rec edges command q1 q2 nodesCount determinism =
    match command with
    | Skip ->
        [ { source = q1
            label = CommandSkipLabel
            target = q2 } ],
        nodesCount
    | Assignment(var, expr) ->
        [ { source = q1
            label = CommandAssignmentLabel(var, expr)
            target = q2 } ],
        nodesCount
    | ListAssignment(var, expr1, expr2) ->
        [ { source = q1
            label = CommandListAssignmentLabel(var, expr1, expr2)
            target = q2 } ],
        nodesCount
    | Program(c, c') ->
        // The the id of the end node of the left program must be computed before edges are computed
        let nodesInLeftProgram = NodesInCommand c + nodesCount

        let edge, _ =
            edges c q1 (createNodeIdFromNodeCount nodesInLeftProgram) nodesCount determinism

        let edge', totalNodesCount =
            edges c' (createNodeIdFromNodeCount nodesInLeftProgram) q2 nodesInLeftProgram determinism

        edge @ edge', totalNodesCount
    | If(gc) -> guardedEdges gc q1 q2 nodesCount determinism
    | Do(gc) ->
        let edge, nodesCount = guardedEdges gc q1 q1 nodesCount determinism

        let doneEdge =
            { source = q1
              label = BoolLabel(Done(gc))
              target = q2 }

        doneEdge :: edge, nodesCount + 1

and guardedEdges gcommand q1 q2 nodesCount determinism =
    let edge, nodesCount, _ =
        guardedEdgesHelper gcommand q1 q2 nodesCount determinism False

    edge, nodesCount

and guardedEdgesHelper gcommand q1 q2 nodesCount determinism previousBool : Edge list * int * bool =
    match gcommand with
    | Implies(b, c) ->
        // An intermediate node must be created
        // A boolean edge goes from source node into intermediate node
        // and the program uses the intermediate node as the source node

        let intermediateNodeNumber = nodesCount + 1
        let intermediateNodeId = createNodeIdFromNodeCount intermediateNodeNumber

        let bool, nextBool =
            match determinism with
            | NonDeterministic -> b, False
            | Deterministic -> And(b, Not(previousBool)), Or(b, previousBool)

        let intermediateNode =
            { source = q1
              label = BoolLabel(bool)
              target = intermediateNodeId }

        let edge, totalNodes =
            edges c intermediateNodeId q2 intermediateNodeNumber determinism

        intermediateNode :: edge, totalNodes, nextBool
    | GuardedOr(gc1, gc2) ->
        let edge, leftGCNodesCount, nextBool =
            guardedEdgesHelper gc1 q1 q2 nodesCount determinism previousBool

        let edge', toalNodesCount, nextBool' =
            guardedEdgesHelper gc2 q1 q2 leftGCNodesCount determinism nextBool

        edge @ edge', toalNodesCount, nextBool'

let rec printLabel label =
    match label with
    | CommandSkipLabel -> "skip"
    | CommandAssignmentLabel(var, expr) -> var + ":=" + printExpr (expr)
    | CommandListAssignmentLabel(var, expr1, expr2) -> var + "[" + printExpr (expr1) + "] := " + printExpr (expr2)
    | BoolLabel(b) -> prettyPrintBool b

let rec printDotEdges edges =
    match edges with
    | e1 :: t ->
        e1.source
        + " -> "
        + e1.target
        + "[label = \""
        + (printLabel e1.label)
        + "\"];\n"
        + printDotEdges t
    | [] -> ""

let rec printDot edges =
    match edges with
    | _ -> "digraph program_graph {rankdir=LR;"
    + printDotEdges edges
    + "}"

let analysis (input: Input) : Output =
    // TODO: change start_expression to start_commands
    match parse Grammar.start_command input.commands with
    | Ok ast ->
        let edge, _ = edges ast "qI" "qF" 0 input.determinism
        { dot = printDot (edge) }
    | Error e -> { dot = "" }
