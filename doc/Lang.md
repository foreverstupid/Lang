# LANG

**Lang** is a Turing-full script programming language. The main concept of the
program in Lang is an *expresion* - the statement that returns some value. The
Lang program itself is expression.

## BNF of Lang
**\<expression\>** ::= **\<group\>** | [ **\<unar\>** ] **\<operand\>** [ **\<tail\>** ] { **\<binar\>** **\<expression\>** }

**\<group\>** ::= **{** **\<expression\>** { **;** **\<expression\>** } **}**

**\<tail\>** ::= **\<indexator\>** **\<tail\>** | **\<args\>** **\<tail\>** |

**\<indexator\>** ::= **[** **\<expression\>** **]**

**\<args\>** ::= **(** [ **\<expression\>** {, **\<expression\>** } ] **)**

**\<operand\>** ::= **(** **\<expression\>** **)** | **\<if_expression\>** | **\<while_expression\>** | **\<literal\>** | **\<lambda\>**

**\<literal\>** ::= **\<string\>** | **\<int\>** | **\<float\>** | **\<variable\>**

**\<lambda\>** ::= **\<params\>** **=>** **\<expression\>**

**\<params\>** ::= **[** [ **\<variable\>** { ,**\<variable\>** } ] **]**

**\<if_expression\>** ::= **if** **(** **\<expression\>** **)** **\<expression\>** [ **or** **\<expression\>** ]

**\<while_expression\>** ::= **as** **(** **\<expression\>** **)** **\<expression\>**

**\<unar\>** ::= **-** | **!** | **$**

**\<binar\>** ::= **+** | **-** | **\*** | **/** | **%** | **>** | **<** | **~** | **&** | **|** | **=** | **?** | **->**

**\<variable\>** ::= **\<letter\>**{ **\<letter_or_digit\>** }

**\<int\>** ::= **\<digit\>**{ **\<digit\>** }

**\<float\>** ::= **\<digit\>**{ **\<digit\>** }**.**{ **\<digit\>** }

**\<string\>** ::= **"**{**\<any_symbol\>**}**"**

**\<digit\>** ::= **0**..**9**

**\<letter\>** ::= **a**..**z** | **A**..**Z** | **_**

**\<letter_or_digit\>** ::= **\<letter\>** | **\<digit\>**

## Main concepts

### Data types

Lang has three main data types: integer number, float point number, and string. All number literals are non-negative (to get negative literal use negate operation). The fourth data type is a functional type that represents some lambda function.

#### Casting

Data types can be automatically casted to each other in some situations. Thus, in every binary operations the right operand will be casted to the type of the left operand. If such a transformation is not possible then the programm finishes with an error. Moreover there exists the special cast operator (?).

#### Bool

There isn't such a type as bool in Lang. Nevertheless, integer, float, and string could be used as bool in conditions. Thus, 0, 0.0, and "" are used as **false**. All others values are used as **true**.

### Variables

Variables in Lang are created on their first assignment from some values. They can change their values and types. In a sense variables are just labels for data, to get the variable value a special dereference operation ($) is used. Thus, variables can be thought as pointers to data that should be dereferenced. Variables could have another variables as their values. Variables of a functional type could be evaluated as functions.

### Lambdas

All functions in Lang are lambdas. You can give them names by assignment to some variable. Lambda return value is the value of its body expression. Any lambda takes values (maybe none of them) as its input parameters, but as far as variable itself can be a value, you can pass a variable without dereferencing, that is similar to passing arguments by the reference in C++. Lambda parameters override any outer variables of the same name.

### Buil-in functions

Lang has several built-in named functions that shouldn't be described to be used. Their behaviour is the same as the behaviour of ordinary lambdas.

### Expression

Expression is the main concept of Lang. It is a set of operations over data that returns a single value. There exists the following kinds of expressions:

1. **Usual expression**: operands linked by binary and unary operations (e.g. arithmetic operations, assignment, etc.)
2. **Expression group**: several expressions grouped together. The value of such an expression is the value of the last expression of the group.
3. **Lambda**: expression that can be evaluated only after substitution certain values as parameters.
4. **If-expression**: expression that evaluates or not according to some condition. Can have else-part (if-else-expression) that is evaluated if the consition is false.
5. **Cycle-expression**: expression that evaluates several times while some consition is true. Its value is a value of its last iteration. If none iteration is performed, returns the special None value that cannot be used in any expressions.

## Operations

All the following operations can be applied only for main data types: integers, floats, and strings.

### Binary

Note that the right operand is trying to be convert to the type of the left operand if it is possible.

#### Arithmetic operations

|Name|Description
|--|--|
|+|If its operands are numbers, then adds them (with necessary conversion) returning the sum. If the left operand is string then concatenate the right operand string representation to it. If the right operand string and the left is not, raise an error|
|-|Can be applied only to numbers, returning their subtraction|
|*|Can be applied only to numbers, returning their multiplication|
|/|Can be applied only to numbers, returning their division|
|%|Can be applied only to numbers, returning their modulo division|

TO BE CONTINUED