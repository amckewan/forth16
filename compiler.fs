\ Cross compiler for a Forth16 image using gforth.

EMPTY HEX

( Following polyFORTH manual)
( HOST and HOST'S FORTH are immediate for convenience. )
0017 VOCABULARY HOST        IMMEDIATE
HOST DEFINITIONS
0071 VOCABULARY FORTH       IMMEDIATE
0179 VOCABULARY ASSEMBLER
000B VOCABULARY TARGET ( Target's FORTH )

( Target memory )
CREATE IMAGE ( 8K )  HERE 2000  DUP ALLOT ERASE
: TC@ ( ta -- u8 )   IMAGE + C@ ;
: TC! ( u8 ta -- )   IMAGE + C! ;
: T@  ( ta -- u16 )  DUP TC@  SWAP 1+ TC@  8 LSHIFT OR ;
: T!  ( u16 ta -- )  2DUP TC!  SWAP 8 RSHIFT SWAP  1+ TC! ;
: TMOVE ( a ta u -)  SWAP IMAGE + SWAP CMOVE ;

VARIABLE H
: ORG   ( ta -- )      H ! ;
: HERE  ( -- taddr )   H @ ;
: ALLOT ( n -- )       H +! ;
: C,    ( char -- )    HERE TC!   1 H +! ;
: ,     ( n -- )       HERE  T!   2 H +! ;
: S, ( addr len -- )   0 ?DO  COUNT C,  LOOP DROP ;
: ALIGN ( -- )         HERE 1 AND IF 0 C, THEN ;

( Just 1 block for now)
: SAVE   IMAGE 0 BUFFER 400 CMOVE  UPDATE FLUSH ;

( Create target words)
CREATE CONTEXT   FORTH 1 , HERE 8 CELLS DUP ALLOT ERASE HOST
VARIABLE CURRENT   1 CURRENT !
VARIABLE WIDTH     3 DUP WIDTH C! WIDTH 1+ C!
VARIABLE LAST ( nfa)
: HASH ( str voc - 'link)
   2/  SWAP 1+ C@ +  7 AND  1+ CELLS CONTEXT + ;
: NAME ( str -)   HERE LAST !  COUNT DUP $80 OR C,
   WIDTH C@ MIN  1 ?DO COUNT C, LOOP  C@ $80 OR C,
   WIDTH 1+ C@ WIDTH C! ;
: TOGGLE ( n)   LAST @  DUP TC@ ROT XOR  SWAP TC! ;
: SMUDGE      $20 TOGGLE ;
: IMMEDIATE   $40 TOGGLE ;
: HEADER   ALIGN HERE  BL WORD  DUP CURRENT @ HASH
   DUP @ ,  ROT SWAP !  NAME ;

\ Let's do some test code
\ u8 hello[] = { 7,11,0,  7,3,0,  7,1,0,  8,  255,  };
0 [IF]
7 C, 0B ,   7 C, 3 ,   7 C, 1 ,   8 C, ( BIOS)   FF C, ( BAD)
( 0B: )'O' C, 'K' C, 0A C,
[THEN]

ASSEMBLER DEFINITIONS
: OP ( op)   CREATE FORTH C, DOES> C@ HOST C, ;
: OP, ( n op)   + C, ; ( not OR!)
: # ( n -)   07 C, , ;
08 OP BIOS   FF OP DIE ( invalid opcode)
: JMP ( a -)   0A C, , ;
: CALL ( a -)   09 C, , ;
0B OP RET
: LD ( reg -)   ?DUP IF 10 OP, ELSE 10 C, , THEN ;
: ST ( reg -)   ?DUP IF 18 OP, ELSE 18 C, , THEN ;

: BEGIN ( - a)   HERE ;
: UNTIL ( a cond -)   20 OP,  HERE - C, ;
: AGAIN ( a -)   -14 UNTIL ; ( 0C = unconditional branch )
: IF ( cond - a)   20 OP,  HERE 0 C, ;
: THEN ( a -)   HERE OVER -  SWAP TC! ;
: ELSE ( a - a2)   -14 IF  SWAP THEN ;
: WHILE    IF SWAP ;
: REPEAT   AGAIN THEN ;
: NOT ( cond - !cond)   8 XOR ;

30 OP DUP       31 OP DROP      32 OP SWAP      33 OP OVER
34 OP ROT       35 OP NIP       36 OP ?DUP      37 OP PICK
38 OP >R        39 OP R>        3A OP R@

40 OP 2DUP      41 OP 2DROP     42 OP 2SWAP     43 OP 2OVER
44 OP 2>R       45 OP 2R>       46 OP 2R@

50 OP +         51 OP -         52 OP *         53 OP /
54 OP MOD       55 OP AND       56 OP OR        57 OP XOR
58 OP LSHIFT    59 OP RSHIFT    5A OP INVERT    5B OP NEGATE

0 CONSTANT T   1 CONSTANT S   2 CONSTANT R   3 CONSTANT P
4 CONSTANT I   5 CONSTANT W

0 CONSTANT 0=   1 CONSTANT 0<   2 CONSTANT 0>   3 CONSTANT =
4 CONSTANT <    5 CONSTANT >    6 CONSTANT U<   7 CONSTANT U>

HOST DEFINITIONS

: PATCH ( a -)   9 T! ; \ patch "10 JMP" at offset 8

ASSEMBLER
( init stack and jump to test )
0 ORG   FF00 T LD  R ST  FE00 T LD  S ST  10 JMP

( print OK )
10 ORG   18 #  3 #  1 #  BIOS  DIE
18 ORG   S" OK" S,  0A C,

( subroutine that prints)
20 ORG   30 #  7 #  1 #  BIOS  RET
30 ORG   S" Subway" S,  0A C,

( call/ret)
40 PATCH
40 ORG   20 CALL  DIE

( loop )
50 PATCH
50 ORG   5 #  BEGIN  20 CALL  1 # -  0= UNTIL  DROP  DIE


HOST

SAVE .( Saved )
: RUN  S" ./vm" SYSTEM ;


0 [IF]
\ **********************************************************************
\ Compiler

: +ORIGIN  ( n -- ta )  CELL * ;

: COMPILE,  ( addr -- )   , ;

: EXIT  1 abort" no EXIT" 0 C, ;

\ Create Headers in Target Image
VARIABLE LAST
: DONE  ( store here and context for target )
    HERE 2 +ORIGIN T!  LAST @ 6 +ORIGIN T! ;

: HEADER   ( -- ) \ create target header, no cfa
    ALIGN  HERE  LAST  DUP @ ,  !
    BL WORD COUNT  DUP C, S,  ALIGN ;

: PRIOR ( -- nfa count )  LAST @ CELL +  DUP TC@ ;

VARIABLE STATE-T
: ?EXEC  STATE-T @ 0= ABORT" cannot execute target word!" ;

VARIABLE CSP
: !CSP  DEPTH CSP ! ;
: ?CSP  DEPTH CSP @ - ABORT" definition not finished" ;

: TARGET-CREATE ( -- ) \ create target word that compiles itself
   >IN @ HEADER >IN !  CREATE  HERE H,
   DOES>  ?EXEC  @ COMPILE, ;

: H.  . ;
: T'  ' >BODY @ ;
: HAS ( n -- )  T' SWAP +ORIGIN T! ; \ set boot vecs (needed?)

\ Generate primatives
: ?COMMENT  ( allow Forth comment after OP: etc. )
    >IN @  BL WORD COUNT S" (" COMPARE
    IF  >IN !  ELSE  DROP  [COMPILE] (  THEN ;

: C-COMMENT  S" /* " WRITE  BL WORD COUNT WRITE  S"  */ " WRITE ;
VARIABLE OP  ( next opcode )
: OP!  OP ! ;
: OP:  ( output opcode case statement )
    OP @ FF > ABORT" opcodes exhausted"
    OP @ info
    C-COMMENT  S" case 0x" WRITE  OP @ 0 <# # # #> WRITE  S" : " WRITE
    ?COMMENT ` ( copy rest of line )  1 OP +! ;
: ---  1 OP +! ;

: CODE   >IN @ TARGET-CREATE >IN !  OP @ C,  EXIT  OP: ;

\ Target Literals
: LITERAL  ( n -- )  ?EXEC  20 C,  , ;
: $   BL WORD NUMBER DROP LITERAL ;
: [']  T' LITERAL ;

\ Target branching constructs
: ?CONDITION  INVERT ABORT" unbalanced" ;
: MARK      ( -- here )     ?EXEC  HERE  ;
: >MARK     ( -- f addr )   TRUE  MARK   0 C, ;
: >RESOLVE  ( f addr -- )   MARK  OVER -  SWAP TC!   ?CONDITION ;
: <MARK     ( -- f addr )   TRUE  MARK ;
: <RESOLVE  ( f addr -- )   MARK  - C,   ?CONDITION ;

: NOT  ?EXEC  70 C, ;

: IF        58 C,  >MARK ;
: THEN      >RESOLVE ;
: ELSE      3 C,  >MARK  2SWAP >RESOLVE ;
: BEGIN     <MARK ;
: UNTIL     58 C,  <RESOLVE ;
: AGAIN     3 C,  <RESOLVE ;
: WHILE     IF  2SWAP ;
: REPEAT    AGAIN  THEN ;

: ?DO       4 C,  >MARK  <MARK ;
: DO        5 C,  >MARK  <MARK ;
: LOOP      6 C,  <RESOLVE  >RESOLVE ;
: +LOOP     7 C,  <RESOLVE  >RESOLVE ;

\ Compile Strings into the Target
: C"   HERE  [CHAR] " PARSE S, 0 C, ; \ c-style string
: ,"         [CHAR] " PARSE DUP C, S, ;

: S"      A C,  ," ;
: ."      B C,  ," ;
: ABORT"  C C,  ," ;

: { ;
: } ;
: forget ;
: ,A  , ;
: [COMPILE] ;
: 0, 0 , ;

\ : CELL+ CELL + ;
\ : CELLS CELL * ;

\ Defining Words
: CONSTANT  TARGET-CREATE  10 C, ALIGN   , ;
: VARIABLE  TARGET-CREATE  11 C, ALIGN 0 , ;

: BUFFER ( n <name> -- )  ALIGN  HERE  SWAP ALLOT  CONSTANT ;
: TAG  HERE  TAG DUP C, S,  CONSTANT ;

: [   0 STATE-T ! ;
: ]  -1 STATE-T ! ;

: T:  HEADER  ] ;  \ to create words with no host header

: ;_  POSTPONE ; ; IMMEDIATE
: IMMEDIATE  PRIOR 40 OR SWAP TC! ;
: ;   ?CSP EXIT [ ;
: :   TARGET-CREATE !CSP ] ;_

include ./kernel.f

DONE
SAVE
[THEN]
