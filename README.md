# LANG

**Lang** is a Turing-full script programming language. The main concept of the program in Lang is an *expresion* - the statement that returns some value. A Lang program itself is an expression. See [language BNF](doc/LangBnf.md) for more information about programs structure. Program examples can be found [here](examples).

# Main concepts

## Data types

Lang has three the main data types: integer number, float point number, and string. All number literals are non-negative (to get a negative literal use the negate operation). The fourth data type is a functional type that represents some lambda function. Variables themselves can be thought as a kind of a data type too, because they can be assigned to another variables or even can be used in some operations. 

### Casting

The main data types (integer, float, and string) can be automatically casted to each other in some situations. Thus, in every binary operation the right operand will be casted to the type of the left one. If such a transformation is not possible then the program finishes with an error. Moreover, there exists the special cast operator (`:`). To check whether the cast is possible or not you can use another special cast-checking operator `?`. Here is the cast table:

|Casting type|Integer|Float|String|
|--|--|--|--|
|Integer|Cast is not needed|Trivial cast|String representation of the number|
|Float|Round to integer|Cast is not needed|String representation of the number|
|String|Parse string as integer or error|Parse string as float or error|Cast is not needed|

Note, that the output value of the whole program should be a value that can be casted to a string. Otherwise the program will stop with an error.

### Bool

There isn't such a type as bool in Lang. Nevertheless, integer, float, string (and [variables](#variables)) could be used as _bool-like_ types in conditions. Thus, `0`, `0.0`, and `""` are used as **false**. All others values are used as **true**.

## Variables

Variables in Lang are created on their first assignment from some values. They can change their values and types during their lifetime. In a sense variables are just labels for data, to get the value of a variable a special dereference operation (`$`) is used. So, variables can be thought as pointers to data, that should be dereferenced. Variables can have another variables as their values. Variables of a functional type could be evaluated as functions. Variables also can be used as bool-like values. Namely a variable is **true** if it exists and **false** otherwise. Using this feature you can check variable existence in runtime.

## Lambdas

All functions in Lang are lambdas. You can give them names by assignment to some variable. Lambda's return value is the value of its body expression. Any lambda takes values (maybe none of them) as its input parameters. As far as a variable itself can be a value, you can pass a variable without dereferencing as lambda argument. That is similar to passing arguments by the reference in C++. Lambda parameters hide any outer variables of the same name in the outer scope (see more in the [variable visibility](#variable-visibility) section).

## Built-in functions

Lang has built-in library of functions that needn't be described to be used. For example, there are console IO built-in functions `sys.read` and `sys.write`. All built-in functions are described [here](#built-ins-library).

## Expression

Expression is the main concept of Lang. It is a set of operations over data that returns a single value. There exist the following types of expressions:

1. **Usual expression**: operands linked by binary and unary operations (e.g. arithmetic operations, assignment, etc.)
2. **Expression group**: several expressions grouped together. The value of such an expression is the value of the last expression of the group.
    ```
    {
        <expression_1>;
        <expression_2>;
        ...
        <expression_last>
    }
    ```
3. **If-expression**: expression that evaluates or not according to a condition. Can have else-part (*if-else-expression*) that is evaluated if the condition is **false**. When the operation has only if-part (*if-only-expression*) and the condition is **false**, then it returns the special `None` value that cannot be used in any operation.
    ```
    if (<condition>)
        <if expression>
    or
        <else expression>
    ```
4. **Cycle-expression**: expression that evaluates several times, while a condition is **true**. Its value is a value of its last iteration. If none iteration is performed, then it returns the special `None` value that cannot be used in any operation.
    ```
    as (<condition>) <expression>
    ```
    4.1 **Cycle jumps**: two special operations that help to stop early a cycle (`end`) or its iteration (`new`). These are only _nullary_ operations in Lang (they do not take parameters). They always return **true**. Using thees operations outside of any cycle is forbidden and leads to a runtime error.
