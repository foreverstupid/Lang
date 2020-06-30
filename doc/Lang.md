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