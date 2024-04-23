module SignAnalysis

open Io.SignAnalysis
open Io.GCL
open AST
open Parser
open Compiler

let abstract_power =
    Map.ofList [
        (Negative, Negative), List.Empty;
        (Negative, Zero), [Positive];
        (Negative, Positive), [Negative; Positive];
        (Zero, Negative), List.Empty;
        (Zero, Zero), [Positive];
        (Zero, Positive), [Zero];
        (Positive, Negative), List.Empty;
        (Positive, Zero), [Positive];
        (Positive, Positive), [Positive]
    ]
let abstract_divison  =
        Map.ofList [
        ((Negative,Negative), [Positive; Zero]);
        ((Negative,Zero), List.empty);
        ((Negative,Positive), [Negative;Zero]);

        ((Zero,Negative), [Zero]);
        ((Zero,Zero),  List.empty);
        ((Zero,Positive), [Zero]);

        ((Positive,Negative), [Negative;Zero]);
        ((Positive,Zero), List.empty);
        ((Positive,Positive), [Positive; Zero]);
    ]
let abstract_plus  =
        Map.ofList [
        ((Negative,Negative), [Negative]);
        ((Negative,Zero), [Negative]);
        ((Negative,Positive), [Negative; Positive; Zero]);

        ((Zero,Negative), [Negative]);
        ((Zero,Zero),  [Zero]);
        ((Zero,Positive), [Positive]);

        ((Positive,Negative), [Negative;Zero;Positive]);
        ((Positive,Zero), [Positive]);
        ((Positive,Positive), [Positive]);
    ]
let abstract_multiplication =
    Map.ofList [
        ((Negative, Negative),  [Positive]);
        ((Negative, Zero),      [Zero]);
        ((Negative, Positive),  [Negative]);

        ((Zero, Negative),      [Zero]);
        ((Zero, Zero),          [Zero]);
        ((Zero, Positive),      [Zero]);

        ((Positive, Negative),  [Negative]);
        ((Positive, Zero),      [Zero]);
        ((Positive, Positive),  [Positive]);
    ]
let abstract_subtraction  =
    Map.ofList [
        ((Negative,Negative), [Negative; Positive; Zero]);
        ((Negative,Zero), [Negative]);
        ((Negative,Positive), [Negative]);

        ((Zero,Negative), [Positive]);
        ((Zero,Zero),  [Zero]);
        ((Zero,Positive), [Negative]);

        ((Positive,Negative), [Positive]);
        ((Positive,Zero), [Positive]);
        ((Positive,Positive), [Negative;Positive;Zero]);

    ]
let abstract_lessThan = 
    Map.ofList [
        ((Negative,Negative),   [true; false]);
        ((Negative,Zero),       [true]);
        ((Negative,Positive),   [true]);

        ((Zero,Negative),       [false]);
        ((Zero,Zero),           [false]);
        ((Zero,Positive),       [true]);

        ((Positive,Negative),   [false]);
        ((Positive,Zero),       [false]);
        ((Positive,Positive),   [true; false]);
    ]
let abstract_equals = 
    Map.ofList [
        ((Negative,Negative),   [true;false]);
        ((Negative,Zero),       [false]);
        ((Negative,Positive),   [false]);

        ((Zero,Negative),       [false]);
        ((Zero,Zero),           [true]);
        ((Zero,Positive),       [false]);

        ((Positive,Negative),   [false]);
        ((Positive,Zero),       [false]);
        ((Positive,Positive),   [true;false]);
    ]
let abstract_notEquals = 
    Map.ofList [
        ((Negative,Negative),   [false;true]);
        ((Negative,Zero),       [true]);
        ((Negative,Positive),   [true]);

        ((Zero,Negative),       [true]);
        ((Zero,Zero),           [false]);
        ((Zero,Positive),       [true]);

        ((Positive,Negative),   [true]);
        ((Positive,Zero),       [true]);
        ((Positive,Positive),   [false;true]);
    ]
let abstract_greaterThanOrEquals = 
    Map.ofList [
        ((Negative,Negative),   [true; false]);
        ((Negative,Zero),       [false]);
        ((Negative,Positive),   [false]);

        ((Zero,Negative),       [true]);
        ((Zero,Zero),           [true]);
        ((Zero,Positive),       [false]);

        ((Positive,Negative),   [true]);
        ((Positive,Zero),       [true]);
        ((Positive,Positive),   [true; false]);
    ]
let abstract_lessThanOrEquals = 
    Map.ofList [
        ((Negative,Negative),   [true; false]);
        ((Negative,Zero),       [true]);
        ((Negative,Positive),   [true]);

        ((Zero,Negative),       [false]);
        ((Zero,Zero),           [true]);
        ((Zero,Positive),       [true]);

        ((Positive,Negative),   [false]);
        ((Positive,Zero),       [false]);
        ((Positive,Positive),   [true; false]);
    ]
