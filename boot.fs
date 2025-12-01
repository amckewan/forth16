( Initialization file for gforth )
\ This provides some compatibility with Forth16 so
\ we can load the same source blocks.
\ to start:  $ gforth boot.fs -e hi

ONLY FORTH DEFINITIONS DECIMAL
warnings off

\ Simulate polyFORTH vocabularies. There are 8 wordlists,
\ 1=FORTH, 2=ASSEMBLER, 3=EDITOR, 4-8 unassigned.
\ CONTEXT specifies the search order, up to 4 wordlists.
WORDLIST WORDLIST WORDLIST WORDLIST 
WORDLIST WORDLIST WORDLIST FORTH-WORDLIST 
CREATE WIDS   , , , , , , , ,

: WID ( v - a )  15 AND 1- CELLS WIDS + ;

VARIABLE CONTEXT
VARIABLE CURRENT

: .ORDER ( voc)   BASE @ SWAP  HEX
   BEGIN  DUP 15 AND .  4 RSHIFT  ?DUP 0= UNTIL  BASE ! ;
: ORDER   ."  Context: " CONTEXT @ .ORDER
          ."  Current: " CURRENT @ .ORDER ;


: VOC>ORDER ( n - )  0 OVER ( n order n )
   BEGIN  DUP 12 RSHIFT  ?DUP IF ( order n v )
       SWAP >R  WID @ SWAP 1+  R>  ( +order n )
     THEN  4 LSHIFT ( next) $FFFF AND ( gforth)
   ?DUP 0= UNTIL  SET-ORDER  CONTEXT ! ;

: VOCABULARY ( n)  CREATE , DOES> @ VOC>ORDER ;

: DEFINITIONS   DEFINITIONS  CONTEXT @ CURRENT ! ;
: :   :  CURRENT @ VOC>ORDER ; \ NON-STANDARD!

$0001 VOCABULARY FORTH
$0012 VOCABULARY ASSEMBLER
$0013 VOCABULARY EDITOR

FORTH DEFINITIONS

VARIABLE GOLDEN
: GILD    ALIGN marker, GOLDEN ! ;
: EMPTY   GOLDEN @ marker!  GILD  FORTH DEFINITIONS ;

( things we expect in the kernel)
1024 CONSTANT B/BUF
: 2+   2 + ;
: 2-   2 - ;

( shorthand )
: HI   9 LOAD ;
: CC   S" compiler.fs" INCLUDED ;
: KK   S" kernel.fs" INCLUDED ;
: VM   S" make vm" SYSTEM ;
: RUN  S" ./vm" SYSTEM ;

9 BLOCK DROP ( gforth fails when you try 0 block first! )

GILD
warnings on
