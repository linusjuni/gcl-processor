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

let rec edges ast =
    failwith "lol"

let rec printDot edges =
    failwith "fack"

let analysis (input: Input) : Output =
    // TODO: change start_expression to start_commands
    match parse Grammar.start_command input.commands with
        | Ok ast ->
            Console.Error.WriteLine("> {0}", ast)
            { dot = printDot <| edges <| ast}
        | Error e -> { dot = String.Format("Parse error: {0}", e) }