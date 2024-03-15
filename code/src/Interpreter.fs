module Interpreter
open Io.Interpreter
open Io.GCL
open AST
open Parser
open Compiler

exception StuckError 
exception NoEdgesError
exception RaisedByNegative

let getValueOfVariable variable (memory: InterpreterMemory) : int64 =
    Map.find variable memory.variables

let getArray array (memory: InterpreterMemory) : List<int64> =
    Map.find array memory.arrays

let getArrayElement mem array index =
    (getArray array mem).Item(index)

let updateVariable variable value (memory: InterpreterMemory) : InterpreterMemory =
    { variables = Map.add variable value memory.variables
      arrays = memory.arrays }

let updateArray array value (memory: InterpreterMemory) : InterpreterMemory =
    { variables = memory.variables
      arrays = Map.add array value memory.arrays }

let updateArrayElement array index value (memory: InterpreterMemory) : InterpreterMemory =
    let array' = getArray array memory
    let updatedArray = List.mapi (fun i x -> if i = index then value else x) array'

    { variables = memory.variables
      arrays = Map.add array updatedArray memory.arrays }

let rec arithmeticSemantics mem expr =
  match expr with
  | Num(n) -> n
  | Var(v) -> getValueOfVariable v mem
  | ListElement(array,e) -> getArrayElement mem array (int(arithmeticSemantics mem e))
  | UMinusExpr(e) -> - arithmeticSemantics mem e
  | PowExpr(e1,e2) -> 
      let exponent = int(arithmeticSemantics mem e2)
      if exponent < 0 
          then raise RaisedByNegative
          else pown (arithmeticSemantics mem e1) exponent
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

let rec findEdgesOriginatingInNode node edges =
    match edges with
    | e1 :: t ->
        if e1.source = node
            then e1 :: findEdgesOriginatingInNode node t
            else findEdgesOriginatingInNode node t
    | [] -> []

let isGuardedCommand (edges: Edge list) = 
    let head = List.head edges 

    match head.label with
    | BoolLabel(_) -> true
    | _ -> false

#nowarn "25" // set because the below function never matches on anything but BoolLabel
let rec chooseNextNode (edges: Edge list) memory = 
    match edges with 
    | edge :: t -> match edge.label with 
                   | BoolLabel(bool) -> 
                       if boolSemantics memory bool 
                           then edge.target, printLabel edge.label
                           else chooseNextNode t memory
    | [] -> raise StuckError

let executeCommand edge memory = 
    let newMemory = 
        match edge.label with 
        | CommandSkipLabel -> memory
        | CommandAssignmentLabel(var, expr) -> 
            // failwith "wtf"
            let value = arithmeticSemantics memory expr
            updateVariable var value memory
        | CommandListAssignmentLabel(var, expr1, expr2) -> 
            let index = int <| arithmeticSemantics memory expr1
            let value = arithmeticSemantics memory expr2
            updateArrayElement var index value memory
        | BoolLabel(_) -> raise StuckError
    edge.target, newMemory

let rec executeStep (edges: Edge list) memory : string * InterpreterMemory * string = 
    if List.length edges = 0 
        then raise NoEdgesError
        else 
            if isGuardedCommand edges
                then 
                    let nextNode, action = chooseNextNode edges memory
                    nextNode, memory, action
                else 
                    let edge = List.head edges
                    let nextNode, newMemory = executeCommand edge memory
                    let action = printLabel edge.label
                    nextNode, newMemory, action
            

let rec executeSteps node programGraph memory traceLength = 
    try
        if traceLength > 0 
            then
                let edges = findEdgesOriginatingInNode node programGraph
                let nextNode, newMemory, action = executeStep edges memory

                let step = 
                    {
                        action = action
                        node = nextNode
                        memory = newMemory
                    }
                let trace, termination = executeSteps nextNode programGraph newMemory (traceLength - 1)
                step :: trace, termination
            else 
                [], Running
    with 
    | StuckError | RaisedByNegative -> [], Stuck
    | NoEdgesError -> [], Terminated

let analysis (input: Input) : Output =
    let initialNode = "qI"
    let finalNode = "qF"
    
    match parse Grammar.start_command input.commands with
    | Ok ast ->
        let edge, _ = edges ast initialNode finalNode 0 input.determinism
        let dot = printDot edge
        let trace, termination = executeSteps initialNode edge input.assignment (int input.trace_length)
        { initial_node = initialNode
          final_node = finalNode
          dot = dot
          trace = trace
          termination = termination }
    | Error e -> failwith e.Message