5. **Lambda**: expression that can be evaluated only after substitution certain values as parameters.
    ```
    [<p1>, <p2>, ... ] => <expression depending on <p1>,<p2>,...>
    ```
    5.1. **Lambda break**: special operation (`out`) that early breaks a lambda evaluation. It is an unary operation that takes the out value of the lambda.
6. **Function evaluation**: the process of evaluating a lambda or a lambda-valued variable. It takes given expressions and substitutes them as the lambda parameters. Note, that you don't have to (but if you want, you can) use dereferencing to evaluate lambda that is a value of the variable.
    ```
    <lambda or variable>(<expression1>, <expression2>, ...)
    ```

## String literals

String literals are collection of characters between quotes. They can contain the following escape characters:
- `\n` - new line
- `\t` - tabulation
- `\x` - interpretating the following two-digit hexadecimal number as the code of a character (e.g. `"\x1b"` is a string containing ESC symbol).
- `\` - allows to insert any symbol into the string. E.g. to insert quotes without breaking the literal you can use `\"`. To insert backslash itself you have to double it (`\\`).

Lang supports string interpolation. That means that you can insert any expression into a string literal via a specail syntaxis. This syntaxis substitutes a string representation of the expression result into the corresponding string literal part. Interpolating expression should be inserted between curly brackets. For example,
```
a = "interpolation";
b = 2;
sys.write("This is {$a}. This is {$a + $b}");
```
The given code will write the following text:
```
This is interpolation. This is interpolation2.
```
So, everything inside a string literal that is between curly brackets is interpreted as an expression whose value's string representation should be substituted.

### Raw strings

It is very useful to be able put into a string all characters as themselves without escaping special symbols like `"` etc. Moreover, sometimes characters `{}`, that delimit interpolation inside a string, are parts of the literal itself, so you have to escape them every time (e.g., when you construct JSON). And lastly often you want to format indenting of the multiline string literal in terms of your code, but that leads to a vast ammount of extra spaces in the literal itself. For example, in the following code
```
{
    if ($condition)
    {
        str = "
            [
                1, 2, 3
            ]";
    }
}
```
you want to see, that the value of `str` is
```
[
    1, 2, 3
]
```
but you also don't want to shift its definition to the beginning of the left side of the screen. For solving all these issues Lang uses *raw strings*.

