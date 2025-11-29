( Initialization file for gforth )
\ This provides some compatibility with Forth16 so
\ we can load the same source blocks.
\ to start:  $ gforth boot.fs -e hi

ONLY FORTH DEFINITIONS DECIMAL
warnings off

\ Simulate polyFORTH vocabularies. There are 8 wordlists,
\ 1=FORTH, 3=ASSEMBLER, 5=EDITOR, 7,9,B,D,F unassigned.
\ CONTEXT specifies the search order, up to 4 wordlists.

WORDLIST ( 3 ) CONSTANT ASSEMBLER \ names for order
WORDLIST ( 5 ) CONSTANT EDITOR
WORDLIST ( 7 ) CONSTANT HOST
WORDLIST ( 9 ) CONSTANT HOST-ASSEMBLER
WORDLIST ( B ) CONSTANT TARGET-FORTH
WORDLIST ( D ) CONSTANT TARGET-ASSEMBLER
WORDLIST ( F ) CONSTANT TARGET-EDITOR

CREATE WIDS
  FORTH-WORDLIST , ASSEMBLER , EDITOR , HOST , HOST-ASSEMBLER ,
  TARGET-FORTH , TARGET-ASSEMBLER , TARGET-EDITOR ,

VARIABLE CONTEXT
VARIABLE CURRENT

: 'WID ( v - a )  15 AND 2/ CELLS WIDS + ;

: VOC>ORDER ( n - )  0 OVER ( n order n )
   BEGIN  DUP 12 RSHIFT  ?DUP IF ( order n v )
       SWAP >R  'WID @ SWAP 1+  R>  ( +order n )
     THEN  4 LSHIFT ( next) $FFFF AND ( gforth)
   ?DUP 0= UNTIL  SET-ORDER  CONTEXT ! ;

: VOCABULARY ( n)  CREATE , DOES> @ VOC>ORDER ;

: DEFINITIONS   DEFINITIONS  CONTEXT @ CURRENT ! ;
: :   :  CURRENT @ VOC>ORDER ; \ NON-STANDARD!

$0001 VOCABULARY FORTH
$0013 VOCABULARY ASSEMBLER
$0015 VOCABULARY EDITOR

FORTH DEFINITIONS

VARIABLE GOLDEN
: GILD    ALIGN marker, GOLDEN ! ;
: EMPTY   GOLDEN @ marker!  GILD  FORTH DEFINITIONS ;

: HI   9 LOAD ;
: CC   S" compiler.fs" INCLUDED ;
: KK   S" kernel.fs" INCLUDED ;
: VM   S" make vm" SYSTEM ;

9 BLOCK DROP ( gforth fails when you try 0 first! )

GILD
warnings on
