module Interpreter
open Io.Interpreter
open Io.GCL
open AST

let getValueOfVariable variable (memory: InterpreterMemory) : int64 =
    Map.find variable memory.variables

let getValueOfArray array (memory: InterpreterMemory) : List<int64> =
    Map.find array memory.arrays

let getArrayElement mem array index =
    (getValueOfArray array mem).Item(index)

let updateVariable variable value (memory: InterpreterMemory) : InterpreterMemory =
    { variables = Map.add variable value memory.variables
      arrays = memory.arrays }

let updateArray array value (memory: InterpreterMemory) : InterpreterMemory =
    { variables = memory.variables
      arrays = Map.add array value memory.arrays }

let rec arithmeticSemantics mem expr =
  match expr with
  | Num(n) -> int64(n)
  | Var(v: Variable) -> getValueOfVariable v mem
  | ListElement(a : Array,e) -> getArrayElement mem a (int(arithmeticSemantics mem e))
  | UMinusExpr(e) -> - arithmeticSemantics mem expr
  | PowExpr(e1,e2) -> int64(float(arithmeticSemantics mem e1) ** float(arithmeticSemantics mem e2))
  | TimesExpr(e1,e2) -> arithmeticSemantics mem e1 * arithmeticSemantics mem e2
  | DivExpr(e1,e2) -> arithmeticSemantics mem e1 / arithmeticSemantics mem e2
  | PlusExpr(e1,e2) -> arithmeticSemantics mem e1 + arithmeticSemantics mem e2
  | MinusExpr(e1,e2) -> arithmeticSemantics mem e1 - arithmeticSemantics mem e2

let rec boolSemantics mem boolExpr =
  match boolExpr with
  | True -> true
  | False -> false
  | Not(e1) -> not (boolSemantics mem e1)
  | And(b1,b2) -> 
                let a1 = boolSemantics mem b1
                let a2 = boolSemantics mem b2 
                a1 && a2
  | ShortAnd(b1,b2) -> boolSemantics mem b1 && boolSemantics mem b2
  | Or(b1,b2) -> 
                let or1 = boolSemantics mem b1
                let or2 = boolSemantics mem b2 
                or1 || or2
  | ShortOr(b1,b2) -> boolSemantics mem b1 || boolSemantics mem b2
  | Equal(e1,e2) -> arithmeticSemantics mem e1 = arithmeticSemantics mem e2
  | NotEqual(e1,e2) -> arithmeticSemantics mem e1 <> arithmeticSemantics mem e2
  | Less(e1,e2) -> arithmeticSemantics mem e1 < arithmeticSemantics mem e2
  | LessEqual(e1,e2) -> arithmeticSemantics mem e1 <= arithmeticSemantics mem e2
  | Greater(e1,e2) -> arithmeticSemantics mem e1 > arithmeticSemantics mem e2
  | GreaterEqual(e1,e2) -> arithmeticSemantics mem e1 >= arithmeticSemantics mem e2

let analysis (input: Input) : Output =
    failwith "Interpreter not yet implemented" // TODO: start here

    { initial_node = ""
      final_node = ""
      dot = ""
      trace = []
      termination = Terminated }