let abstract_AndSC = 
    Map.ofList [
        ((true, true),          [true]);
        ((true, false),         [false]);

        ((false, true),         [false]);
        ((false, false),        [false]);
    ]
let abstract_And = 
    Map.ofList [
        ((true, true),          [true]);
        ((true, false),         [false]);

        ((false, true),         [false]);
        ((false, false),        [false]);
    ]
let abstract_OrSC = 
    Map.ofList [
        ((true, true),          [true]);
        ((true, false),         [true]);

        ((false, true),         [true]);
        ((false, false),        [false]);
    ]
let abstract_Or = 
    Map.ofList [
        ((true, true),          [true]);
        ((true, false),         [true]);

        ((false, true),         [true]);
        ((false, false),        [false]);
    ]
let abstract_greaterThan = 
    Map.ofList [
        ((Negative,Negative),   [true; false]);
        ((Negative,Zero),       [false]);
        ((Negative,Positive),   [false]);

        ((Zero,Negative),       [true]);
        ((Zero,Zero),           [false]);
        ((Zero,Positive),       [false]);

        ((Positive,Negative),   [true]);
        ((Positive,Zero),       [true]);
        ((Positive,Positive),   [true; false]);
    ]

let signOf = function
    |n when n=0 -> [Zero]
    |n when n<0 -> [Negative]
    |_ -> [Positive]

let ophat list1 list2 operator =
    List.rev (List.fold ( fun acc1 sign1 ->
                                                ((List.fold ( fun acc2 sign2 -> (Map.find (sign1,sign2) operator) @ acc2)) acc1 list2)
                        ) [] list1)
let rec abstract_arithmetic_semantics (variableMemory : Map<Variable, Sign> ) (arrayMemory : Map<Array, List<Sign>>) = function
    |Num n -> signOf n
    |Var str -> [Map.find str variableMemory]
    |PlusExpr (a1, a2) -> ophat
                            (abstract_arithmetic_semantics variableMemory arrayMemory a1)
                            (abstract_arithmetic_semantics variableMemory arrayMemory a2)
                            (abstract_plus)
    |MinusExpr (a1,a2) -> ophat
                            (abstract_arithmetic_semantics variableMemory arrayMemory a1)
                            (abstract_arithmetic_semantics variableMemory arrayMemory a2 )
                            (abstract_subtraction)
    |TimesExpr (a1,a2) -> ophat
                            (abstract_arithmetic_semantics variableMemory arrayMemory a1)
                            (abstract_arithmetic_semantics variableMemory arrayMemory a2 )
                            (abstract_multiplication)
    |DivExpr (a1,a2) -> ophat
                            (abstract_arithmetic_semantics variableMemory arrayMemory a1)
                            (abstract_arithmetic_semantics variableMemory arrayMemory a2 )
                            (abstract_divison)
    
    |PowExpr (a1, a2) -> ophat
                            (abstract_arithmetic_semantics variableMemory arrayMemory a1)
                            (abstract_arithmetic_semantics variableMemory arrayMemory a2 )
                            (abstract_power)
    
    |UMinusExpr a1 -> List.collect (fun sign ->
                                match sign with
                                |Positive -> [Negative]
                                |Negative -> [Positive]
                                |Zero -> [Zero]
                            ) (abstract_arithmetic_semantics variableMemory arrayMemory a1)
    | ListElement (st,a1) -> failwith "MISSING ARRAY"

let rec abstract_boolean_semantics (variableMemory:Map<Variable,Sign>) arrayMemory = function
    |True ->  [true]
    |False -> [false]
    |Equal (a1,a2) -> ophat
                            (abstract_arithmetic_semantics variableMemory arrayMemory a1)
                            (abstract_arithmetic_semantics variableMemory arrayMemory a2)
                            (abstract_equals)

    |NotEqual(a1,a2) ->ophat
                            (abstract_arithmetic_semantics variableMemory arrayMemory a1)
                            (abstract_arithmetic_semantics variableMemory arrayMemory a2)
                            (abstract_notEquals)

    |Greater(a1,a2) ->ophat
                            (abstract_arithmetic_semantics variableMemory arrayMemory a1)
                            (abstract_arithmetic_semantics variableMemory arrayMemory a2)
                            (abstract_greaterThan)

    |GreaterEqual(a1,a2) -> ophat
                                    (abstract_arithmetic_semantics variableMemory arrayMemory a1)
                                    (abstract_arithmetic_semantics variableMemory arrayMemory a2)
                                    (abstract_greaterThanOrEquals)

    |Less(a1,a2) ->ophat
                            (abstract_arithmetic_semantics variableMemory arrayMemory a1)
                            (abstract_arithmetic_semantics variableMemory arrayMemory a2)
                            (abstract_lessThan)

    
    |LessEqual(a1,a2) ->ophat
                                    (abstract_arithmetic_semantics variableMemory arrayMemory a1)
                                    (abstract_arithmetic_semantics variableMemory arrayMemory a2)
                                    (abstract_lessThanOrEquals)

    |ShortOr (b1,b2) -> ophat
                            (abstract_boolean_semantics variableMemory arrayMemory b1)
                            (abstract_boolean_semantics variableMemory arrayMemory b2)
                            (abstract_OrSC)
    |Or (b1,b2) -> ophat
                            (abstract_boolean_semantics variableMemory arrayMemory b1)
                            (abstract_boolean_semantics variableMemory arrayMemory b2)
                            (abstract_Or)

    |ShortAnd (b1,b2) -> ophat
                            (abstract_boolean_semantics variableMemory arrayMemory b1)
                            (abstract_boolean_semantics variableMemory arrayMemory b2)
                            (abstract_AndSC)

    |And (b1,b2) -> ophat
                            (abstract_boolean_semantics variableMemory arrayMemory b1)
                            (abstract_boolean_semantics variableMemory arrayMemory b2)
                            (abstract_And)
    |Not b1 -> List.map (fun truthValue -> not truthValue) (abstract_boolean_semantics variableMemory arrayMemory b1)

