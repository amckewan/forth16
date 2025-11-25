( F83 screen editor )

\ [DEFINED] EMPTY [IF] EMPTY [THEN] MARKER EMPTY
ONLY FORTH ALSO DEFINITIONS DECIMAL

VOCABULARY EDITOR

: L   SCR @ LIST ;
: N    1 SCR +! ;
: B   -1 SCR +! ;

1024 CONSTANT B/BUF
16 CONSTANT L/SCR

\ String Functions  SEARCH                            10Mar84map
\ COMPARE ( a1 u1 a2 u2 - -1/0/1 )
\ SEARCH ( buf size str len - adr remaining t | buf size f )

\ F83 SEARCH implemented with standard SEARCH
\ SEARCH   ( sadr slen badr blen -- n f )                         
\   Search for the s string inside of the b string.  If found    
\   f is true and n is the offset from the beginning of the      
\   string to where the pattern was found.  If not found, f is   
\   false and n is meaningless.  
: F83-SEARCH   ( sadr slen badr blen -- n f )
   DUP >R  2SWAP SEARCH DUP IF  R@ ROT - SWAP  THEN
   ROT R> 2DROP ;

[DEFINED] T{ [IF]
T{ : s1 S" abcdefghijklmnopqrstuvwxyz" ; s1 nip -> 26 }T
T{ : s2 S" abc"   ; -> }T
T{ : s3 S" jklmn" ; -> }T
T{ : s4 S" z"     ; -> }T
T{ : s5 S" mnoq"  ; -> }T
T{ : s6 S" 12345" ; -> }T
T{ : s7 S" "      ; -> }T
T{ s2 s1 F83-SEARCH -> 0 TRUE  }T
T{ s3 s1 F83-SEARCH -> 9 TRUE  }T
T{ s4 s1 F83-SEARCH -> 25 TRUE  }T
T{ s5 s1 F83-SEARCH NIP -> FALSE }T
T{ s6 s1 F83-SEARCH NIP -> FALSE }T
T{ s7 s1 F83-SEARCH -> 0 TRUE  }T
[THEN]

\ String operators                                    04Apr84map
\ Delete count chars at the start of the buffer, fill the end with spaces
: DELETE   ( buffer size count -- )
   OVER MIN >R  R@ - ( left over )  DUP 0>
   IF  2DUP SWAP DUP R@ + -ROT SWAP CMOVE  THEN  + R> BLANK ;

\ Insert a string into the start of the buffer, end chars lost
: INSERT   ( string length buffer size -- )
   ROT OVER MIN >R  R@ - ( left over )
   OVER DUP R@ +  ROT CMOVE>   R> CMOVE  ;

\ Overwrite string at the start of buffer
: REPLACE   ( string length buffer size -- )  ROT MIN CMOVE ;


\ Move the Editor's cursor around                     16Oct83map
EDITOR DEFINITIONS
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
: #AFTER       ( -- n )    C/L COL# -  ; \ Number of characters to the end of the line.
: #REMAINING   ( -- n )    B/BUF CURSOR - ; \ Number of characters to the end of the screen.
: #END         ( -- n )    #REMAINING COL# +  ; \ Number of characters between line start & screen end.

\ buffers                                             11Mar84map
VARIABLE CHANGED
: MODIFIED   ( -- )   CHANGED ON  UPDATE ;
: ?TEXT   ( adr -- adr+1 n )   >R   [CHAR] ^ PARSE DUP
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

\ buffers                                             11Mar84map
: K   ( -- )   'FIND PAD  C/PAD CMOVE
   'INSERT 'FIND  C/PAD CMOVE   PAD 'INSERT  C/PAD CMOVE  ;
: W   ( -- )   SAVE-BUFFERS  ;
: 'C#A   ( -- 'cursor #after )   'CURSOR #AFTER  MODIFIED  ;
: (I)  ( -- len 'insert len 'cursor #after )
   'INSERT ?TEXT  TUCK 'C#A  ;
: (TILL)  ( -- n )   'FIND ?TEXT 'C#A F83-SEARCH ?MISSING ;
: 'F+   ( n1 -- n2 )  'FIND C@ +  ;

\ line editing                                        01Apr84map
: I   ( -- )   (I)  INSERT  C ;
: O   ( -- )   (I)  REPLACE C ;
: P   ( -- )   'INSERT ?TEXT DROP 'LINE C/L CMOVE MODIFIED ;
: U   ( -- )   C/L C 'LINE C/L OVER #END INSERT  P ;
: X   ( -- )   KEEP  'LINE #END C/L  DELETE MODIFIED ;
: SPLIT  ( -- ) \ breaks the current line in two at the cursor
   PAD C/L 2DUP BLANK 'CURSOR #REMAINING INSERT MODIFIED ;
: JOIN   ( -- )   'LINE C/L + C/L  'C#A  INSERT ; \ puts a copy of the next line after the cursor
: WIPE   ( -- )   'START B/BUF BLANK  MODIFIED ;
\  : M   ( -- )   TRUE ABORT" Use G !" ;
: G   (  screen line -- )
   C/L * SWAP BLOCK +  C/L 'INSERT PLACE
   C/L NEGATE C  U  C/L C ;
: BRING   ( screen first last -- )
   1+ SWAP DO  DUP [ FORTH ] I [ EDITOR ] G  LOOP  DROP ;

\ find and replace                                    10Mar84map
: FIND?  ( - n f ) 'FIND ?TEXT  'CURSOR #REMAINING  F83-SEARCH ;
: F   ( -- )   FIND? ?MISSING   'F+ C ;
: ?ENOUGH ( n)   1+ DEPTH > ABORT" arg?" ;
: S   ( n - )   1 ?ENOUGH   FIND?
   IF  'F+ C  EXIT  THEN  DROP  FALSE OVER SCR @
   DO   N TOP  'FIND COUNT 'CURSOR #REMAINING F83-SEARCH
     IF  'F+ C DROP TRUE LEAVE  ELSE  DROP  THEN
     KEY? ABORT" Break!"
   LOOP  ?MISSING ;
: E   ( -- )   'FIND C@  DUP NEGATE C  'C#A ROT DELETE ;
: D   ( -- )   F E ;
: R   ( -- )   E I ;
: TILL    ( -- )   'C#A (TILL)  'F+  DELETE ;
\ deletes up to, but not including, <text>. 'Justify'
: J       ( -- )   'C#A (TILL)  DELETE ;
\ KT puts all text between the cursor and <text> inclusive
\ into the insert buffer. 'Keep-Till'
: KT      ( -- )   'CURSOR (TILL)  'F+  'INSERT PLACE  ;

( Line display)
: .LINE   'LINE COL# TYPE  [CHAR] ^ EMIT  'CURSOR #AFTER TYPE
   LINE# 3 .R SPACE ;
: ?LINE   >IN @ #TIB @ = IF  CR .LINE  THEN ;

: T DEPTH IF T THEN ?LINE ;
: F F ?LINE ;
: S S ?LINE ;
: E E ?LINE ;
: D D ?LINE ;
: R R ?LINE ;
: TILL TILL ?LINE ;
: J J ?LINE ;

: N N L ;
: B B L ;

FORTH DEFINITIONS
: LIST   EDITOR LIST ;
