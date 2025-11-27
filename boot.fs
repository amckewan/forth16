( Initialization file for gforth )
\ This provides some comatibilit with Forth16 so
\ we can load the same source blocks.
\ to start:  $ gforth boot.fs -e hi

ONLY FORTH DEFINITIONS DECIMAL
warnings off

\ : marker, ( -- mark ) \ builds marker at HERE
\ : marker! ( mark -- ) \ restores state
variable golden
: GILD    align marker, golden ! ;
: EMPTY   golden @ marker!  gild ;

\ Simulate polyFORTH vocabularies. There are 8 wordlists,
\ 1=FORTH, 2=ASSEMBLER, 3=EDITOR, 4-8 unassigned.
\ CONTEXT specifies the search order, up to 4 wordlists.

WORDLIST CONSTANT ASSEMBLER \ names for order
WORDLIST CONSTANT EDITOR

WORDLIST WORDLIST WORDLIST WORDLIST WORDLIST 
EDITOR ASSEMBLER FORTH-WORDLIST 

CREATE CONTEXT 0 ,   , , , , , , , ,
CREATE CURRENT 0 ,

: 'WID ( v - a )  CELLS CONTEXT + ;

: VOC>ORDER ( n - wids #wids )
   DUP CONTEXT !  0 SWAP ( order n)
   BEGIN ?DUP WHILE
      DUP 12 RSHIFT ?DUP IF ( order n v )
         SWAP >R  'WID @ SWAP 1+  R>  ( +order n )
      THEN  4 LSHIFT ( next)  $FFFF AND ( for gforth )
   REPEAT ;

: VOCABULARY ( n)  CREATE , DOES> @ VOC>ORDER SET-ORDER ;

: DEFINITIONS   CONTEXT @ CURRENT !  DEFINITIONS ;
: :   :  CURRENT @ VOC>ORDER SET-ORDER ; \ NON-STANDARD!

HEX
0001 VOCABULARY FORTH
0012 VOCABULARY ASSEMBLER
0013 VOCABULARY EDITOR
DECIMAL

FORTH DEFINITIONS
: HI   9 LOAD ;

warnings on
