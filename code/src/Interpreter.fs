module Interpreter
open Io.Interpreter
open Io.GCL

let analysis (input: Input) : Output =
    failwith "Interpreter not yet implemented" // TODO: start here

    { initial_node = ""
      final_node = ""
      dot = ""
      trace = []
      termination = Terminated }

let getValueOfVariable variable (memory: InterpreterMemory) : int64 =
    Map.find variable memory.variables

let getValueOfArray array (memory: InterpreterMemory) : List<int64> =
    Map.find array memory.arrays

let updateVariable variable value (memory: InterpreterMemory) : InterpreterMemory =
    { variables = Map.add variable value memory.variables
      arrays = memory.arrays }

let updateArray array value (memory: InterpreterMemory) : InterpreterMemory =
    { variables = memory.variables
      arrays = Map.add array value memory.arrays }
