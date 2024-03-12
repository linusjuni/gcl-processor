module Compiler

open Io.Compiler
open Io.GCL
open FSharp.Text.Lexing
open System
open AST

exception ParseError of Position * string * Exception

let rec printExpr expr =
    match expr with
    | Num(x) -> string x
    | Var(x) -> x
    | ListElement(x, y) -> sprintf "%s[%s]" x (printExpr y)
    | TimesExpr(x, y) -> sprintf "(%s * %s)" (printExpr x) (printExpr y)
    | DivExpr(x, y) -> sprintf "(%s / %s)" (printExpr x) (printExpr y)
    | PlusExpr(x, y) -> sprintf "(%s + %s)" (printExpr x) (printExpr y)
    | MinusExpr(x, y) -> sprintf "(%s - %s)" (printExpr x) (printExpr y)
    | PowExpr(x, y) -> sprintf "(%s ^ %s)" (printExpr x) (printExpr y)
    | UMinusExpr(x) -> sprintf "-%s" (printExpr x)

let rec prettyPrintBool ast : string =
    match ast with
    | True -> "true"
    | False -> "false"
    | Not(b) -> sprintf "!%s" (prettyPrintBool b)
    | And(b, b1) -> sprintf "(%s & %s)" (prettyPrintBool b) (prettyPrintBool b1)
    | Or(b, b1) -> sprintf "(%s | %s)" (prettyPrintBool b) (prettyPrintBool b1)
    | ShortAnd(b, b1) -> sprintf "(%s && %s)" (prettyPrintBool b) (prettyPrintBool b1)
    | ShortOr(b, b1) -> sprintf "(%s || %s)" (prettyPrintBool b) (prettyPrintBool b1)
    | Equal(b, b1) -> sprintf "(%s = %s)" (printExpr b) (printExpr b1)
    | NotEqual(b, b1) -> sprintf "(%s != %s)" (printExpr b) (printExpr b1)
    | Less(b, b1) -> sprintf "(%s < %s)" (printExpr b) (printExpr b1)
    | LessEqual(b, b1) -> sprintf "(%s <= %s)" (printExpr b) (printExpr b1)
    | Greater(b, b1) -> sprintf "(%s > %s)" (printExpr b) (printExpr b1)
    | GreaterEqual(b, b1) -> sprintf "(%s >= %s)" (printExpr b) (printExpr b1)

let createId x = x |> string |> (+) "q"

let parse parser src =
    let lexbuf = LexBuffer<char>.FromString src

    let parser = parser Lexer.tokenize

    try
        Ok(parser lexbuf)
    with e ->
        let pos = lexbuf.EndPos
        let line = pos.Line
        let column = pos.Column
        let message = e.Message
        let lastToken = new String(lexbuf.Lexeme)
        eprintf "Parse failed at line %d, column %d:\n" line column
        eprintf "Last token: %s" lastToken
        eprintf "\n"
        Error(ParseError(pos, lastToken, e))

let rec Done gc =
    match gc with
    | Implies(b, c) -> Not(b)
    | GuardedOr(gc, gc1) -> And(Done gc, Done gc1)

type Label =
    | CommandSkipLabel
    | CommandAssignmentLabel of string * expr
    | CommandListAssignmentLabel of string * expr * expr
    | BoolLabel of bool

type Edge =
    { source: string
      label: Label
      target: string }

let rec countNodes command =
    match command with
    | Skip -> 1
    | Assignment(var, expr) -> 1
    | ListAssignment(var, expr1, expr2) -> 1
    | Program(c, c') -> countNodes c + countNodes c'
    | If(gc) -> countGuadedNodes gc
    | Do(gc) -> countGuadedNodes gc

and countGuadedNodes gcommand =
    match gcommand with
    | Implies(b, c) -> 1 + countNodes c
    | GuardedOr(gc, gc1) -> countGuadedNodes gc + countGuadedNodes gc1

let rec edges command q1 q2 count determinism =
    match command with
    | Skip ->
        [ { source = q1
            label = CommandSkipLabel
            target = q2 } ],
        count
    | Assignment(var, expr) ->
        [ { source = q1
            label = CommandAssignmentLabel(var, expr)
            target = q2 } ],
        count
    | ListAssignment(var, expr1, expr2) ->
        [ { source = q1
            label = CommandListAssignmentLabel(var, expr1, expr2)
            target = q2 } ],
        count
    | Program(c, c') ->
        let count' = countNodes c + count
        let edge, _ = edges c q1 (createId count') count determinism
        let edge', count'' = edges c' (createId count') q2 count' determinism
        edge @ edge', count''
    | If(gc) ->
        let edge, count', _ = guardedEdges gc q1 q2 count determinism False
        edge, count'
    | Do(gc) ->
        let edge, count', _ = guardedEdges gc q1 q1 count determinism False

        { source = q1
          label = BoolLabel(Done(gc))
          target = q2 }
        :: edge,
        count' + 1

// Returns (edge, count, nextBool)
and guardedEdges gcommand q1 q2 count determinism previousBool =
    match gcommand with
    | Implies(b, c) ->
        let edge, count' = edges c (createId (count + 1)) q2 (count + 1) determinism

        let bool, nextBool =
            match determinism with
            | NonDeterministic -> b, False
            | Deterministic -> And(b, Not(previousBool)), Or(b, previousBool)

        { source = q1
          label = BoolLabel(bool)
          target = createId (count + 1) }
        :: edge,
        count',
        nextBool
    | GuardedOr(gc1, gc2) ->
        let edge, count', nextBool = guardedEdges gc1 q1 q2 count determinism previousBool
        let edge', count'', nextBool' = guardedEdges gc2 q1 q2 count' determinism nextBool
        edge @ edge', count'', nextBool'

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