Such string literals start and end with a symbol `` ` ``(backtick). The first two characters of such a string literal are called *raw string preambula*. The first one defines the interpolation start delimiter, the second one defines the interpolation end delimiter. Note, that the preambula cannot contain symbols `` ` `` and `#` (backtick and hash). All symbols inside a raw string are used as themselves, except the notation `\x00` for hexadecimal characters codes and the combination `` \` `` for inserting backtick itself as a character. Moreover, Lang analyses the beginning position of the raw string and cuts all extra spaces on the following lines precending this position. Note that this behavior only works for spaces, if a line contains any other character, then space dropping is halted for this line (but not for any other ones). Here is an example of raw string usage:
```
{
    age = 42;
    name = "John";

    json =
    # use brackets as interpolation delimiters
    `()
    {
        "age": ($age),
        "name": "($name)",
        "id": "($name + $age)"
    }`;
    _write($json)
}
```
The code above will print exactly:
```
{
    "age": 42,
    "name": "John",
    "id": "John42"
}
```
Note that symbols in preambula can coinside, but cannot be omitted:
```
var = 42;
str = `**var has value *$var*`;         # you can use same symbols
notChanged = `{}var has value {$var}`;  # you can use curly brackets if you want
invalid = `var has vlaue {$var}`;       # but cannot omit preambula or its part
```

# Operations

## Unary operations

|Name|Description|
|--|--|
|-|Negation of a number. Cannot be applied to not a number|
|!|Logical NOT. Can be applied only to a bool-like value|
|$|Dereferencing. Returns the value of the variable. Cannot be applied to a non-variable value|

## Binary operations

Note that before applying the right operand will be implicitly casted to the type of the left one (if it is neccessary). If the cast is not possible the error will occure.

### Arithmetic operations

All the following operations can be applied only to the main data types: integers, floats, and strings.

|Name|Description
|--|--|
|+|Adds numbers or concatenates strings|
|-|Can be applied only to numbers, returning their subtraction|
|\*|Applying to numbers, returns their multiplication. If the left operand is string and the right one is a non-negative integer *n* then returns a string that is a concatenation of *n* copies of the given string. All other operand types are not permited.|
|/|Can be applied only to numbers, returning their division|
|%|Can be applied only to numbers, returning their modulo division|
|>>|If the operands are integers, then performs bitwise right shift. If the left operand is a string and the right one is an integer, then then returns a string which characters are shifted in codes by the given number (e.g., `"AA" >> 33` is `"bb"`, because the code of the symbol 'A' is 65 and the code of the symbol 'b' is 98). All other operand types are not permited.|
|<<|Same as `>>` but for opposite direction|

### Comparision operations

|Name|Description|
|--|--|
|~|Equality operation. Can be applied to any data type. Returns **true** if the operands are of the same type and equal and **false** otherwise|
|>|Can be applied only to the main data types. Checks whether the left number is less than the right one or whether the left string is lexicographically less than the right one|
|<|Same as `>` but for opposite direction|

### Bool operations

`&` and `|` stand for AND and OR respectively. Their operands are used as bool-like values (see [bool](#bool)).

### Cast operations

|Name|Description
|--|--|
|:|Casts the left operand to the type of the right one (see [casting](#casting))|
|?|Returns bool-like value that determines whether the left operand can be casted to the type of the right one (see [casting](#casting)). Can be applied to any types|

### Assignments

There are two types of assignment operations in Lang: `=` (left assignment) and `->` (right assignment). The first one assigns the right value to the left operand returning the assigned value. The second one assigns the left value to the right opeand, returning the assignee. Assignable operands of assignments should be variables, otherwise an error ossurs. Left assignment is right associative, the right assignment is left associative, i.e., the following expressions are equivalent:
```
a = b = c = 3;  # is equivalent to
a = (b = (c = 3));

5 -> a -> b;    # is equivalent to
(5 -> a) -> b;
# note that here the last assignment
# assigns a variable 'a' to the variable 'b',
# so $b is 'a' itself
```

If the variable doesn't exist then assignments create it.

Often, you want to update a value of a variable using the previous value of it. For example, if you write a kind of enumeration, your code will probably look like the following:
```
i = 0;
length = 100;
as ($i < $length)
{
    _write($i);
    i = $i + 1;     # here we add 1 to the variable
}
```
It is not very convinient to use the variable and its dereferencing in the same operation for such a simple action. That's why Lang provides a concept of _complex assignments_. You can put any binary operation right before the assignment operation to say, that you want to save the result of this operation over the value of the variable and the second operand into the variable. Thus, the code
```
i = $i + 1;
```
becomes
```
# this same as: i = $i + 1
i += 1;     # this returns a new value of 'i'
```
or
```
# this same as: 1 + $i -> i
1 +-> i;    # this returns 'i' itself
```
Note, that as far as complex assignments use the variable's value, you have to be sure that the variable exists before using these operations. Moreover, notice the order of operands for `=` and for `->`, that can matter for non-commutative operations.

## Indexator

Indexator can be applied to any variable or a string value. Applying to the variable indexator returns a new variable that represents an indexed variable (that can not exist yet). Applying to a string value indexator returns an one-character string that contains the character in the string at the certain position. Note, that indexing has more priority than dereferencing, so the following code is invalid:
```
str = "abcd";
$str[0];    # firstly we try to get "str[0]" variable (that doesn't exist) and then take its value
```
The proper version is:
```
str = "abcd";
($str)[0];
```

The type of the index matters. Thus, the following indexed variables are different:
```
a[0];
a[0.0];
a["0"];
```
Indexing by floats is not suggested (due to rounding error).

### Pseudo-fields

The way, the Lang releases indexing, makes arrays be similar to dictionaries. Moreover, Lang supports some kind of a syntaxic sugar that is called **pseudo-fields**. A special operator **.** (dot) can be used instead of indexing by a string. For example, the following code lines are equivalent:
```
array["length"];
array.length;
```
Note, that pseudo-field names should contain only alphanumeric characters (letters, digits, and underscores), while string indexing allows key to include arbitrary characters.

### String length

Lang also uses indexing-like behavior to provide string values' length determining. Namely, each string value has a "pseudo-field" called `length`, that contains the count of the string characters. You can use it like the following:
```
# getting length of a string
str1Len = "This is a string".length;

