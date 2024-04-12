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

let rec findVariablesExpr expr = 
    match expr with
    | Num n -> Set.empty
    | Var(s)-> Set[s]
    | ListElement(var,expr) -> Set.union Set[var] (findVariablesExpr expr)
    | TimesExpr(expr1, expr2) -> Set.union (findVariablesExpr expr1) (findVariablesExpr expr2)
    | DivExpr(expr1,expr2) -> Set.union (findVariablesExpr expr1) (findVariablesExpr expr2)
    | PlusExpr(expr1,expr2) -> Set.union (findVariablesExpr expr1) (findVariablesExpr expr2)
    | MinusExpr(expr1,expr2) -> Set.union (findVariablesExpr expr1) (findVariablesExpr expr2)
    | PowExpr(expr1,expr2) -> Set.union (findVariablesExpr expr1) (findVariablesExpr expr2)
    | UMinusExpr(expr) -> findVariablesExpr(expr)

let rec findVariablesBool bool  = 
    match bool with
    | True -> Set.empty
    | False -> Set.empty
    | And(b1,b2) -> Set.union (findVariablesBool b1) (findVariablesBool b2)
    | Or(b1,b2) -> Set.union (findVariablesBool b1) (findVariablesBool b2)
    | Not(b) -> findVariablesBool b
    | ShortAnd(b1,b2) -> Set.union (findVariablesBool b1) (findVariablesBool b2)
    | ShortOr(b1,b2) -> Set.union (findVariablesBool b1) (findVariablesBool b2)
    | Equal(expr1,expr2) -> Set.union (findVariablesExpr expr1) (findVariablesExpr expr2)
    | NotEqual(expr1,expr2) -> Set.union (findVariablesExpr expr1) (findVariablesExpr expr2)
    | Less(expr1,expr2) -> Set.union (findVariablesExpr expr1) (findVariablesExpr expr2)
    | LessEqual(expr1,expr2) -> Set.union (findVariablesExpr expr1) (findVariablesExpr expr2)
    | Greater(expr1,expr2) -> Set.union (findVariablesExpr expr1) (findVariablesExpr expr2)
    | GreaterEqual(expr1,expr2) -> Set.union (findVariablesExpr expr1) (findVariablesExpr expr2)

let rec setToFlows flowFrom flowTo = 
    match Set.toList flowFrom with
    | [] -> Set.empty
    | x::xs -> Set.union (Set[({from = flowTo ; into = x})]) (setToFlows xs flowTo)

let rec implicitDeps gc =  
    match gc with
    | Implies(b, c) -> findVariablesBool b
    | GuardedOr(gc1, gc2) -> Set.union (implicitDeps gc1) (implicitDeps gc2) 

let rec actualFlows (commands : command) deps =
    match commands with 
    | Skip -> Set.empty
    | Assignment(var, expr)-> setToFlows (Set.union deps (findVariablesExpr expr)) var
    | ListAssignment(var, expr1, expr2) -> setToFlows (Set.union deps (Set.union (findVariablesExpr expr1) (findVariablesExpr expr2))) var 
    | Program(c1,c2) -> Set.union (actualFlows c1 deps) (actualFlows c2 deps)
    | If(gc) -> actualFlowsGC gc deps
    | Do(gc) -> actualFlowsGC gc deps
and actualFlowsGC gcommand deps = 
    match gcommand with
    | Implies(b,c) -> actualFlows c (Set.union deps (findVariablesBool b))
    | GuardedOr(gc1,gc2) -> Set.union (actualFlowsGC gc1 deps)  (actualFlowsGC gc2 (Set.union deps (implicitDeps gc1)))

let analysis (input: Input) : Output =
    match parse Grammar.start_command input.commands with 
    | Ok ast -> 
        let actualFlows, _ = actualFlows ast Set.empty
        let allowedFlows = allowedFlows input.lattice input.classification
        let violatingFlows = violatingFlows actualFlows allowedFlows
        let isSecure = List.isEmpty violatingFlows
        { actual = actualFlows
          allowed = allowedFlows
          violations = violatingFlows
          is_secure = isSecure }
    | Error e -> failwith e.Message
