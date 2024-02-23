# Task 0: Building a calculator

NOTE: ensure that the master branch of your repository is updated with

```
git remote add upstream https://github.com/team-checkr/fsharp-starter
git checkout master
git pull upstream main
```

## Goals

The goal of this task is to complete the calculator.

## Detailed Description

> **Relevant files in your group's repository:** 
> 
> `Parse.fs; Lexer.fsl; Calculator.fsy; AST.fs`

Launch inspectify:

```
# On Windows
inspectify.ps1 --open
# On macOS and Linux
./inspectify.sh --open
```

Once Inspectify has opened in your browser, click on `Calculator`. Inspectify will complain. Your goal is be able to write arithmetic expressions like the ones of GCL (without variables and arrays).

## Hints
- Open file `Calculator.fs` and complete function `evaluate`.
- Use pattern matching to implement `evaluate` recursively. File `AST.fs` contains the definition of type `expr`, which you should follow to identify the cases neded in the pattern maching.
- Sketch of a possible solution:

```
match expr with
    | Num(x) -> Ok x
    | PlusExpr(x,y) -> 
    ...
```

Done? Move to [task 1](task1.md).
