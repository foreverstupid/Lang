# LANG

**Lang** is a Turing-full script programming language. The main concept of the program in Lang is an *expresion* - the statement that returns some value. The Lang program itself is expression.

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

**\<binar\>** ::= **+** | **-** | **\*** | **/** | **%** | **>** | **<** | **~** | **&** | **|** | **=** | **->** | **:**

**\<variable\>** ::= **\<letter\>**{ **\<letter_or_digit\>** }

**\<int\>** ::= **\<digit\>**{ **\<digit_or_underscore\>** }

**\<float\>** ::= **\<digit\>**{ **\<digit_or_underscore\>** }**.**{ **\<digit_or_underscore\>** }

**\<string\>** ::= **"**{**\<any_symbol\>**}**"**

**\<digit\>** ::= **0**..**9**

**\<letter\>** ::= **a**..**z** | **A**..**Z** | **_**

**\<letter_or_digit\>** ::= **\<letter\>** | **\<digit\>**

**\<digit_or_underscore\>** ::= **\<digit\>** | **_**

## Main concepts

### Data types

Lang has three main data types: integer number, float point number, and string. All number literals are non-negative (to get negative literal use negate operation). The fourth data type is a functional type that represents some lambda function.

#### Casting

The main data types (integer, float, and string) can be automatically casted to each other in some situations. Thus, in every binary operations the right operand will be casted to the type of the left operand. If such a transformation is not possible then the programm finishes with an error. Moreover there exists the special cast operator (**:**). Here is a cast table:

|Casting type|Integer|Float|String|
|--|--|--|--|
|Integer|Cast is not needed|Trivial cast|String representation of the number|
|Float|Round to integer|Cast is not needed|String representation og the number|
|String|Parse string as integer or error|Parse string as float or error|Cast is not needed|

#### Bool

There isn't such a type as bool in Lang. Nevertheless, integer, float, and string could be used as bool in conditions. Thus, 0, 0.0, and "" are used as **false**. All others values are used as **true**.

### Variables

Variables in Lang are created on their first assignment from some values. They can change their values and types. In a sense variables are just labels for data, to get the variable value a special dereference operation ($) is used. Thus, variables can be thought as pointers to data that should be dereferenced. Variables could have another variables as their values. Variables of a functional type could be evaluated as functions.

### Lambdas

All functions in Lang are lambdas. You can give them names by assignment to some variable. Lambda return value is the value of its body expression. Any lambda takes values (maybe none of them) as its input parameters, but as far as variable itself can be a value, you can pass a variable without dereferencing, that is similar to passing arguments by the reference in C++. Lambda parameters override any outer variables of the same name.

### Buil-in functions

Lang has several built-in named functions that shouldn't be described to be used. Their behaviour is the same as the behaviour of ordinary lambdas.

|Name|Arguments|Return value|
|--|--|--|
|_write|A single value to be write on the console of any type. If the value is variable then its name is used. If the variable is the None value, built-in, or a lambda then error occurs|Returns the printed value|
|_read|No arguments|Returns the string that is an input line from the console|
|_writeFile|A value to be writed into the given file and the file path. The value restriction is the same as for *_write*|The written value|
|_readFile|The file path|The full content of the file as a string value|
|_rnd|No arguments|A random float between 0.0 and 1.0|
|_length|A string|The length of the string|

### Expression

Expression is the main concept of Lang. It is a set of operations over data that returns a single value. There exists the following kinds of expressions:

1. **Usual expression**: operands linked by binary and unary operations (e.g. arithmetic operations, assignment, etc.)
2. **Expression group**: several expressions grouped together. The value of such an expression is the value of the last expression of the group.
3. **Lambda**: expression that can be evaluated only after substitution certain values as parameters.
4. **If-expression**: expression that evaluates or not according to some condition. Can have else-part (*if-else-expression*) that is evaluated if the consition is false. If has only if-part (*if-only-expression*) and the condition is false, then returns the special *None* value that cannot be used in any expressions.
5. **Cycle-expression**: expression that evaluates several times while some consition is true. Its value is a value of its last iteration. If none iteration is performed, returns the special *None* value that cannot be used in any expressions.

## Operations

### Binary operations

Note that the right operand is trying to be convert to the type of the left operand if it is possible.

#### Arithmetic operations

All the following operations can be applied only for main data types: integers, floats, and strings. Note that before applying right operand will be casted to the type of the left one. If cast is not possible the error will occure.

|Name|Description
|--|--|
|+|Adds numbers or concatenates strings|
|-|Can be applied only to numbers, returning their subtraction|
|*|Applying to numbers, returns their multiplication. If the left operand is string and the right one is a non-negative integer *n* then returns a string that is concatenation of *n* copies of the given string. All other operand types are not permited.|
|/|Can be applied only to numbers, returning their division|
|%|Can be applied only to numbers, returning their modulo division|
|:|Casts the left operand to the type of the right one (see [casting](####Casting))|

#### Comparision operations

|Name|Description|
|--|--|
|~|Equality operation. Can be applied to any data type. Returns 1 if the operands are of the same type and equals and 0 otherwise|
|>|Can be applied only to the main data types. Checks whether the left number is less than the right one (with the necessary casting), or whether the left string is lexicographically less then the right one|
|<|Can be applied only to the main data types. Checks whether the left number is greater than the right one (with the necessary casting), or whether the left string is lexicographically greater then the right one|

#### Bool operations

**&** and **|** stand for AND and OR respectively. Their operands are used as bool (see [bool](####Bool)).

#### Assignments

There are two assignments operations in Lang: *=* (left assignment) and *->* (right assignment). The first one assigns the right value to the left operand returning the assigned value, the second one assigns the left value to the right opeand, returning the assignable right operand. Assignable operands of assignmebts should be variables, otherwise an error ossurs. Left assignment is right associative, the right assignment is left associative, i.e. the following expressions are equivalent:
```
a = b = c = 3;  # is equivalent to
a = (b = (c = 3));

5 -> a -> b; # is equivalent to
(5 -> a) -> b;  # note that here the last assignment assigns a variable 'a' to the variable 'b'
```

If the variable doesn't exist then assignments create it.

### Unary operations

|Name|Description|
|--|--|
|-|Negation of number. Cannot be applied to not a number|
|!|Logical NOT|
|$|Returns the value of the variable. Cannot be applied to non-variable value|

### Indexator

Indexator can be applied to any variable or a string value. Applying to the variable indexator returns a new variable that represents an indexed variable (that can not exist yet). Applying to a string value indexator returns a one-character string that contains the character in the string at the certain position.

Note that the type of the index matters. Thus the following indexed variables are different:
```
a[0];
a[0.0];
a["0"];
```

## Other features

1. Lang has one-line comments only. Every comment starts with the *#* character and ends when the line ends.
2. The None value that is the result of non-performed actions (e.g. the cycle that hasn't done any iterations) cannot be used in any expressions. So, expressions that use if-only-expressions or cycles as operands are not suggested to be use.
3. All variables except lambda parameters are global.