module Parser
open Io.Parser

open FSharp.Text.Lexing
open System
open AST

exception ParseError of Position * string * Exception

let INDENTATION = "   "

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

let rec prettyPrint (ast : command) indent : string =
    match ast with 
        | Skip -> "skip"
        | If(gc) -> sprintf "if %s \n%sfi" (prettyPrintGCommand gc indent) inden
        | Do(gc) -> sprintf "do %s \n%sod" (prettyPrintGCommand gc indent) indent
        | Program(c, c') -> sprintf "%s ;\n%s" (prettyPrint c indent) (prettyPrint c' indent)
        | Assignment(variable, expr) -> sprintf "%s := %s" variable (printExpr expr)
        | ListAssignment(variable, index, value) -> sprintf "%s[%s] := %s" variable (printExpr index) (printExpr value)

and prettyPrintGCommand (ast : gcommand) indent : string = 
    match ast with
    | Implies(b, c) -> sprintf "%s -> \n%s" (prettyPrintBool b) (prettyPrint c (indent + INDENTATION))
    | GuardedOr(c, c1) -> sprintf "%s \n[] %s" (prettyPrintGCommand c indent) (prettyPrintGCommand c1 indent)

let analysis (input: Input) : Output =
    // TODO: change start_expression to start_commands
    match parse Grammar.start_command input.commands with
        | Ok ast ->
            Console.Error.WriteLine("> {0}", ast)
            { pretty = prettyPrint ast ""}
        | Error e -> { pretty = String.Format("Parse error: {0}", e) }
