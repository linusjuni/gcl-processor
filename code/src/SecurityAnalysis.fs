module SecurityAnalysis
open Io.SecurityAnalysis
open System
open AST
open Parser

type VariableClassification = 
    { variable: String 
      level: String }

let rec isTransitive lattice fromLevel intoLevel = 
    let flowsFromLevel = List.where (fun element -> element.from = fromLevel) lattice.rules
    List.fold (checkElement lattice fromLevel intoLevel) false flowsFromLevel

and checkElement lattice fromLevel intoLevel state element =
    let bool = 
        if element.into = intoLevel 
            then true 
            else isTransitive lattice element.into intoLevel
    bool || state

let isSymmetric level level' = level = level'

let isFlowAllowed (lattice: SecurityLatticeInput) (fromLevel: String) (intoLevel: String) = 
    isSymmetric fromLevel intoLevel
    || 
    isTransitive lattice fromLevel intoLevel
    

let intoVariableIterator lattice fromVariable flowsAllowed variable level = 
    let intoVariable = 
        { variable = variable 
          level = level}
    if isFlowAllowed lattice intoVariable.level fromVariable.level
        then
            let flow = 
                { from = intoVariable.variable 
                  into = fromVariable.variable}
            flow::flowsAllowed 
        else
            flowsAllowed
        

let fromVariableIterator lattice classification flowsAllowed variable level = 
    let fromVariable = 
        { variable = variable 
          level = level }
    Map.fold (intoVariableIterator lattice fromVariable) [] classification @ flowsAllowed

let allowedFlows lattice classification =
    Map.fold (fromVariableIterator lattice classification) [] classification

let violatingFlows actualFlows allowedFlows =
    let actualFlows' = Set.ofList actualFlows
    let allowedFlows' = Set.ofList allowedFlows
    let violatingFlows = Set.difference actualFlows' allowedFlows'
    List.ofSeq violatingFlows

let analysis (input: Input) : Output =
    match parse Grammar.start_command input.commands with 
    | Ok ast -> 
        let actualFlows = []
        let allowedFlows = allowedFlows input.lattice input.classification
        let violatingFlows = violatingFlows actualFlows allowedFlows
        let isSecure = List.isEmpty violatingFlows
        { actual = actualFlows
          allowed = allowedFlows
          violations = violatingFlows
          is_secure = isSecure }
    | Error e -> failwith e.Message
