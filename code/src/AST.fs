// This file implements a module where we define a data type "expr"
// to store represent arithmetic expressions
module AST

type expr =
    | Num of int
    | Var of string
    | ListElement of (string * expr)
    | TimesExpr of (expr * expr)
    | DivExpr of (expr * expr)
    | PlusExpr of (expr * expr)
    | MinusExpr of (expr * expr)
    | PowExpr of (expr * expr)
    | UMinusExpr of (expr)

type command =
    | Skip
    | Program of (command * command)
    | Assignment of (string * expr)
    | ListAssignment of (string * expr * expr) //A[expr] := expr
