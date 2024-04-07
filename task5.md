# Task 5: A Security Analyser for GCL

> **Deadline: April 15, 23:59**
>
> You must submit your solutions by pushing them to the git repository assigned to your group.
> The last push before the deadline will be considered as your submission. 

The detailed rules of the mandatory assignment are found [here](README.md).

NOTE: ensure that the main branch of your repository is updated with:

- MacOS, Linux: `./update.sh`
- Windows: `update.ps1`


## Goals

The goal of this task is to implement a tool for security analysis of GCL programs, that works as a simplified version of the environment `Security Analysis` on [formalmethods.dk/fm4fun](http://www.formalmethods.dk/fm4fun/). The security analysis will follow the approach of the lectures on Language-Based security (see material on DTU Learn and [Formal methods, Chapter 5]).


## Detailed Description

> **Relevant files in your group's repository:** 
> 
> `SecurityAnalysis.fs`

Your task is to implement the function
```
let analysis (input: Input) : Output =
```

The above function `analysis` takes as input:
- a [GCL program](gcl.md);
- a security lattice;
- a security classification for variables.

As an output the function should produce:
- the actual flows in the program;
- the flows allowed by the security policy;
- the violations of the program;
- whether or not the program is deemd secure.

The exact definitions of the types  `Input` and `Output` are found in the file `Io.fs`, which you are *not* allowed to modify.

Launch inspectify as usual:

```
# On Windows
inspectify.ps1 --open
# On macOS and Linux
./inspectify.sh --open
```

Once inspectify has opened in your browser, click on `Security` and start working on your task.

## Hints

* It is highly recommended to first make a plan of what functions you will have to implement and how they should interact with each other.
* Follow the approach of the lectures on Language-Based security (see material on DTU Learn and in [Formal Methods, Chapters 5.3 and 5.4] for implementing security analysis.
* The actual flows in the program can be computed following the exercise that is part of the lectures, i.e. as a recursive function on the AST of programs, which computes the actual flows mimicking what is being done in the security analysis of Def. 5.12. Note that definition 5.12 computes the result (secure/insecure) directly but you need to produce instead the list of actual flows.
* The flows allowed by the security policy is something also covered by lecture exercises. Follow the ideas of [Formal Methods, Chapter 5.4], in particular paragraph "The other is a security classification..."
* The violations of the program can be simply computed as the actual flows not contained in the  allowed flows.
* The result (secure/insecure) is a simple check of whether the set of violations is empty.

## Feedback & Evaluation

You can evaluate your solution by comparing the result to the ones provided by the `fm4fun` or `inspectify` tools.

We encourage you to proactively ask for feedback from the TAs and the teacher.

