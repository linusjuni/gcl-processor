module SignAnalysis
open Io.SignAnalysis

let analysis (input: Input) : Output =
    let compiler_output = Compiler.analysis {
        commands = input.commands
        determinism = input.determinism
    }

    {
        initial_node = "qS"
        final_node = "qF"
        dot = compiler_output.dot
        nodes = Map.empty
    }
