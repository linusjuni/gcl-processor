// This file implements a module where we define a data type "expr"
// to store represent arithmetic expressions
module AST
open Io.GCL
type expr =
    | Num of int
    | Var of Variable
    | ListElement of (string * expr)
    | TimesExpr of (expr * expr)
    | DivExpr of (expr * expr)
    | PlusExpr of (expr * expr)
    | MinusExpr of (expr * expr)
    | PowExpr of (expr * expr)
    | UMinusExpr of (expr)

type bool = 
    | True
    | False
    | And of (bool * bool)
    | Or of (bool * bool)
    | Not of (bool)
    | ShortAnd of (bool * bool)
    | ShortOr of (bool * bool)
    | Equal of (expr * expr)
    | NotEqual of (expr * expr)
    | Less of (expr * expr)
    | LessEqual of (expr * expr)
    | Greater of (expr * expr)
    | GreaterEqual of (expr * expr)

type command =
    | Skip
    | Program of (command * command)
    | If of (gcommand)
    | Do of (gcommand)
    | Assignment of (string * expr)
    | ListAssignment of (string * expr * expr) //A[expr] := expr

and gcommand = 
    | Implies of (bool * command)
    | GuardedOr of (gcommand * gcommand)
