S 0
EAD/GhAA/hkKUAEAAE9LCgcNAAcDAAcBAAgHAAAIAAAHMAAHBwAHAQAICwAAAAAAU3Vid2F5CgAAAAAAAAAAAA==
CSAAAAAAAAAAAAAAAAAAAAcFAAkgAAcBAFEg+DEAAAAHNBIHAwAIChAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==
AwAAAAAAAAAKAQUAAAAAABIBBwMACAAAAAAAAAAAAAAiAQcAAAgAACoBAAAAAAAAAAEQAQgBAAAAAAAAAAAAAA==
AAEwASABAAAAAAAAAAAAABBCARwwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==
B 1
S 9
( Electives)   FORTH DEFINITIONS DECIMAL
( Utils)  10 LOAD  11 LOAD
( Editor)  15 LOAD

: HI ." ready" ;
GILD










S 10
( Screen index )
: USED? ( scr -- f )   0 SWAP BLOCK C/L BOUNDS DO
   I C@ BL < IF DROP 0 LEAVE THEN  I C@ BL > +  LOOP ;
: INDEX ( start end -- )  1+ SWAP DO
   CR I 4 .R SPACE  I USED? IF I BLOCK C/L TYPE THEN  LOOP ;
: QX ( n)   60 / 60 * DUP 60 + SWAP DO
   I 3 MOD 0= IF CR THEN  I 4 .R SPACE
   I USED? IF I BLOCK 19 TYPE ELSE 19 SPACES THEN  SPACE LOOP ;

( Screen copy)
: COPY ( from to -)  SWAP BLOCK SWAP BUFFER B/BUF MOVE UPDATE ;
: CONVEY ( first last dest -)   ROT  2DUP < IF
    ROT 1+ SWAP DO  I OVER COPY  1+  LOOP
  ELSE  SWAP >R  2DUP - R> +  SWAP ROT
     DO  I OVER COPY  1-  -1 +LOOP  THEN DROP ;

S 11
( String operators from F83)
( Delete count chars at the start of the buffer, blank to end)
: DELETE   ( buffer size count -- )
   OVER MIN >R  R@ - ( left over )  DUP 0>
   IF  2DUP SWAP DUP R@ + -ROT SWAP CMOVE  THEN  + R> BLANK ;

( Insert a string into the start of the buffer, end chars lost)
: INSERT   ( string length buffer size -- )
   ROT OVER MIN >R  R@ - ( left over )
   OVER DUP R@ +  ROT CMOVE>   R> CMOVE  ;

( Overwrite string at the start of buffer)
: REPLACE ( string length buffer size -- )  ROT MIN CMOVE ;



B 12
S 15
( Editor )   DECIMAL

: F83-SEARCH   ( sadr slen badr blen -- n f )
   DUP >R  2SWAP SEARCH DUP IF  R@ ROT - SWAP  THEN
   ROT R> 2DROP ;

EDITOR DEFINITIONS   16 23 THRU   FORTH DEFINITIONS

: LIST ( n)   [ EDITOR ] TOP LIST EDITOR DECIMAL ; FORTH
: L   SCR @ LIST ;






