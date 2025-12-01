( Target ASSEMBLER)
ASSEMBLER DEFINITIONS
: OP  ( op)   CREATE FORTH C,  DOES> C@ ( op)   HOST C, ;
: OP# ( op)   CREATE FORTH C,  DOES> C@ ( n op) HOST C, , ;
: OP, ( n op)   + C, ; ( not OR!)

00 OP  NEXT
07 OP# #
08 OP  BIOS
09 OP# CALL
0A OP# JMP
0B OP  RET
0D OP  EXECUTE

: LD ( reg -)   ?DUP IF 10 OP, ELSE 10 C, , THEN ;
: ST ( reg -)   ?DUP IF 18 OP, ELSE 18 C, , THEN ;

: BEGIN ( - a)          HERE ;
: UNTIL ( a cond -)     20 OP,  HERE - C, ;
: AGAIN ( a -)          -14 UNTIL ; ( 0C = unconditional branch )
: IF ( cond - a)        20 OP,  HERE 0 C, ;
: THEN ( a -)           HERE OVER -  SWAP TC! ;
: ELSE ( a - a2)        -14 IF  SWAP THEN ;
: WHILE ( a - a2 a)     IF SWAP ;
: REPEAT ( a2 a -)      AGAIN THEN ;
: NOT ( cond - !cond)   8 XOR ;

0 CONSTANT T    1 CONSTANT S    2 CONSTANT R    3 CONSTANT P
4 CONSTANT I    5 CONSTANT W

0 CONSTANT 0=   1 CONSTANT 0<   2 CONSTANT 0>   3 CONSTANT =
4 CONSTANT <    5 CONSTANT >    6 CONSTANT U<   7 CONSTANT U>

30 OP DUP       31 OP DROP      32 OP SWAP      33 OP OVER
34 OP ROT       35 OP NIP       36 OP ?DUP      37 OP PICK
38 OP >R        39 OP R>        3A OP R@

40 OP 2DUP      41 OP 2DROP     42 OP 2SWAP     43 OP 2OVER
44 OP 2>R       45 OP 2R>       46 OP 2R@

50 OP +         51 OP -         52 OP *         53 OP /
54 OP MOD       55 OP AND       56 OP OR        57 OP XOR
58 OP LSHIFT    59 OP RSHIFT    5A OP ARSHIFT   5B OP INVERT
5C OP NEGATE


HOST DEFINITIONS


0 [IF]
( Assembler test code)
: CODE,  HERE 2+ , ;
: PATCH ( a -)   9 T! ; \ patch "10 JMP" at offset 8
: DIE  FF C, ; ( invalid opcode)

ASSEMBLER
( init stack and jump to test )
0 ORG   FF00 T LD  R ST  FE00 T LD  S ST  10 JMP

( print OK )
0D ORG   S" OK" S,  0A C,
10 ORG   0D #  3 #  1 # BIOS  0 # BIOS ( BYE)

( subroutine that prints)
20 ORG   30 #  7 #  1 #  BIOS  RET
30 ORG   S" Subway" S,  0A C,

( call/ret)
40 PATCH
40 ORG   20 CALL  DIE

( loop )
50 PATCH
50 ORG   5 #  BEGIN  20 CALL  1 # -  0= UNTIL  DROP  DIE

60 PATCH ( print numbers )
60 ORG   1234 #  3 # BIOS  10 JMP

( threading )
100 ORG   3 , ( DOCOL )
108 ORG   CODE,  5 , ( EXIT )
110 ORG   CODE,  3 # BIOS  NEXT ( H.)
120 ORG   CODE,  0 # BIOS ( BYE)
128 ORG   CODE,  DIE
130 ORG   100 ,  110 , 108 , ( H.)
140 ORG   100 ,  130 , 120 , ( H. BYE)

150 PATCH
150 ORG   142 ( pfa) T LD  I ST  DUP  NEXT

HOST
SAVE .( Saved )

[THEN]