# as any pseudo-field 'length' is just a string index
str2Len = "Another string"["length"];

# do not forget, that 'length' is a pseudo-field of a string value,
# so, if you want to get the length of a string-valued variable,
# you should dereference it firstly, but as far as dereferencing
# has less priority than indexing, you have to use brackets
str = "Text";
str3Len = ($str).length;
```

## Operation `in`

There is a special binary operation `in` that performs a search of the given value in the given array or a given substring in a given string.

### String case

If the right operand of `in` is a string, then it checks whether this operand contains the left operand as a substring. Note, that in this case the left operand nust be a string (implicit conversion is not performed). Here is an example:
```
if ("ab" in "abracadabra")
    sys.write("Yes!");      # this code will run

if ("englishman" in "New York")
    sys.write("alien");     # this code will not run

if (123 in "12345")         # this code will fail at runtime
    sys.write("Impossible!");
```

### Array case

If the right operand of `in` is an array (or more precisely a variable), then it tries to find an element of the array that contains the given value. In the case of success it returns the first found element with the given value. Note that thus the result of the operation is itself a variable, so you have to dereference it if you want to get its value. On the other hand, it allows you to perform the element value changing if it is necessary. If the value was not found, then `in` returns bool-like **false**. Note, that as far as a variable can be casted to bool, it is absolutely safe to use `in` operation as condition. Here is an example:
```
arr[0] = 12;
arr.size = "small";

