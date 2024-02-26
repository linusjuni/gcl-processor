# Task 1: A parser for GCL

> **Deadline: March 4, 23:59**
>
> You must submit your solutions by pushing them to the git repository assigned to your group.
> The last push before the deadline will be considered as your submission. 

The detailed rules of the mandatory assignment are found [here](README.md).

NOTE: ensure that the master branch of your repository is updated with

```
git remote add upstream https://github.com/team-checkr/fsharp-starter
git checkout master
git pull upstream main
```

## Goals

The goal of this task is to build a parser for [GCL](gcl.md) that accepts or rejects programs and builds ASTs for them, thus working like the syntax checker of [formalmethods.dk/fm4fun](http://www.formalmethods.dk/fm4fun/). 

## Detailed Description

> **Relevant files in your group's repository:** 
> 
> `Parser.fs; Lexer.fsl; Grammar.fsy; AST.fs`

You should implement a parser that takes as input a string, which is intended to describe a [GCL program](gcl.md) but may contain errors, and build an abstract syntax tree (AST) for it. 

In addition, the program must produce compilation results: it should return whether the input is a program accepted by the [GCL grammar specified in this repository](gcl.md).

Furthermore, you must implement a "Pretty Printer", i.e. a code generator, that prints the AST so you can easily check your solution.

Launch inspectify as usual:

```
# On Windows
inspectify.ps1 --open
# On macOS and Linux
./inspectify.sh --open
```

Once inspectify has opened in your browser, click on `Parse` and start working on your task


## Hints
- Get inspired by the calculator example of the code framework in your group's repository.
- Start with the [GCL grammar specified in this repository](gcl.md) and adapt it to your parser generator:
    - `AST.fs`: add one type for each non-terminal symbol of the grammar, define the constructors of the type.
    - `Grammar.fsy`: add new non-terminals with their productions, add token declarations, and add the code generation part (based on your new types). You may need to specify precedence and associativity of some operators in the parser generator language, or by applying some of the grammar transformations seen in class. 
    - `Lexer.fsl`: add rules to define the new tokens. 
    - `Parser.fs`: you need to implement function 'prettyPrint', which, given the AST of the parsed GCL program, generates the code as a string.

## Feedback & Evaluation

You will need the GCL parser developed in this task to complete the follow-up tasks.

We encourage you to proactively ask for feedback from the TAs and the teacher.

Please submit your task solution by simply pushing changes to the main (master) branch of your repository. You can do this as many times as you want. We will have a look at the latest update (before the deadline).
