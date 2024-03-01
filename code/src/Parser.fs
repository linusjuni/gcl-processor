module Parser
open Io.Parser

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

let rec prettyPrintBool ast : string =
    match ast with
    | True -> "true"
    | False -> "false"
    | Not(b) -> sprintf "!%s" (prettyPrintBool b)
    | And(b, b1) -> sprintf "(%s & %s)" (prettyPrintBool b) (prettyPrintBool b1)
    | _ -> "not ready"

let rec prettyPrint (ast : command) : string =
    match ast with 
    | Skip -> "skip"
    | Program(c, c') -> sprintf "%s; %s" (prettyPrint c) (prettyPrint c')
    | If(gc) -> sprintf "if %s \nfi" (prettyPrintGCommand gc)
    | Do(gc) -> sprintf "do %s \nod" (prettyPrintGCommand gc)
    
and prettyPrintGCommand (ast : gcommand) : string = 
    match ast with
    | Implies(b, c) -> sprintf "%s -> \n   %s" (prettyPrintBool b) (prettyPrint c)

let analysis (input: Input) : Output =
    // TODO: change start_expression to start_commands
    match parse Grammar.start_command input.commands with
        | Ok ast ->
            Console.Error.WriteLine("> {0}", ast)
            { pretty = prettyPrint ast }
        | Error e -> { pretty = String.Format("Parse error: {0}", e) }