if (43 in arr)  # will return false
    sys.write("arr contains 43);   # this code will not run

if (12 in arr)  # here we use the result of 'in' (a variable) as true
    sys.write("arr contains 'small' string");  # this code will run

("small" in arr) = "big";   # we use returning variable to set new value
if ("big" in arr)
    sys.write("now it's big");  # this code will run
```

## Bool-like operation reversing

In many cases you want to reverse bool-like result of a binary operation. For example, if you want to check, that a string is not empty, you will write something like this: `if (!($str ~ "")) ...`. But as you can see it leads to usage of extra pair of brackets. That is not convenient. So, Lang has a syntax sugar for this case. You can put `!` operation right before bool-like returning binary operation to reverse its result. E.g.:
```
if ($str !~ "")         # string is not empty
    ...
if ($number !< 12)      # number is not less than 12
    ...
if ($token !? 0)        # token cannot be casted to integer
    ...
if (42 !in array)       # array doesn't contain 42
    ...
```

## Initializer

Quite often you want to set multiple pseudo-fields of the same variable or initialize an array with the given collection of elements. Then you have to write something like this:
```
arr[0] = "first element";
arr[1] = "second element";
arr[2] = "third element";
...
```
or this
```
object.field1 = "value1";
object.field2 = "value2";
object.field3 = "value3";
...
```
It is not very convenient. And it gets worse when your array has a pretty long name or you want to initialize a lot of subfields of a deep-hierarchy field of a variable (e.g. `object.subobject.another.also.object`). That's why Lang provides a more simple way for initilaization arrays or dictionaries: *initializers*. This is a syntaxic sugar that allows you to write initialization expressions for multiple fields (or items) of a single variable.

Initializer follows a variable and is bounded by curly brackets. It contains at least one initializer item that can be indexator or simple-array initialization. Initializer items are separated by commas. Let's see an example:
```
objectWithQuiteLongName {
    .length = 10 * 23,
    .name.short = "Some name",
    [10] = 0,
    ["indexing property"].float = 13.0
};
```
This code is completely equivalent to:
```
objectWithQuiteLongName.length = 10 * 23;
objectWithQuiteLongName.name.short = "Some name";
objectWithQuiteLongName[10] = 0;
objectWithQuiteLongName["indexing property"].float = 13.0;
```
Simple-array initializer items stand for initialization of arrays. Their indecies are created automatically starting from `0` with incrementing. E.g. the following code
```
arr { 0, 10, 100 + 34, "abra" + "cadabra" };
```
is completely equivalent to
```
arr[0] = 0;
arr[1] = 10;
arr[2] = 100 + 34;
arr[3] = "abra" + "cadabra";
```
You can mix initializer items of all types together, but note, that for simple-array initializer items indecies only count of simple-array items matters. That means that the code:
```
obj.inner.inner.and.even.inner {
    .size.integer["simple"] = 11,
    "Hello",
    .name = "Name",
    [10][0] = 1,
    "world"
};
```
is completely equivalent to the following one:
```
obj.inner.inner.and.even.inner.size.integer["simple"] = 11;
obj.inner.inner.and.even.inner[0] = "Hello";       # first simple-array element
obj.inner.inner.and.even.inner.name = "Name";
obj.inner.inner.and.even.inner[10][0] = 1;
obj.inner.inner.and.even.inner[1] = "world";       # second simple-array element
```

# Built-ins library

All built-in functions are pseudo-fields of predefined dictionary called `sys`. Thus, these functions behave as ordinary variables with functional value type. You can even reassign them, changing their behavior, or assign their values to another variables.

## `sys.`
|Name|Description|Arguments|Returns|
|--|--|--|--|
|sys.write|Writes the given value to the console|String or number|Written value|
|sys.read|Reads a line from the console|No arguments|The string that is an input line from the console|
|sys.readKey|Reads a key from the console|Bool-like value that determines whether the pressed key character should be displayed or not|A key that was pressed by a user as a one-character string|
|sys.rnd|Generates a random float number|No arguments|A random float between 0.0 and 1.0|
|sys.alloc|Creates a new [dynamic variable](#dynamic-variables)|No arguments|Created dynamic variable|
|sys.free|Disposes an existing [dynamic variable](#dynamic-variables)|A dynamic variable that should be disposed|Bool-like **true** value|
|sys.sleep|Stops the program evaluating for a given amount of time|A number (float or integer) representing the delay in milliseconds|Bool-like **true** value|
|sys.exec|Executes the given program|The program path as a string, execution command-line arguments as a string, a variable that will be used as an output parameter for the program execution's output, and a variable that will be used as an output parameter for the program execution's error output|The program's exit-code|

Example of `sys.exec` built-in function usage:
```
args = "ls -l";

# we pass out and err variables as themselves, not their values,
# moreover, if these variables do not exist, then they will be created
exitCode = sys.exec("/bin/bash", $args, out, err);

sys.write($out);     # here we use the value of the out variable
sys.write($err);     # here we use the value of the err variable
```

## `sys.file.`
|Name|Description|Arguments|Returns|
|--|--|--|--|
|sys.file.write|Appends the given value into the file|String or number for appending and the file path as a string|The written value. If the file doesn't exist then this function creates it|
|sys.file.read|Reads the content of the file|The file path as a string|The full content of the file as a string value|
|sys.file.exists|Checks whether the given file exists|The file path as a string|Bool-like **true** value if the file exists and bool-like **false** value otherwise|
|sys.file.delete|Deletes the given file|The file path as a string|Bool-like **true** value if file existed and bool-like **false** value otherwise|

## `sys.math.`

All these functions take a number (integer or float) as an input and return a float number as an output.

|Name|Description|
|--|--|
|sys.math.sqrt|Square root function|
|sys.math.ln|Natural logarithm|
|sys.math.exp|Exponent function (`e` to the power of the input)|
|sys.math.sin|Sine function|
|sys.math.cos|Cosine function|
|sys.math.tan|Tangent function|

# Variable visibility

By default all variables are defined in the global scope, i.e., they are visible and can be used in every lambda (doesn't matter from outer or from inner one) or expression. It can lead to collisions. So, the special keyword **loc** is introduced for preventing such problems. All variables that are defined with this keyword are visible only from the inner lambdas.
Let's consider the following program:
```
{
    func = [] =>
    {
        a = 1;
        loc b = 2;

        f = [] =>
        {
            ...
        }
    };

    b = 100
}
```
Here the variable `a` is a global one. It can be used in functions `func`, `f`, or even in the outer program block. The variable `b` is a local one. It can be used only in functions `func`, `f` and all their inner lambdas. The expression `b = 100` will create another variable without changing the local variable `b` of the function `func`. Thus, the following code is invalid:
```
{
    func = [] => loc a = "abracadabra";
    sys.write($a)
}
```
It will raise an error because the value of the variable `a` cannot be gotten as far as it is a local variable of the function `func`, and the global variable `a` doesn't exist.

Note, that **loc** keyword should be used only once at the variable definition (i.e., the first assignment). Nevertheless, you can define local variable without assignment. For example, the following code is valid:
```
loc a;
a = 10;
```
Anyway, you cannot use the variable before the first assignment, because local definition doesn't create the variable, but just marks its name as the local one. Such a kind of a hack can be used for simplifying localisation of a complex variables (arrays or dictionaries). E.g., the following code is correct:
```
loc a;
a.length = 3;
a[0] = "a";
a[1] = "b";
a[2] = "c";
```
Here every variable that relates to the `a` name is local.

By the way, all lambda parameters can be thought as the local variables.

# Reference parameters and variables

As it was said previously, you can pass the variable itself as a parameter to a lambda. In this case you have to dereference such a parameter before performing any operation over it. That can be annoying. That's why a special syntaxic construction is introduced. If you want to mark some parameter as a reference one and then not to dereference it every time, you can define it using a keyword `ref`, e.g., `ref parameter`. Thus, the following versions of the code are equivalent:
```
# First version
func = [a] =>
{
    ($a).length = 2;
    $($a).length
};

array.length = 0;
func(array);
```

```
# Second version
func = [ref a] =>
{
    a.length = 2;
    $a.length
};

array.length = 0;
func(array);
```

You can also declare a usual variable (not a lambda parameter) as a reference one. This should be done only once and only on the first variable usage. If the variable is also the local one, then the keyword `ref` should follow the keyword `loc`. This scenario is usefull when the lambda returns a dictionary. Then you can assign its returning value to a reference variable and avoid a vast ammount of dereferencing during usage such a return value. Compare the following equivalent code parts:
```
# First version
fun = [] =>
{
    loc result;
    result.num = 10;
    result.str = "ten";
    result                  # thus we return a dictionary
};

loc res = fun();
sys.write($($res).num);     # first dereference to get a variable, then get a field, and
sys.write($($res).str);     # then dereference to get a field value
```
```
# Second version
fun = [] =>
{
    loc result;
    result.num = 10;
    result.str = "ten";
    result                  # thus we return a dictionary
};

loc ref res = fun();        # declare reference variable
sys.write($res.num);        # no additional dereferencing anymore
sys.write($res.str);

```

# Dynamic variables

Let's see the following example of a function that splits a given string by spaces, returning the result in a form of a dictionary:
```
split = [str] =>
{
    loc result;

    loc wordCount = 0;
    loc word = "";
    loc len = sys.length($str);

    loc handleWord = [] =>
        if ($word)
        {
            result.words[$wordCount] = $word;
            word = "";
            wordCount = $wordCount + 1
        };

    loc i = -1;
    as ((i = $i + 1) < $len)
    {
        if (($str)[$i] ~ " ")
            handleWord()
        or
            word = $word + ($str)[$i]
    };

    handleWord();                # check last word
    result.count = $wordCount;
    result
};
```
Now suppose that we want to compare two lines word by word (spaces don't matter). We can do this via the following function that uses `split`:
```
compareByWords = [line1, line2] =>
{
    ref res1 = split($line1);
    ref res2 = split($line2);

    if ($res1.count ~ $res2.count)
    {
        i = -1;
        eq = 1;
        as (((i = $i + 1) < $res1.count) & $eq)
        {
            eq = $res1.words[$i] ~ $res2.words[$i]
        };

        $eq
    }
    or
    {
        0
    }
}
```
So, the above function is supposed to return `1` if lines are word-wise equivalent and `0` otherwise. But in fact the function will always return `1`. The problem is that in Lang all usual variables are _static_. It means that once they are created they are binded to some lambda (or global) context and live forever. Thus, when `split` is firstly invoked it creates local `result` variable. Then this variable is filled with the parsing information and returned to the outer code. But when `split` is called the second time, it fills the same static variable `result` with a new information. So, inside `compareByWords` variables `res1` and `res2` label the same portion of information (namely the variable `result` of the function `split`). That's why by comparision these variables will always contain the information about the second line.

To avoid such a collision there exist _dynamic_ variables. Such variables are created via the special built-in fucntion `sys.alloc`. It has no input parameters and returns a dynamic variable that by default has an integer value `0`. An example of usage:
```
ref var = sys.alloc();
var = 2;
sys.write($var);
```
Reference variables allow you to omit an additional dereferencing when using dynamic variable that they are labeled. So, this is a suggested way of dynamic variables usage, but you can also use them like this:
```
var = sys.alloc();
$var = 2;           # the value of "var" is a dynamic variable itself
sys.write($$var);   # the 1-st $ gets the variable and the 2-nd $ gets its value
```

As dynamic variables are created in runtime their count is potentially infinite. To prevent memory loss you should deallocate dynamic variables once they are not using anymore. For this purpose there exists another built-in function `sys.free`. It takes a dynamic variable as an input parameter, returning `None`. Note, that usage of the deallocated dynamic variable leads to an error:
```
ref var = sys.alloc();
sys.free(var);
var = 2;        # RUNTIME ERROR
```
Perfectly, all created dynamic variables should be deallocated by the end of the program. But it's at your discretion.

# Other features

1. Lang has one-line comments only. Every comment starts with the *#* character and ends when the line ends.
2. The `None` value that is the result of non-performed actions (e.g. the cycle that hasn't done any iterations) cannot be used in any expressions. So, expressions that use if-only-expressions or cycles as operands are not suggested to be use.
3. Semicolon can be thought as value ignoring unary postfix operation, that just flushes its operand from the stack.
4. Be careful with recursion. Lang supports it, but as far as all usual variables are static, you should use tail recursion only. E.g. the following factorial realisation is incorrect:
   ```
   factorial = [n] =>
       if ($n < 3)
           $n
       or
           factorial($n - 1) * $n;
   ```
   As the recursion reaсhes its basis (the if-part) and starts to evaluating backward, all the instances of `factorial` will have the variable `n` equal to `2` (that is the recursion basis). So, in all else-parts we will multiply returning value by `2` and instead of `n` factorial we will get `2` to the power of `n - 1`. The proper realisation is
   ```
   factorial = [n] =>
       if ($n < 3)
           $n
       or
           # tail recursion allows us extracting the value of "n" before the follwoing instance of
           # "factorial" changes it, so everything is correct
           $n * factorial($n - 1);
   ```

# The author isn't responsible for your mental health that can be damaged during reading this code
