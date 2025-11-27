( Initialization file for gforth )
\ This provides some comatibility with Forth16 so
\ we can load the same source blocks.
\ to start:  $ gforth boot.fs -e hi

ONLY FORTH DEFINITIONS DECIMAL
warnings off

\ Simulate polyFORTH vocabularies. There are 8 wordlists,
\ 1=FORTH, 3=ASSEMBLER, 5=EDITOR, 7,9,B,D,F unassigned.
\ CONTEXT specifies the search order, up to 4 wordlists.

WORDLIST CONSTANT ASSEMBLER \ names for order
WORDLIST CONSTANT EDITOR

WORDLIST WORDLIST WORDLIST WORDLIST WORDLIST 
EDITOR ASSEMBLER FORTH-WORDLIST

CREATE CONTEXT 0 , 0 ,   , , , , , , , ,

CONTEXT CELL+ CONSTANT CURRENT

: 'WID ( v - a )  15 AND 2/ 2 + CELLS CONTEXT + ;

: VOC>ORDER ( n - )
   DUP CONTEXT !  0 SWAP ( order n)
   BEGIN  DUP 12 RSHIFT ( v) ?DUP IF ( order n v )
       SWAP >R  'WID @ SWAP 1+  R>  ( +order n )
     THEN  4 LSHIFT ( next)  $FFFF AND ( for gforth )
   ?DUP 0= UNTIL  SET-ORDER ;

: VOCABULARY ( n)  CREATE , DOES> @ VOC>ORDER ;

: DEFINITIONS   CONTEXT @ CURRENT !  DEFINITIONS ;
: :   :  CURRENT @ VOC>ORDER ; \ NON-STANDARD!

HEX
0001 VOCABULARY FORTH
0013 VOCABULARY ASSEMBLER
0015 VOCABULARY EDITOR
DECIMAL

FORTH DEFINITIONS

VARIABLE GOLDEN
: GILD    ALIGN marker, GOLDEN ! ;
: EMPTY   GOLDEN @ marker!  FORTH DEFINITIONS  GILD ;

: HI   9 LOAD ;

GILD
warnings on
