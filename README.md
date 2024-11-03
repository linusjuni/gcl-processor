# Parser, Compiler & Interpreter in F#

This project is a compilation tool developed in F# based on the Guarded Command Language (GCL) for the Computer Science Modelling course at DTU.

## Components

- **Parser**: Converts tokenized code into an Abstract Syntax Tree (AST).
- **Compiler**: Transforms the AST into a flow graph of nodes and edges.
- **Interpreter**: Executes the graph by following edges and performing actions.

## Running the Program

Navigate to the `code` directory and run:

```bash
./inspectify.sh -o
