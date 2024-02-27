module Calculator
open Io.Calculator

open AST
open System

let rec evaluate (expr: expr) : Result<int, string> =
    match expr with 
    | Num(x) -> Ok x
    | PlusExpr(x ,y) -> binaryOperator x y (+)
    | MinusExpr(x, y) -> binaryOperator x y (-)
    | TimesExpr(x, y) -> binaryOperator x y (*)
    | DivExpr(x, y) -> binaryOperator x y (/)
    | PowExpr(x, y) -> binaryOperator x y pown
    | UMinusExpr(x) -> match evaluate x with
                       | Ok x -> Ok (-x)
                       | Error e -> Error e
and binaryOperator operand1 operand2 operator = 
    match evaluate operand1 with 
    | Ok x -> match evaluate operand2 with
              | Ok y -> Ok (operator x y)
              | Error e -> Error e
    | Error e -> Error e

let analysis (input: Input) : Output =
    match Parser.parse Grammar.start_expression input.expression with
    | Ok ast -> 
        Console.Error.WriteLine("> {0}", ast)
        match evaluate ast with
        | Ok result -> { result = result.ToString(); error = "" }
        | Error e -> { result = ""; error = String.Format("Evaluation error: {0}", e) }
    | Error e -> { result = ""; error = String.Format("Parse error: {0}", e) }
