( Initialization file for gforth )
\ gforth boot.fs -e hi

ONLY FORTH ALSO DEFINITIONS DECIMAL
warnings off

\ : marker, ( -- mark ) \ builds marker at HERE
\ : marker! ( mark -- ) \ restores state
variable golden
: GILD    align marker, golden ! ;
: EMPTY   golden @ marker! ;

\ Simulate polyFORTH-style vocabularies.
\ CONTEXT specifies the search order, up to 4 wordlists.
\ There are up to 8 wordlists, 1=FORTH, 3=ASSEMBLER, 5=EDITOR,
\ 5 more for apps

WORDLIST CONSTANT ASSEMBLER \ nice for order
WORDLIST CONSTANT EDITOR

WORDLIST WORDLIST WORDLIST WORDLIST WORDLIST 
EDITOR ASSEMBLER FORTH-WORDLIST 
CREATE WORDLISTS  , , , , , , , ,

variable 'context
variable 'current

: 'wid ( n - a)  15 and 1- 2/ cells wordlists + ;

: voc>order ( n)
   dup 'context !  0 swap ( order n)
   begin ?dup while
      dup 12 rshift 15 and ?dup if ( order n v# )
         swap >r  'wid @ swap 1+ ( +order )  r>
      then  4 lshift $ffff and
   repeat set-order ;

: VOCABULARY ( n)  create , does> @ voc>order ;

: DEFINITIONS   'context @ 'current !  definitions ;
: :   :  CURRENT @ CONTEXT !  'current @ voc>order ; \ NON-STANDARD!

HEX
0001 VOCABULARY FORTH
0013 VOCABULARY ASSEMBLER
0015 VOCABULARY EDITOR
DECIMAL

FORTH DEFINITIONS
: HI   9 LOAD ;

1024 CONSTANT B/BUF
: L   SCR @ LIST ;
: N    1 SCR +!  L ;
: B   -1 SCR +!  L ;


warnings on

