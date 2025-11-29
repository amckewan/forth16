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
#define LIT   (P[0] | (P[1] << 8))


// test programs
// #define sample hello
#ifdef sample
u8 bye[] = {7,0,0, 7,0,0, 8, 255}; // 0 # 0 # SWI
u8 hello[] = { 7,11,0,  7,3,0,  7,1,0,  8,  255,  'h','i','\n'};
#endif

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
        if (i && i%16 == 0) putchar('\n');
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
            case 0x07:  push LIT, P+=2; break; // #

            case 0x08:  S = bios(T, S, vmem), pop; break;
            case 0x09:  *--R = va(P+2); // CALL..
            case 0x0A:  P = ptr(LIT); break; // JMP
            case 0x0B:  P = ptr(*R++); break; // RET  
            case 0x0C:  P += *(i8*)P; break; // BRANCH

            // 10-17 load T from register or immediate
            case 0x10:  T = LIT, P+=2; break;
            case 0x11:  T = va(S); break;
            case 0x12:  T = va(R); break;
            case 0x13:  T = va(P); break;
            case 0x14:  T = va(I); break;
            case 0x15:  T = va(W); break;

            // 18-1F store T to register (aligned) or mem
            case 0x18:  W = ptr(LIT), P+=2, *W = T; break;
            case 0x19:  S = ptr(T & 0xFFFE); break;
            case 0x1A:  R = ptr(T & 0xFFFE); break;
            case 0x1B:  P = ptr(T);          break;
            case 0x1C:  I = ptr(T & 0xFFFE); break;
            case 0x1D:  W = ptr(T & 0xFFFE); break;

            // 20-2F conditional branches, 8-bit signed offset
#define IF(cond)  if (cond) P += 1; else P += *(i8*)P; break;
            case 0x20:  IF(T == 0)
            case 0x21:  IF(T < 0)
            case 0x22:  IF(T > 0)
            case 0X23:  IF(*S == T)
            case 0x24:  IF(*S < T)
            case 0x25:  IF(*S > T)
            case 0x26:  IF((u16)*S < (u16)T) // U<
            case 0x27:  IF((u16)*S > (u16)T) // U>

            // 28-2F are the opposites of 20-27
            case 0x28:  IF(T != 0)
            case 0x29:  IF(T >= 0)
            case 0x2A:  IF(T <= 0)
            case 0X2B:  IF(*S != T)
            case 0x2C:  IF(*S >= T)
            case 0x2D:  IF(*S <= T)
            case 0x2E:  IF((u16)*S >= (u16)T)
            case 0x2F:  IF((u16)*S <= (u16)T)

            // 30-3F stack
            case 0x30:  *--S = T;               break; // DUP
            case 0x31:  pop;                    break; // DROP
            case 0x32:  t = T, T = *S, *S = t;  break; // SWAP
            case 0x33:  push *S;                break; // OVER
            case 0x34:  t = S[1], S[1] = *S, *S = T, T = t; break ; // ROT
            case 0x35:  S++;                    break; // NIP
            case 0x36:  if (T) *--S = T;        break; // ?DUP
            case 0x37:  T = S[T];               break; // PICK

            case 0x38:  *--R = T, pop;          break; // >R
            case 0x39:  push *R++;              break; // R>
            case 0x3A:  push *R;                break; // R@

            // 40-4F double stack
            case 0x40:  t = *S, *--S = T, *--S = t;     break; // 2DUP 
            case 0x41:  pop2;                           break; // 2DROP
            case 0x42:  t = S[0], S[0] = S[2], S[2] = t,
                        t = S[1], S[1] = T, T = t;      break; // 2SWAP
            case 0x43:  t = S[2], *--S = T, *--S = t, T = S[3]; break; // 2OVER
            case 0x44:  *--R = *S++, *--R = T, pop;     break; // 2>R
            case 0x45:  push R[1], push R[0], R += 2;   break; // 2R>
            case 0x46:  push R[1], push R[0];           break; // 2R@

            // 50-5F arithmetic
            case 0x50:  T = *S++ + T; break; // +
            case 0x51:  T = *S++ - T; break; // -
            case 0x52:  T = *S++ * T; break; // *
            case 0x53:  T = *S++ / T; break; // /
            case 0x54:  T = *S++ % T; break; // MOD
            case 0x55:  T = *S++ & T; break; // AND
            case 0x56:  T = *S++ | T; break; // OR
            case 0x57:  T = *S++ ^ T; break; // XOR
            case 0x58:  T = *S++ << T; break; // LSHIFT
            case 0x59:  T = ((u16)*S++) >> T; break; // RSHIFT
            case 0x5A:  T = ~T; break; // INVERT 
            case 0x5B:  T = -T; break; // NEGATE 




            case 0xA0: { // M*/ ( d1 n1 n2 -- d2)
                int64_t d = S[1] << 16 | S[2];
                d = d * S[0] / T;
                S += 2;
                S[0] = d, T = d >> 16;
                break;
            }
            case 0xA1: { // UM* ( u1 u2 -- ud )
                unsigned d = (unsigned)S[0] * (unsigned)T;
                S[0] = d, T = d >> 16;
                break;
            }
            case 0xA2: { // M* ( n1 n2 -- d )
                int d = S[0] * T;
                S[0] = d, T = d >> 16;
                break;
            }
            case 0xA3: { // UM/MOD ( ud u -- rem quot )
                unsigned d = (unsigned)S[0] << 16 | (unsigned)S[1];
                S[0] = d % T, T = d / T;
                break;
            }
            case 0xA4: { // UM* ( u1 u2 -- ud )
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
