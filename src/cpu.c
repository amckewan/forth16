// Forth16 CPU

#include "vm.h"

u8 vmem[65536]; // 64K Forth memory

#define NEXT  W = ptr(*I++); P = ptr(*W); break; // ITC NEXT
#define EXIT  I = ptr(*R++)
#define push  *--S = T, T =
#define pop   T = *S++
#define pop2  T = S[1], S += 2
#define pop3  T = S[2], S += 3
#define LOGICAL ? -1 : 0

#define OFFSET      *(i8*)P
#define BRANCH      P += OFFSET
#define NOBRANCH    P += 1

void cpu_run(void) {
    i16  T=0;
    i16 *S=0;
    u16 *R=0;
    u8  *P=0;
    u16 *I=0;
    u16 *W=0;

    i16 t;

    while (1) {
        switch (*P++) {
            case 0x00:  NEXT

            case 0x01:  P = ptr(*R++); break; // RET
            case 0x02:  *--R = va(P+2); // CALL...
            case 0x03:  P = ptr(P[0] | (P[1] << 8)); break; // JMP
            case 0x04:  break;
            case 0x05:  BRANCH; break; // BRANCH
            case 0x06:  push *I++; break; // LIT
            case 0x07:  push P[0] | (P[1] << 8), P+=2; break; // #

            // Load T from register or immed.
            case 0x10:  T = *P++;  break; // LDI8
            case 0x11:  T = va(S); break; // S LDR
            case 0x12:  T = va(R); break; // R LDR
            case 0x13:  T = va(P); break; // P LDR
            // Store T to register
            case 0x18:  T = P[0] | (P[1] << 8), P+=2; break; // LDI16
            case 0x19:  S = ptr(T); break;
            case 0x1A:  R = ptr(T); break;
            case 0x1B:  P = ptr(T); break;


            case 0x30:  I = ptr(*R++),  NEXT // EXIT
            case 0x31:  push va(W + 1), NEXT // DOVAR
            case 0x32:  push *(W + 1),  NEXT // DOCON
            case 0x33:  *--R = va(I), I = W + 1, NEXT // DOCOL
            case 0x34:  push va(W + 1), I = (u16*)(P + 1), NEXT // DODOES

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

            // Conditional branches, 8-bit offset
#define IF(cond)  if (cond) NOBRANCH; else BRANCH
#define IF1(cond) IF(cond); pop; break;
#define IF2(cond) IF(cond); pop2; break;

            case 80:  IF1(T == 0)   // 0=IF
            case 81:  IF1(T < 0)    // 0<IF
            case 82:  IF1(T > 0)    // 0>IF
            case 83:  IF2(*S == T)  // =IF
            case 84:  IF2(*S < T)   // <IF
            case 85:  IF2(*S > T)   // >IF
            case 86:  IF2((u16)*S < (u16)T)   // U<IF
            case 87:  IF2((u16)*S > (u16)T)   // U>IF
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


}

    }
}
