# Task 6: A Sign Analyser for GCL

> > **Make sure to [update](#update-instructions) your repository and inspectify** 
>
> **Deadline: April 25, 23:59**
>
> You must submit your solutions by pushing them to the git repository assigned to your group.
> The last push before the deadline will be considered as your submission. 

The detailed rules of the mandatory assignment are found [here](README.md).

NOTE: ensure that the main branch of your repository is updated with:

- MacOS, Linux: `./update.sh`
- Windows: `update.ps1`

## Goals

The goal for this task is to implement a tool that overapproximates the possible signs of variables and array elements for each node in a given program graph. 
Examples of such a program analysis are found in the inspectify reference implementations and on [formalmethods.dk/fm4fun](http://www.formalmethods.dk/fm4fun/) under the environment “Detection of Signs Analysis”.

We recommend implementing a detection of signs analysis using the approach described in [Formal Methods, Chapter 4].

## Detailed Description

> **Relevant files in your group's repository:** 
> 
> `SignAnalysis.fs`

Your task is to implement the function
```
let analysis (input: Input) : Output =
    failwith "Sign analysis not yet implemented" // TODO: start here
```

The above function `analysis` takes as input a type that consists of
- a string representation of a GCL program,
- whether we consider a deterministic program graph or not, and
- the initial sign assignment.

We represent a sign assignment using the type `SignMemory`.
Like a regular memory (see task 3), a sign memory consists of two maps, one for variables and one for array elements.
However, a sign memory maps every variable and every array element to a sign instead of an integer value.

The function `analysis` must return the result of the detection of signs analysis as a structure of type `Output`, which consists of the following information:
- `initial_node`: the name of the initial node in the program graph.
- `final_node`: the name of the final node in the program graph.
- `nodes`: the result of your detection of signs analaysis as a map from node names to a list of sign memories, which should overapproximate all possible sign memories that can be reached from the initial sign memory.
- `dot`: a string with the dot code of the program graph as in task 2.

The exact definitions of the types `Input` and `Output` are found in `Io.fs`.
The following excerpt contains the relevant part:
```
module SignAnalysis =
  type Input =
    { commands: string
      determinism: GCL.Determinism
      assignment: SignAnalysis.SignMemory }
  type Output =
    { initial_node: string
      final_node: string
      nodes: Map<string, List<SignAnalysis.SignMemory>>
      dot: string }
  type SignMemory =
    { variables: Map<GCL.Variable, SignAnalysis.Sign>
      arrays: Map<GCL.Array, List<SignAnalysis.Sign>> }
  [<JsonFSharpConverter(BaseUnionEncoding = JsonUnionEncoding.ExternalTag + JsonUnionEncoding.UnwrapFieldlessTags + JsonUnionEncoding.UnwrapSingleFieldCases)>]
  type Sign =
    | Positive
    | Zero
    | Negative
```

## Hints

- Follow [Formal Methods, Chapter 4] for implementing the detection of sign analysis.
- Your starting point is the algorithm in Figure 4.15 on page 59  **We recommend starting with the algorithm and then moving backwards through [Formal Methods, Chapter 4].** That is, look up the definitions used in the algorithm and implement corresponding functions. 
- The aforementioned algorithm includes computing a fixed point. There are many different approaches to do so. You can find two approaches in the FM book. Alternatively, you can try to implement a function that takes a map as in the `Output` type and returns such a map in which the analysis information of *all* nodes have been updated. Once you have such a function, you can apply it again and again until the function does not change the map.
- For detection of sign analysis, you can find
    - the analysis functions on page on pages 54 and 55,
    - the abstract evaluation functions on pages 52 and 53, and
    - the set of abstract memories on page 48.


