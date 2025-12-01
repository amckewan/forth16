( Number conversion)
EMPTY

VARIABLE 'NUMBER 0 ,
VARIABLE DPL
VARIABLE BASE

: DIGIT ( char - n)  DUP '9' > IF BL OR 'a' - 10 + ELSE '0' - THEN

   DUP BASE @ U< NOT ABORT" ?"
;

: 'c' ( a n - f)

: (NUMBER) ( adr len - d f)
   OVER DUP C@ ''' =  SWAP 2+ C@ ''' = AND  OVER 3 = AND
   IF  DROP C@ 0  -1 EXIT  THEN

   BASE @  OVER C@ ( 1st char) 2>R
   DUP 1 > IF
     R@ '#' = IF  10 BASE !  1 /STRING  ELSE
     R@ '$' = IF  16 BASE !  1 /STRING  ELSE
     R@ '%' = IF   2 BASE !  1 /STRING  THEN THEN
   THEN

   DUP 1 >  OVER C@ '-' = AND  DUP >R ( sign)  NEGATE /STRING  
   

  


   DPL @ 1+ IF  1 DPL +!  THEN

   R> IF DNEGATE THEN

   2R> DROP BASE ! ;


: NUMBER ( str -- n)   \ `NUMBER @ EXECUTE 
   -1 DPL !  (NUMBER)  0= ABORT" ?"  'NUMBER 2+ ! ;
