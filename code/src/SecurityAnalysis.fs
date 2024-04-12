module SecurityAnalysis
open Io.SecurityAnalysis
open AST

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
    {   
        actual = actualFlows input.commands Set.empty
        allowed = []
        is_secure = true
        violations = []
    }