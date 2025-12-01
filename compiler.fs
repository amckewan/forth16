\ Target compiler for gforth

EMPTY HEX

( Following polyFORTH manual)
( HOST and HOST'S FORTH are immediate for convenience)
0014 VOCABULARY HOST        IMMEDIATE
HOST DEFINITIONS
0041 VOCABULARY FORTH       IMMEDIATE
0145 VOCABULARY ASSEMBLER
0006 VOCABULARY TARGET      ( Target's FORTH )

( Target memory )
CREATE IMAGE ( 8K )  HERE 2000  DUP ALLOT ERASE
: TC@ ( ta -- u8 )   IMAGE + C@ ;
: TC! ( u8 ta -- )   IMAGE + C! ;
: T@  ( ta -- u16 )  DUP TC@  SWAP 1+ TC@  8 LSHIFT OR ;
: T!  ( u16 ta -- )  2DUP TC!  SWAP 8 RSHIFT SWAP  1+ TC! ;
: TMOVE ( a ta u -)  SWAP IMAGE + SWAP  CMOVE ;
: TDUMP ( ta u -)    SWAP IMAGE + SWAP  DUMP ;

( Target dictionary pointer )
VARIABLE H
: ORG   ( ta -- )      H ! ;
: HERE  ( -- taddr )   H @ ;
: ALLOT ( n -- )       H +! ;
: C,    ( char -- )    HERE TC!   1 H +! ;
: ,     ( n -- )       HERE  T!   2 H +! ;
: S, ( addr len -- )   0 ?DO  COUNT C,  LOOP DROP ;
: ALIGN ( -- )         HERE 1 AND IF 0 C, THEN ;

( Save just 1 block for now)
: SAVE   IMAGE 0 BUFFER 400 CMOVE  UPDATE FLUSH ;

( Target assembler)
include assembler.fs


( Create target words)
CREATE   CONTEXT   FORTH 1 , HERE 8 CELLS DUP ALLOT ERASE HOST
VARIABLE CURRENT   1 CURRENT !
VARIABLE WIDTH     31  DUP WIDTH C!  WIDTH 1+ C!
VARIABLE LAST ( nfa)
: HASH ( str voc - 'link)   2DROP CONTEXT CELL+ EXIT ( 1 thread)
   1-  SWAP 1+ C@ +  7 AND  1+ CELLS CONTEXT + ;
: NAME, ( str -)   HERE LAST !  COUNT DUP $80 OR C,
   WIDTH C@ MIN  1 ?DO COUNT C, LOOP  C@ ( $80 OR ) C,
   WIDTH 1+ C@ WIDTH C! ;
: TOGGLE ( n)   LAST @  DUP TC@ ROT XOR  SWAP TC! ;
: SMUDGE      $20 TOGGLE ;
: IMMEDIATE   $40 TOGGLE ;
: HEADER   ALIGN HERE  BL WORD  DUP CURRENT @ HASH
   DUP @ ,  ROT SWAP !  NAME,  ALIGN ;

: (TCREATE)   >IN @  HEADER  >IN !
    CREATE  HERE ( cfa) FORTH ,  DOES> @  HOST , ;
: TCREATE  TARGET DEFINITIONS (TCREATE)
    [COMPILE] HOST DEFINITIONS ;

: CODE  TCREATE  HERE 2+ ,  ASSEMBLER ;
: uCODE ( op)   TCREATE  HERE 2+ ,  , ;

: LABEL   HERE CONSTANT  ASSEMBLER ;

( We don't know the xt of the runtime words yet)
\  VARIABLE 'LIT
\  VARIABLE 'VAR
\  VARIABLE 'CONST
\  VARIABLE 'COLON
\  VARIABLE '(;CODE)
\  VARIABLE 'DOES


\  : CREATE     TCREATE 'VAR @ , ;
\  : VARIABLE   CREATE 0 , ;
\  : CONSTANT   TCREATE 'CONST @ , ;




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
