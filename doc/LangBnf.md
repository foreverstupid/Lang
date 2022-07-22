## [BNF](https://en.wikipedia.org/wiki/Backus%E2%80%93Naur_form) of Lang

Here `this text style` is used for the grammar terms and literal symbols. Special symbols of the BNF are written in **this style**.

`<expression>` **::=** `<group>` **|** `<jump>` **|** **{** `<unar>` **}** `<operand>` `<tail>` **{** `<tie>` `<expression>` **}**

`<group>` **::=** `{` `<expression>` **{** `;` `<expression>` **}** `}`

`<jump>` **::=** `new` **|** `end` **|** `out` `<expression>`

`<tail>` **::=** `<indexator>` `<tail>` **|** `<args>` `<tail>` **|**

`<indexator>` **::=** `[` `<expression>` `]` **|** `.` `<identifier>`

`<args>` **::=** `(` **[** `<expression>` **{**`,` `<expression>` **}** **]** `)`

`<operand>` **::=** `(` `<expression>` `)` **|** `<if_expression>` **|** `<while_expression>` **|** `<literal>` **|** `<lambda>`

`<literal>` **::=** `<string>` **|** `<int>` **|** `<float>` **|** `<variable>` **|** `loc` `<variable>`

`<lambda>` **::=** `<params>` `=>` `<expression>`

`<params>` **::=** `[` **[** `<variable>` **{**`,` `<variable>` **}** **]** `]`

`<if_expression>` **::=** `if` `(` `<expression>` `)` `<expression>` **[** `or` `<expression>` **]**

`<while_expression>` **::=** `as` `(` `<expression>` `)` `<expression>`

`<tie>` **::=** **[** `<modifier>` **]** `<binar>`

`<modifier>` **::=** `!` **|** `=` **|** `->`

`<unar>` **::=** `-` **|** `!` **|** `$`

`<binar>` **::=** `+` **|** `-` **|** `*` **|** `/` **|** `%` **|** `>` **|** `<` **|** `~` **|** `&` **|** `|` **|** `=` **|** `->` **|** `:` **|** `?` **|** `in`

`<variable>` **::=** **[** `ref` **]** `<identifier>` **[** `<initializer>` **]**

`<initializer>` **::=** `{` `<init_atom>` **{**`,` `<init_atom>` **}** `}`

`<init_atom>` **::=** `<indexator>`**{ `<indexator>` }** `=` `<expression>` **|** `<expression>`

`<identifier>` **::=** `<letter>`**{**`<letter_or_digit>`**}**

`<int>` **::=** `<digit>`**{**`<digit_or_underscore>`**}**

`<float>` **::=** `<digit>`**{**`<digit_or_underscore>`**}**`.`**{**`<digit_or_underscore>`**}**

`<string>` **::=** see description [here](../Lang.md#string-literals)

`<digit>` **::=** `0`**..**`9`

`<letter>` **::=** `a`**..**`z` **|** `A`**..**`Z` **|** `_`

`<letter_or_digit>` **::=** `<letter>` **|** `<digit>`

`<digit_or_underscore>` **::=** `<digit>` **|** `_`