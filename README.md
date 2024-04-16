# Massie Equation Parser

This is an equation parser with support for configuring variables, functions, operators, etc.

## How to install

### From Nuget

Include the package `Scot.Massie.EquationParser`

### From Github

1. Go to https://github.com/c-massie/Massie-Equation-Parser/releases
2. Download the latest `.dll` version of the library
3. Right-click the project you want to add this to, go to `Add Reference...` in VS, `Add... > Add Reference...` in Rider.
4. Click the "Add from..."/"Browse" button in the window that opens, and navigate to the .dll file downloaded.

## How to use

Create a new EquationParser with:

```var parser = new EquationParser();```

When you instantiate an equation parser, you can call a number of fluent methods to specify how equations should be parsed. e.g. what functions are available to equations, what operators it can use, etc. - this is covered in "Types of equation" below.

Once you have an equation parser configured, you can pass in the string representation of an equation to the `.Parse(string)` method and it'll return an equation object that can then be evaluated.

```
var equation = parser.Parse("2 + 2");
var result = equation.Evaluate();
```

An equation can be evaluated multiple times - this will be faster than parsing the equation every time you want to get a result from the same equation. You may evaluate the same equation multiple times to get different results where the results of functions change, where functions have been re-implemented, or where variables have been re-assigned.

An equation parser can be configured with a pre-chosen set of functionality (functions, variables, operators) covering basic arithmetic by calling either `.WithStandardMaths()` or `.WithBasicMaths()`, where `.WithStandardMaths()` includes a more comprehensive set of functionality, and where `.WithBasicMaths()` includes a cut down version of the functionality available in the former. An equation parser can also be configured with a pre-chosen set of functionality covering logical operations with `.WithLogic()`, which can be used in conjunction with either `.WithStandardMaths()` or `.WithBasicMaths()`.

## Types of equation

In the following examples, numbers are literal values and letters represent variables.

### Literals

Example:

```5.4```

Literals in an equation are actual numeric values. These can be with or without the decimal point, and a negative number can be indicated be prefixing it with a "-", even if there is no "-" prefix operator specified.

### Variables

Example:

```x```

Variables are values stored against strings, which may be referred to in equations by those strings.

Variables can be declared on the equation parser with `.WithVariable(string variableName, double value)`, and may be re-assigned on the produced equation with `.SetVariable(string variableName, double value)`.

### Bracketed equations

Example:

```(x)```

Any other equation wrapped in brackets. These may be used to control the order of operations in an equation.

The bracket symbols by default are "(" and ")", but can set with `.WithBracketSymbols(string leftBracketSymbol, string rightBracketSymbol)`.

### Prefix operations

Example:

```-x```

Prefix operators are appended to the start of another equation.

Prefix operators can be declared on the equation parser with `.WithPrefixOperator(string symbol, operand => ...)`. Declaring a prefix operator then gives you access to methods for configuring it. i.e. associativity and precedence.

### Postfix operations

Example:

```x%```

Postfix operators are appended to the end of another equation.

Postfix operators can be declared on the equation parser with `.WithPostfixOperator(string symbol, operand => ...)`. Declaring a postfix operator then gives you access to methods for configuring it. i.e. associativity and precedence.

### Binary operations

Example:

```a + b```

Binary operators are placed between two other equations. Binary operators can be chained.

Binary operators can be declared on the equation parser with `.WithBinaryOperator(string symbol, (left, right) => ...)`. Declaring a binary operator then gives you access to methods for configuring it. i.e. associativity and precedence.

### Infix (n-ary) operations

Example:

```a ? b : c```

Infix operators have their symbols interleaved in order between other equations. Binary operators can be considered the smallest possible infix operator. Infix operators can be chained.

Infix operators can be declared on the equation parser with `.WithInfixOperator(IList<string> symbols, operandList => ...)`. Infix operators may be specified to have any number of operands, as implied by the number of symbols passed to them. Declaring an infix operator then gives you access to methods for configuring it. i.e. associativity and precedence.

### Bracketed operations

Example:

```⌊x⌋```

Bracketed operators are wrapped around an equation, or separator-delimited series of equations.

Bracketed operators can be declared on the equation parser with `.WithBracketedOperator(string opener, string closer, operand => ...)` or with `.WithBracketedMultiOperator(string opener, string closer, operandList => ...)`. Using the latter will result in a bracketed operator that can accept a separator-delimited series of operands and not just a single one. In the latter case, declaring a bracketed operator gives you access to methods for configuring it. i.e. whether it can accept 0 operators.

The separator is a comma (",") by default, but can be changed on the equation parser with `.WithArgumentSeparatorSymbol(string symbol)`.

### Function calls

Example:

```x(a, b, c)```

Functions provide an easy way of providing equations with access to values that may change depending on the values provided, or may change over time. Functions are called using the name of the function followed by a separator-delimited list of other equations contained in brackets. This list can also be empty.

Functions can be declared on the equation parsed with `.WithFunction(string functionName, argList => ...)`, and may be re-implemented on the produced equation with `.ReimplementFunction(string functionName, argList => ...)`.

The bracket symbols by default are "(" and ")", but can set with `.WithBracketSymbols(string leftBracketSymbol, string rightBracketSymbol)`.

The separator is a comma (",") by default, but can be changed on the equation parser with `.WithArgumentSeparatorSymbol(string symbol)`.

### Scientific notation

Example:

```4e5```

Numeric literals can be written in scientific notation, where the above would be equivalent to "4 * (10^5)". This only supports literal numbers for the mantissa and exponent.

### Juxtaposition

Example:

```ab```

Juxtaposition is the placement of two equations next to each other with nothing between. By default, this represents multiplication, but can represent any operation by providing an implementation.

Operation by juxtaposition is not enabled by default, and the equation parser must it enabled. It can be enabled through the methods `.WithJuxtaposition()` or `.WithJuxtaposition((left, right) => ...)`

Operations by juxtaposition are treated as a single unit within an equation, and as such, aren't split to account for operator precedence. e.g. `a / bc` will always parse the same as `a / (bc)`. Operations by juxtaposition is also currently right-associative.

## Other features

### Maximum depth

You can specify a maximum equation depth with `.WithMaximumDepth(int depth)`. This sets a maximum number of recursions down into an equation the equation parser will attempt to go before giving up. This is to allow you to protect against user-provided equations that are maliciously complex. If this is a concern, I also recommend limiting the size of the equation accepted. (e.g. to 256 characters)

## Why

This is a re-write of an earlier library element that was written to provide equation evaluation for configuration files in some Java projects, as well as make those values configurable by remote users in a way that doesn't involve running arbitrary code remotely. I've made a point to better structure this project, and to provide more functionality. e.g. The original version didn't have support for bracketed operators, operation by juxtaposition, or having characters be in both variable/function names, and operator symbols.
