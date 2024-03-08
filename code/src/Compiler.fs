module Compiler
open Io.Compiler
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

let parse parser src =
    let lexbuf = LexBuffer<char>.FromString src

    let parser = parser Lexer.tokenize

    try
        Ok(parser lexbuf)
    with
    | e ->
        let pos = lexbuf.EndPos
        let line = pos.Line
        let column = pos.Column
        let message = e.Message
        let lastToken = new String(lexbuf.Lexeme)
        eprintf "Parse failed at line %d, column %d:\n" line column
        eprintf "Last token: %s" lastToken
        eprintf "\n"
        Error(ParseError(pos, lastToken, e))

type Label = 
    CommandLabel of command

type Edge = {
    source: string
    label: Label
    target: string
}

let rec edges command q1 q2 =
    match command with
    | Skip -> [ { source = q1; label = CommandLabel(Skip); target = q2 } ]
    | Assignment (var, expr) -> [ { source = q1; label = CommandLabel(Assignment (var, expr)); target = q2}]
    | _ -> []

let printLabel label = 
    match label with
    | CommandLabel Skip -> "skip"
    | CommandLabel (Assignment (var, expr)) -> var + ":=" + printExpr(expr)
    | _ -> "TODO"

let printDotEdges edge =
    match edge with
    | [ e1 ] -> e1.source + " -> " + e1.target + "[label = \"" + (printLabel e1.label) + "\"];"
    | _ -> ""

let rec printDot e =
    match e with
    | _ -> "digraph program_graph {rankdir=LR;"
    + printDotEdges e
    + "}"

let analysis (input: Input) : Output =
    // TODO: change start_expression to start_commands
    match parse Grammar.start_command input.commands with
        | Ok ast -> { dot = printDot (edges ast "qS" "qF") }
        | Error e -> { dot = "" }
