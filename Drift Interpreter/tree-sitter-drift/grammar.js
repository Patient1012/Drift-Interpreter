module.exports = grammar({
    name: "drift",

    extras: $ => [
        /\s/,
        $.comment
    ],

    rules: {
        source_file: $ => repeat($._statement),

        
_statement: $ => choice(
  $.variable_declaration,
  $.function_declaration,
  $.return_statement,
  $.if_statement,
  $.while_statement,
  $.assignment,
  $.call_statement
),

variable_declaration: $ =>
  seq(
    choice("var", "const"),
    $.type,
    $.identifier,
    "=",
    $._expression,
    ";"
  ),

function_declaration: $ =>
  seq(
    $.type,
    $.identifier,
    "(",
    optional($.parameter_list),
    ")",
    "{",
    repeat($._statement),
    "}"
  ),

parameter_list: $ =>
  seq(
    $.parameter,
    repeat(seq(",", $.parameter))
  ),

parameter: $ =>
  seq($.type, $.identifier),

return_statement: $ =>
  seq(
    "return",
    optional($._expression),
    ";"
  ),

if_statement: $ =>
  seq(
    "if",
    "(",
    $._expression,
    ")",
    "{",
    repeat($._statement),
    "}",
    repeat($.elif_clause),
    optional($.else_clause)
  ),

elif_clause: $ =>
  seq(
    "elif",
    "(",
    $._expression,
    ")",
    "{",
    repeat($._statement),
    "}"
  ),

else_clause: $ =>
  seq(
    "else",
    "{",
    repeat($._statement),
    "}"
  ),

while_statement: $ =>
  seq(
    "while",
    "(",
    $._expression,
    ")",
    "{",
    repeat($._statement),
    "}"
  ),

assignment: $ =>
  seq(
    $.identifier,
    "=",
    $._expression,
    ";"
  ),

call_statement: $ =>
  seq(
    $.call_expression,
    ";"
  ),

call_expression: $ =>
  seq(
    $.identifier,
    "(",
    optional($.argument_list),
    ")"
  ),

argument_list: $ =>
  seq(
    $._expression,
    repeat(seq(",", $._expression))
  ),

_expression: $ =>
  choice(
    $.call_expression,
    $.identifier,
    $.number,
    $.string,
    $.binary_expression
  ),

binary_expression: $ =>
  prec.left(seq(
    $._expression,
    choice(
      "+",
      "-",
      "*",
      "/",
      "<",
      ">",
      "<=",
      ">=",
      "==",
      "!="
    ),
    $._expression
  )),

identifier: _ => /[a-zA-Z_][a-zA-Z0-9_]*/,

number: _ => /\d+/,

string: _ => /"[^"]*"/,

comment: _ => token(seq("//", /.*/)),

type: _ => choice(
  "void",
  "bool",
  "str",
  "i8",
  "i16",
  "i32",
  "i64",
  "f16",
  "f32",
  "f64"
)


}
})
;
