module Compiler
open Io.Compiler
open FSharp.Text.Lexing
open System
open AST

exception ParseError of Position * string * Exception

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

let rec edges c q1 q2 =
    match c with
    | Skip -> [ { source = q1; label = CommandLabel(Skip); target = q2 } ]
    | _ -> []

let printL l = 
    match l with
    | CommandLabel Skip -> "skip"
    | _ -> "TODO"

let printDotEdges e =
    match e with
    | [ e1 ] -> e1.source + " -> " + e1.target + "[label = \"" + (printL e1.label) + "\"];"
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