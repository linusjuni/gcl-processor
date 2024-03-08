module Compiler
open Io.Compiler
open FSharp.Text.Lexing
open System
open AST

exception ParseError of Position * string * Exception

let mutable ID = 0

let getId = 
    let id = "q" + string ID
    ID <- ID + 1
    id

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
let rec Done gc = 
    match gc with
    | Implies(b,c) -> Not (b)
    | GuardedOr(gc,gc1) -> And (Done gc, Done gc1)

type Label = 
    | CommandLabel of command
    | BoolLabel of bool

type Edge = {
    source: string
    label: Label
    target: string
}

let rec edges command q1 q2 =
    match command with
    | Skip -> [ { source = q1; label = CommandLabel(Skip); target = q2 } ]
    | Assignment (var, expr) -> [ { source = q1; label = CommandLabel(Assignment (var, expr)); target = q2}]
    | Program (c, c') -> let id = "q" + string ID
                         ID <- ID + 1
                         edges c q1 id  @ edges c' id q2
    | If(gc) -> guardedEdges gc q1 q2
    | Do(gc) -> guardedEdges gc q1 q1 @ [ {source = q1; label = BoolLabel(Done(gc)) ;target = q2} ]
    | _ -> []
and guardedEdges gcommand q1 q2 = 
    match gcommand with
    | Implies(b,c) -> {source = q1; label = BoolLabel(b); target = "q"} :: edges c "q" q2
    // | GuardedOr(c, c1) -> edges c q1 q2 @ edges c1 q1 q2
    | _ -> []

let rec printLabel label = 
    match label with
    | CommandLabel Skip -> "skip"
    | CommandLabel (Assignment (var, expr)) -> var + ":=" + printExpr(expr)
    | BoolLabel (b) -> prettyPrintBool b
    | _ -> "TODO"

let rec printDotEdges edges =
    match edges with
    | e1::t -> e1.source + " -> " + e1.target + "[label = \"" + (printLabel e1.label) + "\"];\n" + printDotEdges t
    | [] -> ""

let rec printDot edges =
    match edges with
    | _ -> "digraph program_graph {rankdir=LR;"
    + printDotEdges edges
    + "}"

let analysis (input: Input) : Output =
    // TODO: change start_expression to start_commands
    match parse Grammar.start_command input.commands with
        | Ok ast -> { dot = printDot (edges ast "qS" "qF") }
        | Error e -> { dot = "" }
