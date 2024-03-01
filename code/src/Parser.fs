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
    | UMinusExpr(x) -> sprintf "(- %s)" (printExpr x)

let rec prettyPrint ast : string =
    match ast with 
    | Skip -> "skip"
    | Program(c, c') -> sprintf "%s ;\n%s" (prettyPrint c) (prettyPrint c')
    | Assignment(variable, expr) -> sprintf "%s := %s" variable (printExpr expr)
    | ListAssignment(variable, index, value) -> sprintf "%s[%s] := %s" variable (printExpr index) (printExpr value)

let analysis (input: Input) : Output =
    // TODO: change start_expression to start_commands
    match parse Grammar.start_command input.commands with
        | Ok ast ->
            Console.Error.WriteLine("> {0}", ast)
            { pretty = prettyPrint ast }
        | Error e -> { pretty = String.Format("Parse error: {0}", e) }