S 16
( Move the Editor's cursor around)
VARIABLE R# ( cursor, 0-1023)
: TOP          ( -- )      0 R# ! ;
: C            ( n -- )    R# @ + B/BUF MOD R# ! ;
: T            ( n -- )    TOP  C/L *  C ;
: CURSOR       ( -- n )    R# @ ;
: LINE#        ( -- n )    CURSOR  C/L  /  ;
: COL#         ( -- n )    CURSOR  C/L  MOD  ;
: 'START       ( -- adr )  SCR @ BLOCK ;
: 'CURSOR      ( -- adr )  'START  CURSOR  + ;
: 'LINE        ( -- adr )  'CURSOR  COL# -  ;
: #AFTER       ( -- n )    C/L COL# -  ;
: #REMAINING   ( -- n )    B/BUF CURSOR - ;
: #END         ( -- n )    #REMAINING COL# +  ;


S 17
( Editor buffers)
VARIABLE CHANGED
: MODIFIED   ( -- )   CHANGED ON  UPDATE ;
: ?TEXT   ( adr -- adr+1 n )   >R  94 PARSE DUP
   IF  R@ C/L 1+ BLANK  R@ PLACE  ELSE  2DROP  THEN  R> COUNT ;
84 CONSTANT C/PAD
: 'INSERT   ( -- insert-buffer )   PAD     C/PAD + ;
: 'FIND     ( -- find-buffer )     'INSERT C/PAD + ;
: .FRAMED   ( adr -- )   ." '" COUNT TYPE ." '" ;
: .BUFS     ( -- )
   CR ." I " 'INSERT .FRAMED   CR ." F " 'FIND .FRAMED ;
: ?MISSING   ( n f -- n | )
   0= IF  DROP 'FIND .FRAMED ."  not found " QUIT THEN ;
: KEEP   ( -- )   'LINE C/L 'INSERT  PLACE  ;


S 18
( Editor buffers)
: K   ( -- )   'FIND PAD  C/PAD CMOVE
   'INSERT 'FIND  C/PAD CMOVE   PAD 'INSERT  C/PAD CMOVE  ;
: W   ( -- )   SAVE-BUFFERS  ;
: 'C#A   ( -- 'cursor #after )   'CURSOR #AFTER  MODIFIED  ;
: (I)  ( -- len 'insert len 'cursor #after )
   'INSERT ?TEXT  TUCK 'C#A  ;
: (TILL)  ( -- n )   'FIND ?TEXT 'C#A F83-SEARCH ?MISSING ;
: 'F+   ( n1 -- n2 )  'FIND C@ +  ;







S 19
( Line editing)
: I   ( -- )   (I)  INSERT  C ;
: O   ( -- )   (I)  REPLACE C ;
: P   ( -- )   'INSERT ?TEXT DROP 'LINE C/L CMOVE MODIFIED ;
: U   ( -- )   C/L C 'LINE C/L OVER #END INSERT  P ;
: X   ( -- )   KEEP  'LINE #END C/L  DELETE MODIFIED ;
: SPLIT  ( -- ) \ breaks the current line in two at the cursor
   PAD C/L 2DUP BLANK 'CURSOR #REMAINING INSERT MODIFIED ;
: JOIN   ( -- )   'LINE C/L + C/L  'C#A  INSERT ;
: WIPE   ( -- )   'START B/BUF BLANK  MODIFIED ;
: G   (  screen line -- )  ( not M? )
   C/L * SWAP BLOCK +  C/L 'INSERT PLACE
   C/L NEGATE C  U  C/L C ;
: BRING   ( screen first last -- )
   1+ SWAP DO  DUP [ FORTH ] I [ EDITOR ] G  LOOP  DROP ;

S 20
( Find and replace)
: FIND? ( - n f ) 'FIND ?TEXT  'CURSOR #REMAINING  F83-SEARCH ;
: F   ( -- )   FIND? ?MISSING   'F+ C ;
: ?ENOUGH ( n)   1+ DEPTH > ABORT" arg?" ;
: S   ( n - )   1 ?ENOUGH   FIND?
   IF  'F+ C  EXIT  THEN  DROP  FALSE OVER SCR @
   DO   1 SCR +! TOP  'FIND COUNT 'CURSOR #REMAINING F83-SEARCH
     IF  'F+ C DROP TRUE LEAVE  ELSE  DROP  THEN
     KEY? ABORT" Break!"
   LOOP  ?MISSING ;
: E   ( -- )   'FIND C@  DUP NEGATE C  'C#A ROT DELETE ;
: D   ( -- )   F E ;
: R   ( -- )   E I ;
: TILL    ( -- )   'C#A (TILL)  'F+  DELETE ;
: J       ( -- )   'C#A (TILL)  DELETE ;
: KT      ( -- )   'CURSOR (TILL)  'F+  'INSERT PLACE  ;
S 21
( Enter multiple new lines)
: NEW ( n)   16 SWAP
   DO [ FORTH ] I [ EDITOR ] DUP T  CR 2 .R SPACE  QUERY
      #TIB @ IF  P  ELSE  LEAVE  THEN
   LOOP ;











S 22
( Editor LIST)
: .LN ( n)   2 .R SPACE ;
: .ROW ( row)  DUP .LN  C/L * 'START +  C/L TYPE ;
: .LINE   LINE# .LN
   'LINE COL# TYPE  94 EMIT  'CURSOR #AFTER TYPE ;

: LIST ( n)   DUP SCR !  ." Scr " .
   16 0 DO  [ FORTH ] I [ EDITOR ]  CR
      DUP LINE# = IF DROP .LINE ELSE .ROW THEN ." |"
   LOOP ;

: PLAIN ( n)   BLOCK  16 0 DO
   DUP C/L -TRAILING CR TYPE  C/L +   LOOP DROP  CR ;



S 23
( Line editor UI )
( Display current line if there are no more commands)
: ?L   >IN @ #TIB @ = IF  CR .LINE  THEN ;

: T ( n - )   DEPTH IF T THEN ?L ;

: F F ?L ;   : S S ?L ;   : E E ?L ;   : D D ?L ;
: O O ?L ;   : R R ?L ;   : I I ?L ;   : J J ?L ;
: TILL TILL ?L ;

: L  SCR @ LIST ;   : N  1 SCR +! L ;   : B  -1 SCR +! L ;
: LL  TOP L ;




B 24
S 60
( Target compiler for gforth)  EMPTY HEX
( Following polyFORTH manual)
( HOST and HOST'S FORTH are immediate for convenience)
0014 VOCABULARY HOST        IMMEDIATE
HOST DEFINITIONS
0041 VOCABULARY FORTH       IMMEDIATE
0145 VOCABULARY ASSEMBLER
0006 VOCABULARY TARGET      ( Target's FORTH )
DECIMAL

( Target memory)  61 LOAD  62 LOAD
( Assembler)  63 LOAD




S 61
( Target memory )
CREATE IMAGE   HERE 8 B/BUF * DUP ALLOT ERASE
: TC@ ( ta -- u8 )   IMAGE + C@ ;
: TC! ( u8 ta -- )   IMAGE + C! ;
: T@  ( ta -- u16 )  DUP TC@  SWAP 1+ TC@  8 LSHIFT OR ;
: T!  ( u16 ta -- )  2DUP TC!  SWAP 8 RSHIFT  SWAP 1+ TC! ;

: TMOVE ( a ta u -)  SWAP IMAGE + SWAP  CMOVE ;
: TDUMP ( ta u -)    SWAP IMAGE + SWAP  DUMP ;







S 62
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
: SAVE   IMAGE 0 BUFFER B/BUF CMOVE  UPDATE FLUSH ;




S 63
( Target assembler)
HOST ASSEMBLER DEFINITIONS DECIMAL
: OP  ( op)   CREATE FORTH C,  DOES> C@ ( op)   HOST C, ;
: OP# ( op)   CREATE FORTH C,  DOES> C@ ( n op) HOST C, , ;
: OP, ( n op)   + C, ; ( not OR!)

64 66 THRU
HOST DEFINITIONS DECIMAL








S 64
( Special ops)  HEX
00 OP  NEXT
07 OP# #
08 OP  BIOS
09 OP# CALL
0A OP# JMP
0B OP  RET
0D OP  EXECUTE

( Register load and store)
0 CONSTANT T    1 CONSTANT S    2 CONSTANT R    3 CONSTANT P
4 CONSTANT I    5 CONSTANT W

: LD ( reg -)   ?DUP IF 10 OP, ELSE 10 C, , THEN ;
: ST ( reg -)   ?DUP IF 18 OP, ELSE 18 C, , THEN ;

S 65
( Conditional branches)  HEX
: BEGIN ( - a)       HERE ;
: UNTIL ( a cond -)  20 OP,  HERE - C, ;
: AGAIN ( a -)       -14 UNTIL ; ( 0C = unconditional branch)
: IF ( cond - a)     20 OP,  HERE 0 C, ;
: THEN ( a -)        HERE OVER -  SWAP TC! ;
: ELSE ( a - a2)     -14 IF  SWAP THEN ;
: WHILE ( a - a2 a)  IF SWAP ;
: REPEAT ( a2 a -)   AGAIN THEN ;

: NOT ( cond - !cond)   8 XOR ;

0 CONSTANT 0=   1 CONSTANT 0<   2 CONSTANT 0>   3 CONSTANT =
4 CONSTANT <    5 CONSTANT >    6 CONSTANT U<   7 CONSTANT U>


S 66
( Opcodes 30,40,50)
30 OP DUP       31 OP DROP      32 OP SWAP      33 OP OVER
34 OP ROT       35 OP NIP       36 OP ?DUP      37 OP PICK
38 OP >R        39 OP R>        3A OP R@

40 OP 2DUP      41 OP 2DROP     42 OP 2SWAP     43 OP 2OVER
44 OP 2>R       45 OP 2R>       46 OP 2R@

50 OP +         51 OP -         52 OP *         53 OP /
54 OP MOD       55 OP AND       56 OP OR        57 OP XOR
58 OP LSHIFT    59 OP RSHIFT    5A OP ARSHIFT   5B OP INVERT
5C OP NEGATE




B 67
S 69
( Create target words)  ( patched to ease bootstrap)
CREATE CONTEXT   FORTH 1 ,  HERE 8 CELLS DUP ALLOT ERASE  HOST
VARIABLE CURRENT   1 CURRENT !
VARIABLE WIDTH     31  DUP WIDTH C!  WIDTH 1+ C!
VARIABLE LAST ( nfa)
: HASH ( str voc - 'link) 2DROP CONTEXT CELL+ EXIT ( 1 thread)
   1-  SWAP 1+ C@ +  7 AND  1+ CELLS CONTEXT + ;
: NAME, ( str -)   HERE LAST !  COUNT DUP $80 OR C,
   WIDTH C@ MIN  1 ?DO COUNT C, LOOP  C@ ( $80 OR ) C,
   WIDTH 1+ C@ WIDTH C! ( reset width) ;
: TOGGLE ( n)   LAST @  DUP TC@ ROT XOR  SWAP TC! ;
: SMUDGE      $20 TOGGLE ;
: IMMEDIATE   $40 TOGGLE ;
: HEADER   ALIGN HERE  BL WORD  DUP CURRENT @ HASH
   DUP @ ,  ROT SWAP !  NAME,  ALIGN ;

S 70
( Target defining words)
: (TCREATE)   >IN @  HEADER  >IN !
    CREATE  HERE ( cfa) FORTH ,  DOES> @  HOST , ;
: TCREATE  TARGET DEFINITIONS (TCREATE)
    [COMPILE] HOST DEFINITIONS ;

: CODE  TCREATE  HERE 2+ ,  ASSEMBLER ;
: uCODE ( op)   TCREATE  HERE 2+ ,  , ;
: LABEL   HERE CONSTANT  ASSEMBLER ;







B 71
S 75
( Forth16 kernel)  HOST DEFINITIONS HEX
( Memory map)
FORTH   F000 CONSTANT R0
        E000 CONSTANT S0
        0010 CONSTANT 'COLD

( Startup code: init stacks, execute 'COLD )
0 ORG  HOST ASSEMBLER
    R0 T LD   R ST   S0 T LD   S ST   'COLD T LD   EXECUTE
10 ORG   FFFF , ( xt of startup word)

( e.g.,  ' MY-STARTUP-WORD 'COLD T! )




B 76
E 120
