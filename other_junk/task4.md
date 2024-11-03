# Task 4: Program Verification

> **Make sure to [update](#update-instructions) your repository and inspectify** 
>
> **Deadline: April 4, 23:59**
>
> You must submit your solutions by pushing them to the git repository assigned to your group.
> The last push before the deadline will be considered as your submission. 

The detailed rules of the mandatory assignment are found [here](README.md).

NOTE: ensure that the master branch of your repository is updated with:

- MacOS, Linux: `./update.sh`
- Windows: `update.ps1`

## Goals

The goal of this task is to apply your knowledge about program verification to write formal correctness proofs for five GCL programs.


## Detailed Description

> **Relevant files in your group's repository:**
>
> `task4/part1.gcl`
> `task4/part2.gcl`
> `task4/part3.gcl`
> `task4/part4.gcl`
> `task4/part5.gcl`
> `task4/part6.gcl`

Each file listed above contains a Floyd-Hoare triple `{ P } C { Q }` consisting of a precondition `P`, a GCL command `C` and a postcondition `Q`.


For each of those Floyd-Hoare triples `{ P } C { Q }`, your task is to show that the triple is valid, i.e., `|= { P } C { Q }`, by extending it to a *fully annotated command*.

You can use our verification playground [CHIP](https://chip-pv.netlify.app) to develop and check your solutions.
CHIP should yield both "verified" *and* "The program is fully annotated".

Notice that CHIP does not report "verified" for some of the given example programs because you might have to add suitable invariants first.

## Rules

You can add invariants and as many annotations as you want. However, you are *not* allowed to change the given command `C`, 
the precondition `P`, or the postcondition `Q`. This also means that you are *not* allowed to add any annotations above the provided precondition or below the provided postcondition.

As usual, push your solution to your group's git repository before the deadline. Your solution to each part must be provided in the initially provided file in the directory `task4`. For example, the solution to part 1 must be contained in the file `task4/part1.gcl`.


## Hints

* All material needed for this task is covered by the slides on program verification, which are available on DTU learn.
* CHIP is typically powerful enough to verify a (correct) proof outline as long as you provide a precondition, a postcondition, and suitable invariants for all loops. You can thus first play around to find invariants before attempting to write a fully annotated command.
* You might want to verify smaller parts of a given program with CHIP before considering the whole program.
* Finding loop invariants is difficult. It may take you some time to fully comprehend how each program works in detail.
* You can use your interpreter to test the given programs on different inputs in order to get some intuition about potential invariants.
* You can also try to write a proof outline and infer from that how an invariant needs to look like.


## Feedback & Evaluation

Besides using CHIP, we encourage you to proactively ask for feedback from the TAs and the teacher.