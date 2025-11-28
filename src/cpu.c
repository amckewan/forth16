// Forth16 CPU

#include <stdio.h>
#include <string.h>
#include "vm.h"


#define NEXT  W = ptr(*I++); P = ptr(*W); break; // ITC NEXT
#define EXIT  I = ptr(*R++)
#define push  *--S = T, T =
#define pop   T = *S++
#define pop2  T = S[1], S += 2
#define pop3  T = S[2], S += 3
#define LOGICAL ? -1 : 0


// test programs
u8 bye[] = {7,0,0, 7,0,0, 8, 255}; // 0 # 0 # SWI
u8 hello[] = { 7,11,0,  7,3,0,  7,1,0,  8,  255,  'h','i','\n'};

// #define sample hello

int cpu_run(u16 start, u8 *vmem) {
    i16  T = 0;
    i16 *S = ptr(0);
    u16 *R = ptr(0);
    u8  *P = ptr(start);
    u16 *I = ptr(0);
    u16 *W = ptr(0);

    i16 t;

#ifdef sample
    memcpy(vmem, sample, sizeof sample);
#endif
#if 0
    for (int i = 0; i < 100; i++) {
        if (i%16 == 0) putchar('\n');
        printf("%02X ", vmem[i]);
    }   putchar('\n');
    // return 0;
#endif

    while (1) {
        switch (*P++) {
            // 0-F runtime
            case 0x00:  NEXT
            case 0x01:  push va(W + 1), NEXT // DOVAR
            case 0x02:  push  *(W + 1), NEXT // DOCON
            case 0x03:  *--R = va(I), I = W + 1, NEXT // DOCOL
            case 0x04:  *--R = va(I), I = (u16*)(P + 1), push va(W + 1), NEXT // DOES
            case 0x05:  I = ptr(*R++),  NEXT // EXIT
            case 0x06:  push *I++; break; // LIT
            case 0x07:  push (P[0] | (P[1] << 8)), P+=2; break; // #

            case 0x08:  S = bios(T, S, vmem); break;
            case 0x09:  *--R = va(P+2); // CALL..
            case 0x0A:  P = ptr(P[0] | (P[1] << 8)); break; // JMP
            case 0x0B:  P = ptr(*R++); break; // RET  
            case 0x0C:  P += *(i8*)P; break; // BRANCH

            // 10-17 load T from register
            case 0x10:  break;
            case 0x11:  T = va(S); break;
            case 0x12:  T = va(R); break;
            case 0x13:  T = va(P); break;
            case 0x14:  T = va(I); break;
            case 0x15:  T = va(W); break;

            // 18-1F store T to register (aligned)
            case 0x18:  break;
            case 0x19:  S = ptr(T & 0xFFFE); break;
            case 0x1A:  R = ptr(T & 0xFFFE); break;
            case 0x1B:  P = ptr(T);          break;
            case 0x1C:  I = ptr(T & 0xFFFE); break;
            case 0x1D:  W = ptr(T & 0xFFFE); break;

            // 20-2F conditional branches, 8-bit signed offset
#define IF(cond)  P += (cond) ? 1 : *(i8*)P
#define IF1(cond) IF(cond); pop;  break;
#define IF2(cond) IF(cond); pop2; break;
            case 0x20:  IF1(T == 0)             // 0= IF
            case 0x21:  IF1(T < 0)              // 0< IF
            case 0x22:  IF1(T > 0)              // 0> IF
            case 0X23:  IF2(*S == T)            //  = IF
            case 0x24:  IF2(*S < T)             //  < IF
            case 0x25:  IF2(*S > T)             //  > IF
            case 0x26:  IF2((u16)*S < (u16)T)   // U< IF
            case 0x27:  IF2((u16)*S > (u16)T)   // U> IF

            // 28-2F are the opposites of 20-27
            case 0x28:  IF1(T != 0)             // 0= NOT IF
            case 0x29:  IF1(T >= 0)             // 0< NOT IF
            case 0x2A:  IF1(T <= 0)             // 0> NOT IF
            case 0X2B:  IF2(*S != T)            //  = NOT IF
            case 0x2C:  IF2(*S >= T)            //  < NOT IF
            case 0x2D:  IF2(*S <= T)            //  > NOT IF
            case 0x2E:  IF2((u16)*S >= (u16)T)  // U< NOT IF
            case 0x2F:  IF2((u16)*S <= (u16)T)  // U> NOT IF


            case 0x40:  *--S = T; break; // DUP
            case 0x41:  pop; break; // DROP
            case 0x42:  t = T; T = *S; *S = t; break; // SWAP
            case 0x43:  push S[1]; break; // OVER
            case 0x44:  t = S[1], S[1] = *S, *S = T, T = t; break; // ROT
            case 0x45:  T = S[T]; break; // PICK
// CODE NIP   SWAP DROP NEXT
// CODE TUCK  SWAP OVER NEXT
// CODE ?DUP   DUP IF DUP THEN NEXT

// CODE >R   >R  NEXT
            case 100:  *--R = T, pop; break; // >R
            case 101:  push *R++; break; // R>
            case 102:  push *R  ; break; // R@
            case 103:  *--R = *S++, *--R = T, pop;   break; // 2>R
            case 104:  push R[1], push R[0], R += 2; break; // 2R>
            case 105:  push R[1], push R[0];         break; // 2R@

            case 110:  T = *S++ + T; break; // +
            case 111:  T = *S++ - T; break; // -
            case 112:  T = *S++ * T; break; // *
            case 113:  T = *S++ / T; break; // /
            case 117:  T = *S++ % T; break; // MOD
            case 114:  T = *S++ & T; break; // AND
            case 115:  T = *S++ | T; break; // OR
            case 116:  T = *S++ ^ T; break; // XOR

// : OP ( op)   CREATE C,  DOES> C@ C, ;
// 110 OP +    111 OP -   112 OP *   ... 
// CODE +   + NEXT
// CODE @   @ NEXT

            // 88-95 NOTs

// ASSEMBLER
// : IF ( cond - a)   80 + C,  HERE  0 C, ;
// : THEN ( a -)   HERE OVER -  SWAP C! ;
// : ELSE ( a - a2)   -75 IF  SWAP THEN ;
// : BEGIN ( - a)   HERE ;
// : UNTIL ( a cond -)   80 + C,  HERE SWAP - C, ;
// : AGAIN ( a -)   -75 UNTIL ;
// : WHILE ( a cond - a2 a)   IF SWAP ;
// : REPEAT ( a a2 -)   AGAIN THEN ;
// 0 CONSTANT 0=    1 CONSTANT 0<   2 CONSTANT 0>
// 3 CONSTANT =     4 CONSTANT <    5 CONSTANT  >
// 6 CONSTANT U<    7 CONSTANT U>
// : NOT   8 XOR ;

            case 70: { // M*/ ( d1 n1 n2 -- d2)
                int64_t d = S[1] << 16 | S[2];
                d = d * S[0] / T;
                S += 2;
                S[0] = d, T = d >> 16;
                break;
            }
            case 71: { // UM* ( u1 u2 -- ud )
                unsigned d = (unsigned)S[0] * (unsigned)T;
                S[0] = d, T = d >> 16;
                break;
            }
            case 72: { // M* ( n1 n2 -- d )
                int d = S[0] * T;
                S[0] = d, T = d >> 16;
                break;
            }
            case 73: { // UM/MOD ( ud u -- rem quot )
                unsigned d = (unsigned)S[0] << 16 | (unsigned)S[1];
                S[0] = d % T, T = d / T;
                break;
            }
            case 74: { // UM* ( u1 u2 -- ud )
                unsigned d = (unsigned)S[0] * (unsigned)T;
                S[0] = d, T = d >> 16;
                break;
            }
            // : 
            default:
                return P[-1]; // invalid opcode

}

    }
}
