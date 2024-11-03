# Task 2: A Compiler for GCL > **Deadline: March 11, 23:59** You must submit your solutions by pushing them to the git repository assigned to your group.
> The last push before the deadline will be considered as your submission. 

The detailed rules of the mandatory assignment are found [here](README.md).

NOTE: ensure that the master branch of your repository is updated with:

- MacOS, Linux: `./update.sh`
- Windows: `update.ps1`

## Goals

The goal of this task is to implement a compiler that turns GCL programs into Program Graphs (PGs) similar to the results you obtain under 'Program Graph' in [formalmethods.dk/fm4fun](http://www.formalmethods.dk/fm4fun/).

## Detailed Description

> **Relevant files in your group's repository:**
>
> `Compiler.fs`

Your task is to implement the function
```
let analysis (input: Input) : Output =
    failwith "Compiler not yet implemented" // TODO: start here
```
which takes a [GCL program](gcl.md) and produces a program graph in the [DOT language](https://graphviz.org/doc/info/lang.html) - a language for visualizing graphs.
That is, the compiler must produce a program graph in the textual graphviz format used by the export feature on [formalmethods.dk/fm4fun](http://www.formalmethods.dk/fm4fun/). The input also specifies whether you have to produce a deterministic or a non-deterministic program graph.

Launch inspectify as usual:

```
# On Windows
inspectify.ps1 --open
# On macOS and Linux
./inspectify.sh --open
```

Once inspectify has opened in your browser, click on `Compiler` and start working on your task.

## Hints

* **IMPORTANT**: Implement 2 functions: (1) A function `edges` that, given an `Input`, produces an internal representation of the program graph as a set of edges and (2) A function `printDot` that, given a set of edges, generates program graph in the text-based [dot format](https://graphviz.org/doc/info/lang.html).
* You may need to `edges` function in some subsequent task.
* To implement the function `edges` follow [Formal Methods, Chapter 2.2], which explains how to construct a program graph for a GCL program. You will have to implement the function `edges` from the book, which takes as input the AST of a GCL program and produces as output a program graph (represented as a set of edges).
* You have to consider both deterministic and non-deterministic semantics of GCL. You can use a flag to deal with the corresponding type indicated in argument `input : Input`.
* The function `printDot` prints the graph in the dot format given its representation as a set of edges.
* The function `analysis` will invoke first the `edges` function and then the `printDot`function.

## Feedback & Evaluation

You can evaluate your solution by comparing the result to the ones provided by the `fm4fun` or `inspectify` tools.

We encourage you to proactively ask for feedback from the TAs and the teacher.