let rec checkOneMemory action (memory:SignMemory) =
    match memory with
    |{variables = var; arrays= arr} when List.contains true (abstract_boolean_semantics var arr action) -> true
    |_ -> false

let rec boolSemantics (action : bool) (M:List<SignMemory>) =
    match M with
    |[] -> []
    |memory::xt when checkOneMemory action memory -> memory :: boolSemantics action xt
    |_::xt -> boolSemantics action xt

let reAssignVariable (variableMemory : Map<Variable, Sign>) variable (signs : Sign) =
    Map.add variable signs variableMemory

let rec unwrapS key signs varMem arrayMem = 
    match signs with
    | [] -> []
    | s::xt -> {variables = reAssignVariable varMem key s; arrays = arrayMem}  :: unwrapS key xt varMem arrayMem

let rec arithmeticSemantics (str : string) (expr: expr) (M:List<SignMemory>) acc =
    match M with
    |[] -> acc
    |{variables = varMem; arrays = arrayMem}::xt -> 
        let s = (abstract_arithmetic_semantics varMem arrayMem expr)
        let findNewVarMem = unwrapS str s varMem arrayMem
        arithmeticSemantics str expr xt (acc @ findNewVarMem)

(*let checkListCondition mem expr1 epxr2 str =
    not (Set.isEmpty (Set.intersect (Set.ofList [Zero,Positive]) (Set.ofList (semantics  ))))

let rec listSemantics str expr1 expr2 M acc =
    match M with
    | [] -> acc
*)
let semantics (edgeaction:Label) (M:List<SignMemory>) = 
    match edgeaction with 
    |CommandSkipLabel -> M
    |BoolLabel(b) -> boolSemantics b M
    |CommandAssignmentLabel (s,e) -> arithmeticSemantics s e M []
    |CommandListAssignmentLabel (s,e1,e2) -> failwith "error"

let updateValue key value map =
    map |> Map.remove key |> Map.add key value

let checkSubset action source target map =
    match (Map.tryFind target map) with
    | Some value -> not (Set.isSubset (Set.ofList (semantics action (Map.find source map))) (Set.ofList (value)))
    | None -> true

let rec updateWorklist outEdges map worklist = 
    printfn("pikk")
    match outEdges with
    | [] -> worklist, map
    | e::rest when checkSubset (e.label) (e.source) (e.target) map -> 
        printfn("lol")
        match Map.tryFind e.target map with
        | Some value -> updateWorklist rest (updateValue e.target (value @ semantics e.label (Map.find e.source map)) map) ([e.target] @ worklist)
        | None -> updateWorklist rest (updateValue e.target ([] @ semantics e.label (Map.find e.source map)) map) ([e.target] @ worklist)
    | e::rest -> updateWorklist rest map worklist


let rec worklistAlgo edges map worklist = 
    printfn("cock")
    match Set.toList(worklist) with
    | [] -> map
    | q::rest -> 
        let outEdges = Interpreter.findEdgesOriginatingInNode q edges
        let newWorklist, newMap = updateWorklist outEdges map rest
        worklistAlgo edges newMap (Set.union (Set.ofList rest) (Set.ofList newWorklist))

let analysis (input: Input) : Output =
    match parse Grammar.start_command input.commands with
    | Ok ast -> 
        let compiler_output = Compiler.analysis { 
            commands = input.commands
            determinism = input.determinism} 
        let edges, _ = Compiler.edges ast "qS" "qF" 0 input.determinism
        let node = worklistAlgo edges (Map.empty.Add ("qS", [input.assignment])) (Set.ofList ["qS"])
        { dot = compiler_output.dot
          final_node = "qS"
          initial_node = "qF"
          nodes = node}
    | Error e -> failwith e.Message
