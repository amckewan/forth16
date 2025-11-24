\ Boot code, build with gforth

[DEFINED] EMPTY [IF] EMPTY [THEN] MARKER EMPTY
ONLY FORTH ALSO DEFINITIONS
WARNINGS ON
HEX

: TAG S" cross" ;

: H, , ;

\ Memory Access Words
CREATE IMAGE 2000 ALLOT   IMAGE 2000 ERASE
: >T     ( taddr -- addr )   IMAGE + ;
: TC@    ( taddr -- char )   >T C@ ;
: TC!    ( char taddr -- )   >T C! ;
: T@     ( taddr -- u )      >T UW@ ;
: T@S    ( taddr -- n )      >T SW@ ;
: T!     ( n taddr -- )      >T W! ;

VARIABLE H
: THERE  ( -- taddr )   H @ ;
: TALLOT ( n -- )       H +! ;
: TC,    ( char -- )    H @ TC!   1 H +! ;
: T,     ( n -- )       H @  T!   2 H +! ;
: TS, ( addr len -- )   0 ?DO  COUNT TC,  LOOP DROP ;

: TALIGN   HERE 1 AND IF 0 C, THEN ;

: TDUMP   IMAGE H @ DUMP ;

1 BLOCK DROP ( seems gforth needs a kick-start )
: SAVE   IMAGE 0 BUFFER 400 CMOVE  UPDATE  FLUSH ;


VOCABULARY ASSEMBLER
ASSEMBLER DEFINITIONS
: OP  ( op)   CREATE C, DOES> C@ TC, ;
: OP# ( op)   CREATE C, DOES> C@ TC, T, ;
: OPR ( op)   CREATE C, DOES> C@ TC, HERE SWAP - TC, ;

00 OP NEXT      01 OP RET       02 OP# CALL     03 OP# JMP
04 OPR BRA    05 OP NOP1      06 OP LIT       07 OP# #
08 OP SWI

\  x0 OP +         x1 OP -         x2 OP *         x3 OP /
\  x0 OP ADD         x1 OP SUB         x2 OP MUL         x3 OP DIV

FF OP DIE

FORTH DEFINITIONS
: C, TC, ;
: , T, ;
ASSEMBLER
\ u8 hello[] = { 7,11,0,  7,3,0,  7,1,0,  8,  255,  'h','i','\n'};
B #  3 #  1 #  SWI  DIE  'h' C, 'i' C, A C,

FORTH DEFINITIONS

\ : RESET  0 BLOCK 0 B/BUF MOVE  0 >R ;
